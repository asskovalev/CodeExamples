using System;
using System.Linq;
using System.Xml.Linq;
using Autostub.Entity.Repository;
using FastReflection;

namespace Autostub
{
    internal static class SerializationHelper
    {
        public static object Deserialize(XElement src, Type type, TypeAliasMap map)
        {
            return DeserializeValue(src.Elements().First(), type, map);
        }

        public static object DeserializeValue(XElement src, Type type, TypeAliasMap map)
        {
            if (IsVoidMethod(type))
            {
                return null;
            }

            if (src.Name.LocalName == "Null")
            {
                return null;
            }
            var serializer = new Serializer(new DirectReflector());
            var serializedObject = new ObjectXmlSerializer(map).ToObject(src);
            return serializer.DeserializeInternal(serializedObject, serializedObject.Type, null);
        }

        public static XElement Serialize(object instance, Type type, string name, TypeAliasMap map)
        {
            return new XElement(name, SerializeValue(instance, type, map));
        }

        private static XElement SerializeValue(object instance, Type type, TypeAliasMap map)
        {

            if (IsVoidMethod(type))
            {
                return new XElement("Void");
            }

            if (instance == null)
            {
                return new XElement("Null");
            }
            var serializer = new Serializer(new DirectReflector());
            var serializedObject = serializer.Serialize(instance, type);

            var serializeValue = new ObjectXmlSerializer(map).ToXml(serializedObject);
            return serializeValue;
        }

        private static bool IsVoidMethod(Type type)
        {
            return type.Name == "Void";
        }
    }
}
