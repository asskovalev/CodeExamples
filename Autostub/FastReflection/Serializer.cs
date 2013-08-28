using System;
using System.Globalization;
using System.Reflection;
using System.Collections;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Runtime.Serialization;
using System.Text;

namespace FastReflection
{
    public class Serializer
    {
        readonly IReflectionProvider _reflectionProvider;
        readonly NetDataContractSerializer serializer;

        public Serializer(IReflectionProvider reflectionProvider)
        {
            _reflectionProvider = reflectionProvider;
            this.serializer = new NetDataContractSerializer("object", string.Empty);

        }

        public SerializedObject Serialize(object instance, Type pubType)
        {
            return SerializeInternal(instance.GetType().Name, instance, pubType);
        }

        SerializedObject SerializeInternal(string name, object instance, Type declaredType)
        {
            SerializedObject child;
            var type = instance == null ? declaredType : instance.GetType();

            // Atomic values
            if (instance == null || IsAtomic(type))
            {
                child = new SerializedAtom { Name = name, Type = type, Value = instance };
            }
            else if (IsXmlType(type))
            {
                using (var sw = new StringWriter())
                using (var xw = XmlWriter.Create(sw, new XmlWriterSettings { ConformanceLevel = ConformanceLevel.Fragment }))
                {
                    if (xw == null)
                    {
                        throw new Exception("SerializeInternal error. xw is null");
                    }

                    xw.WriteStartElement("root");

                    var xmlSerializable = instance as IXmlSerializable;

                    if (xmlSerializable == null)
                    {
                        throw new Exception("SerializeInternal error. xmlSerializable is null");
                    }

                    xmlSerializable.WriteXml(xw);
                    xw.WriteEndElement();

                    xw.Flush();
                    sw.Flush();
                    child = new SerializedXml { Name = name, Type = type, Value = sw.ToString() };
                }

            }
            else if (IsSerializable(type))
            {
                using (var ms = new MemoryStream())
                {
                    serializer.Serialize(ms, instance);
                    ms.Flush();
                    child = new SerializedXml()
                    {
                        Name = name,
                        Type = type,
                        Value = Encoding.Default.GetString(ms.ToArray())
                    };
                }
            }

            // Dictionaries
            else if (type.IsGenericDictionary())
            {
                var dictionary = instance as IDictionary;

                if (dictionary == null)
                {
                    throw new Exception("SerializeInternal error. dictionary is null");
                }

                var genericArguments = dictionary.GetType().GetGenericArguments();
                var keyDeclaredType = genericArguments[0];
                var valueDeclaredType = genericArguments[1];

                child = new SerializedAggregate { Name = name };
                var childAggregation = child as SerializedAggregate;
                foreach (var key in dictionary.Keys)
                    childAggregation.Children.Add(
                        SerializeInternal(null, key, keyDeclaredType),
                        SerializeInternal(null, dictionary[key], valueDeclaredType));
            }

            // Arrays, lists and sets (any collection excluding dictionaries)
            else if (type.IsGenericCollection())
            {
                var collection = instance as IEnumerable;

                if (collection == null)
                {
                    throw new Exception("SerializeInternal error. dictionary is null");
                }

                var declaredItemType = type.IsArray ? type.GetElementType() : type.GetGenericArguments()[0];

                child = new SerializedCollection { Name = name };
                var childCollection = child as SerializedCollection;
                foreach (var item in collection)
                    childCollection.Items.Add(SerializeInternal(null, item, declaredItemType));
            }


            // Everything else (serialized with recursive property reflection)
            else
            {
                child = new SerializedAggregate { Name = name };
                var childAggregation = child as SerializedAggregate;

                foreach (var memberInfo in _reflectionProvider.GetSerializableMembers(type))
                {
                    var memberAttr = _reflectionProvider.GetSingleAttributeOrDefault<SerializationAttribute>(memberInfo);
                    // Make sure we want it serialized
                    if (memberAttr.Ignore)
                        continue;

                    var memberType = memberInfo.GetMemberType();
                    object value = null;
                    try
                    {
                        value = _reflectionProvider.GetValue(memberInfo, instance);
                    }
                    catch
                    {
                    
                    }


                    // Optional properties are skipped when serializing a default or null value
					if (!memberAttr.Required && (value == null || IsDefault(value, (memberInfo as PropertyInfo) != null ? (memberInfo as PropertyInfo).PropertyType : (memberInfo as FieldInfo).FieldType)))
                        continue;

                    // If no property name is defined, use the short type name
                    var memberName = memberAttr.Name ?? memberInfo.Name;
                    childAggregation.Children.Add(memberName, SerializeInternal(memberName, value, memberType));
                }
            }

            // Write the runtime type if different (except nullables since they get unboxed)
            //if (!declaredType.IsNullable() && type != declaredType)
            child.Type = type.HasParameterlessConstructor() ? type : declaredType;

            return child;
        }

