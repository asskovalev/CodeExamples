using System;
using System.Xml.Linq;

namespace Autostub.Entity.Repository
{
    public class TypeAlias : IReadable<TypeAlias>, IRenderable
    {
        private const string NodeName = "alias";
        private const string TypeName = "type";
        private const string AliasName = "name";

        public Type Type { get; set; }
        public string Name { get; set; }

        public TypeAlias() { }

        public TypeAlias(string name, Type type)
            : this()
        {
            Name = name;
            Type = type;
        }

        public TypeAlias Read(XElement src)
        {
            var atrAliasName = src.Attribute(AliasName);
            var atrTypeName = src.Attribute(TypeName);

            if (atrAliasName == null || atrTypeName == null)
            {
                throw new Exception(
                    string.Format(
                        "CallInfo error. XElement AliasName or TypeName is null./n atrAliasName is null {0}./n atrTypeName is null = {1}",
                        atrAliasName == null, atrTypeName == null));
            }
            var name = atrAliasName.Value;
            var typeName = atrTypeName.Value;

            var type = Type.GetType(typeName);

            Name = name;
            Type = type;

            return this;
        }

        public XElement Render()
        {
            return new XElement(NodeName,
                new XAttribute(AliasName, Name),
                new XAttribute(TypeName, Type.AssemblyQualifiedName));
        }
    }
}
