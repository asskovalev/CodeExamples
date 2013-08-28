using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Autostub.Entity;
using Autostub.Entity.Entity.Call;
using Autostub.Entity.Repository;
using FastReflection;

namespace Autostub.Entity.Call
{
    public class CallParameterInfo : IReadable<CallParameterInfo>, IRenderable
    {
        private const string ArgName = "name";
        private const string TypeName = "type";

        public ParameterType ParameterType { get; set; }

        public string Name { get; set; }
        public string TypeAlias { get; set; }
        public XElement Value { get; set; }

        private TypeAliasMap TypeMap { get; set; }

        public CallParameterInfo(TypeAliasMap typemap)
        {
            TypeMap = typemap;
        }

        #region methods
        private static Type GetPublicType(Type t, object v)
        {
            // a.kovalev: если нет беспараметрического конструктора, но есть реализация IList
            //            нужно для корректного глушения CachedCollection
            if (!t.HasParameterlessConstructor() && t.GetInterfaces()
                .Concat(new[] { t })
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>)))
                return typeof(IList<>).MakeGenericType(
                    t.IsArray
                        ? new[] { t.GetElementType() }
                        : t.GetGenericArguments());

            return (!t.IsPublic) && v != null
                ? v.GetType()
                : t;
        }

        public CallParameterInfo AssignValue<T>(T value)
        {
            return AssignValue(typeof(T), value);
        }

        public CallParameterInfo AssignValue(Type type, object value)
        {
            var pubType = GetPublicType(type, value);

            Value = SerializationHelper.Serialize(value, pubType, "value", TypeMap);

            var typeAlias = TypeMap.FindType(pubType);
            if (string.IsNullOrEmpty(typeAlias))
            {
                typeAlias = TypeMap.SuggestName(pubType);
                TypeMap[typeAlias] = pubType;
            }
            TypeAlias = typeAlias;
            return this;
        }

        public object ExtractValue()
        {
            var type = TypeMap[TypeAlias];
            return SerializationHelper.Deserialize(Value, type, TypeMap);
        }

        #endregion

        public CallParameterInfo Read(XElement src)
        {

            var atrArgName = src.Attribute(ArgName);
            var atrTypeName = src.Attribute(TypeName);

            if (atrArgName == null || atrTypeName == null)
            {
                throw new Exception(
                    string.Format(
                        "CallInfo error. XElement ArgName or TypeName is null./n atrArgName is null {0}./n atrTypeName is null = {1}",
                        atrArgName == null, atrTypeName == null));
            }
            var name = atrArgName.Value;
            var typeName = atrTypeName.Value;

            ParameterType paramType;
            Enum.TryParse(src.Name.LocalName, out paramType);

            var value = new XElement("value", src.Elements());

            Name = name;
            TypeAlias = typeName;
            Value = value;
            ParameterType = paramType;

            return this;
        }

        public XElement Render()
        {
            return new XElement(ParameterType.ToString(),
                new XAttribute(ArgName, Name),
                new XAttribute(TypeName, TypeAlias),
                Value.Elements());
        }

    }
}