        public T Deserialize<T>(SerializedObject instance)
        {
            return (T)DeserializeInternal(instance, typeof(T), null);
        }

        public object DeserializeInternal(SerializedObject serialized, Type declaredType, object existingInstance)
        {
            var type = declaredType;
            var instance = existingInstance;

            // Atomic or null values
            if (serialized is SerializedAtom)
                // The current value is replaced; they're immutable
                instance = (serialized as SerializedAtom).Value;

            else if (serialized is SerializedXml)
            {
                var it = (serialized as SerializedXml);

                if (IsXmlType(declaredType))
                {
                    using (var sr = new StringReader((serialized as SerializedXml).Value))
                    using (var xr = XmlReader.Create(sr, new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment }))
                    {
                        instance = _reflectionProvider.Instantiate(declaredType);

                        var readXml = instance as IXmlSerializable;

                        if (readXml == null)
                        {
                            throw new Exception("DeserializeInternal error. readXml is null");
                        }

                        readXml.ReadXml(xr);
                    }
                }
                else if (IsSerializable(declaredType))
                {
                    using (var ms = new MemoryStream(Encoding.Default.GetBytes(it.Value)))
                    {
                        return serializer.Deserialize(ms);
                    }
                }
            }

            // Dictionaries
            else if (type.IsGenericDictionary())
            {
                // Instantiate if necessary
                if (instance == null)
                    instance = _reflectionProvider.Instantiate(type);

                var dictionary = instance as IDictionary;

                if (dictionary == null)
                {
                    throw new Exception("DeserializeInternal  error. dictionary is null");
                }

                var genericArguments = dictionary.GetType().GetGenericArguments();
                var keyDeclaredType = genericArguments[0];
                var valueDeclaredType = genericArguments[1];

                var serializedAggregation = serialized as SerializedAggregate;

                if (serializedAggregation == null)
                {
                    throw new Exception("DeserializeInternal  error. serializedAggregation is null");
                }

                foreach (var key in serializedAggregation.Children.Keys)
                    // Dictionaries always contain atoms as keys
                    SafeAddToDictionary(dictionary,
                        DeserializeInternal(key as SerializedObject, keyDeclaredType, null),
                        DeserializeInternal(serializedAggregation.Children[key], valueDeclaredType, null));
            }

            // Arrays, lists and sets (any collection excluding dictionaries)
            else if (type.IsGenericCollection())
            {
                var isArray = type.IsArray;
                var isHashSet = type.IsHashSet();
                var serializedCollection = serialized as SerializedCollection;

                if (serializedCollection == null)
                {
                    throw new Exception("DeserializeInternal  error. serializedCollection is null");
                }

                var declaredItemType = type.IsArray ? type.GetElementType() : type.GetGenericArguments()[0];
                var genericListType = typeof(System.Collections.Generic.List<>).MakeGenericType(declaredItemType);
                // Instantiate if necessary
                if (instance == null)
                    if (isArray)
                        instance = Array.CreateInstance(declaredItemType, serializedCollection.Items.Count);
                    else if (type.IsAssignableFrom(genericListType))
                        instance = Activator.CreateInstance(genericListType);
                    else
                    {
                        if (declaredType.HasParameterlessConstructor())
                            instance = _reflectionProvider.Instantiate(declaredType) as IEnumerable;
                        else if (serializedCollection.Type.HasParameterlessConstructor())
                            instance = _reflectionProvider.Instantiate(serializedCollection.Type) as IEnumerable;
                    }


                MethodHandler addToHashSet = null;
                if (isHashSet)
                    addToHashSet = _reflectionProvider.GetDelegate(type.GetMethod("Add"));

                var valueIndex = 0;
                foreach (var item in serializedCollection.Items)
                {
                    var value = DeserializeInternal(item, declaredItemType, null);

                    if (isArray)
                    {
                        var list = instance as IList;
                        if (list == null)
                        {
                            throw new Exception("DeserializeInternal collection error. list is null");
                        }
                        list[valueIndex++] = value;
                    }
                    else if (isHashSet)
                        // Potential problem if set already contains key...
                        addToHashSet(instance, value);
                    else if (instance is IList)
                        (instance as IList).Add(value);
                    else
                        throw new NotImplementedException();
                }
            }

