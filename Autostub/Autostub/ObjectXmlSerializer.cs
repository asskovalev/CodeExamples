using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Autostub.Entity.Repository;
using FastReflection;

namespace Autostub
{
    internal class ObjectXmlSerializer
    {
        const string Type = "type";
        const string Value = "value";

        const string Class = "struct";
        const string Atom = "atom";
        const string Collection = "list";
        const string Xml = "xml";

        const string Items = "items";
        const string Member = "member";
        const string KeySimple = "key";
        const string KeyComposite = "key-composite";

        private static readonly Type[] Convertible =
            {
                typeof(int),
                typeof(uint),
                typeof(long),
                typeof(ulong),
                typeof(double),
                typeof(float),
                typeof(byte),
                typeof(sbyte),
                typeof(string),
                typeof(bool),
                typeof(decimal),
                typeof(DateTime),
                typeof(char),
                typeof(short),
                typeof(ushort)
            };

        TypeAliasMap Map { get; set; }

        public ObjectXmlSerializer(TypeAliasMap map)
        {
            Map = map;
        }

        public XElement ToXml(SerializedObject instance)
        {
            if (instance is SerializedAtom) return ToXml(instance as SerializedAtom);
            if (instance is SerializedAggregate) return ToXml(instance as SerializedAggregate);
            if (instance is SerializedCollection) return ToXml(instance as SerializedCollection);
            if (instance is SerializedXml) return ToXml(instance as SerializedXml);
            throw new InvalidOperationException();
        }

        public SerializedObject ToObject(XElement src)
        {
            if (src.Name.LocalName == Atom) return GetAtom(src);
            if (src.Name.LocalName == Collection) return GetCollection(src);
            if (src.Name.LocalName == Class) return GetAggregate(src);
            if (src.Name.LocalName == Xml) return GetXmlValue(src);
            throw new InvalidOperationException();
        }

        XElement ToXml(SerializedCollection instance)
        {
            return new XElement(Collection,
                ToXmlType(instance),
                new XElement(Items, instance.Items
                    .Select(ToXml)
                    .ToArray()));
        }

        XElement ToXml(SerializedAggregate instance)
        {
            return new XElement(Class,
                ToXmlType(instance),
                instance.Children
                    .Select(child =>
                    {
                        return new XElement(Member,
                            child.Key is string ? new XElement(KeySimple, child.Key as string) :
                            child.Key is SerializedObject ? new XElement(KeyComposite, ToXml(child.Key as SerializedObject))
                                : null,
                            ToXml(child.Value));
                    })
                    .ToArray());
        }

        XElement ToXml(SerializedXml instance)
        {
            return new XElement(Xml,
                ToXmlType(instance),
                XElement.Parse(instance.Value));
        }

        XElement ToXml(SerializedAtom instance)
        {
            return new XElement(Atom,
                ToXmlType(instance),
                Convert.ChangeType(instance.Value, typeof(string)));
        }

        XAttribute ToXmlType(SerializedObject instance)
        {
            var typeAlias = Map.SuggestName(instance.Type);
            Map[typeAlias] = instance.Type;
            return instance.Type != null ? new XAttribute(Type, typeAlias) : null;
        }




        SerializedCollection GetCollection(XElement src)
        {
            var type = FromXmlType(src);
            var items = src.Element(Items).Elements()
                .Select(ToObject)
                .ToList();

            var result = new SerializedCollection()
            {
                //Name = name,
                Type = type
            };
            result.Items.AddRange(items);
            return result;
        }

        SerializedAtom GetAtom(XElement src)
        {
            var type = FromXmlType(src);
            var _value = src.Value;
            object value = null;

            if (type.IsEnum)
            {
                var enumValue = Enum.Parse(type, _value, false);
                value = Convert.ChangeType(enumValue, type.GetEnumUnderlyingType());
            }

            else if (Convertible.Contains(type))
                value = Convert.ChangeType(_value, type);

            else if (type.IsNullable() && Convertible.Contains(type.GetGenericArguments()[0]) && !(string.IsNullOrEmpty(_value)))
                value = Convert.ChangeType(_value, type.GetGenericArguments()[0]);

            return new SerializedAtom()
            {
                Type = type,
                Value = value
            };
        }

        SerializedAggregate GetAggregate(XElement src)
        {
            var type = FromXmlType(src);
            var members = src.Elements(Member)
                .Select(el =>
                {
                    var key = el.Elements(KeySimple).Count() == 1
                        ? (object)el.Element(KeySimple).Value
                        : (object)ToObject(el.Element(KeyComposite).Elements().First());
                    var value = ToObject(el.Elements().ElementAt(1));
                    return new KeyValuePair<object, SerializedObject>(key, value);
                })
                .ToList();

            var result = new SerializedAggregate()
            {
                //Name = name,
                Type = type
            };
            members.ForEach(member => result.Children.Add(member.Key, member.Value));
            return result;
        }

        SerializedXml GetXmlValue(XElement src)
        {
            var type = FromXmlType(src);
            var value = src.Elements().First().ToString();
            return new SerializedXml()
            {
                Type = type,
                Value = value
            };
        }

        Type FromXmlType(XElement src)
        {
            return Map[src.Attribute(Type).Value];
        }

    }
}