            // Everything else (serialized with recursive property reflection)
            else
            {
                var mustInstantiate = instance == null;

                if (serialized.Type != null && declaredType != serialized.Type)
                {
                    type = serialized.Type;
                    mustInstantiate = true;
                }

                if (mustInstantiate)
                    instance = _reflectionProvider.Instantiate(type);

                var serializedAggregation = serialized as SerializedAggregate;

                if (serializedAggregation == null)
                {
                    throw new Exception("DeserializeInternal collection error. serializedAggregation is null");
                }
                foreach (var memberInfo in _reflectionProvider.GetSerializableMembers(type))
                {
                    var memberAttr = _reflectionProvider.GetSingleAttributeOrDefault<SerializationAttribute>(memberInfo);
                    if (memberAttr.Ignore)
                        continue;

                    var memberType = memberInfo.GetMemberType();
                    var name = memberAttr.Name ?? memberInfo.Name;

                    // Checking if it's a class before doing GetValue doesn't speed up the process
                    var valueFound = serializedAggregation.Children.ContainsKey(name);

                    if (!valueFound)
                    {
                        if (memberAttr.Required)
                            throw new InvalidOperationException(string.Format("MissingRequiredValue {0} {1}", name, type.Name));
                    }
                    else
                    {
                        try
                        {
                            var currentValue = _reflectionProvider.GetValue(memberInfo, instance);
                            var readValue = DeserializeInternal(serializedAggregation[name], memberType, currentValue);
                            // This dirty check is naive and doesn't provide performance benefits
                            //if (memberType.IsClass && readValue != currentValue && (readValue == null || !readValue.Equals(currentValue)))
                            _reflectionProvider.SetValue(memberInfo, instance, readValue);
                        }
                        catch
                        {
                        
                        }
                    }
                }
            }

            return instance;
        }

        static void SafeAddToDictionary(IDictionary dictionary, object key, object value)
        {
            if (dictionary.Contains(key))
                dictionary.Remove(key);
            dictionary.Add(key, value);
        }

        static bool IsXmlType(Type type)
        {
            if (typeof(IXmlSerializable).IsAssignableFrom(type))
                return true;
            return false;
        }

        bool IsSerializable(Type type)
        {
            return (typeof(ISerializable).IsAssignableFrom(type));
        }

        static bool IsAtomic(Type type)
        {
            // Atomic values are immutable and single-valued/can't be decomposed
            return type.IsPrimitive || type.IsEnum || type == typeof(decimal) || type == typeof(string) || type.IsNullable();
        }

        bool IsDefault(object value, Type declType = null)
        {
            var type = value == null ? declType : value.GetType();

            if (type.IsNullable())
                return value == null;

            if (type.IsHashSet())
                return (int)_reflectionProvider.GetValue(type
                    .GetProperty("Count", BindingFlags.ExactBinding | ReflectionHelper.PublicInstanceMembers, null, typeof(int), Type.EmptyTypes, null),
                    value) == 0;
            if (type.IsEnum)
            {
                return (int)value == 0;
            }

            if (value is Array)
                return (value as Array).Length == 0;
            if (value is string)
                return (string)value == string.Empty;
            if (value is char)
                return (char)value == '\0';
            if (value is int || value is long || value is uint || value is float || value is double ||
                value is decimal || value is short || value is sbyte || value is byte || value is ushort)
                return Convert.ToInt64(value) == 0;
            if (value is bool)
                return (bool)value == false;
            //if (value is ICollection)
            //    return (value as ICollection).Count == 0;

            if (value is CultureInfo)
                return true;


            return value.Equals(_reflectionProvider.Instantiate(type));
        }
    }
}
