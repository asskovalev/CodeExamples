using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Autostub.Entity.Repository
{
    public class TypeAliasMap : IReadable<TypeAliasMap>, IRenderable
    {
        public const string NodeName = "types";

        private Dictionary<string, TypeAlias> _items;
        private Dictionary<string, TypeAlias> Items
        {
            get
            {
                return _items = _items ?? new Dictionary<string, TypeAlias>();
            }
        }

        public Type this[string name]
        {
            get
            {
                return Items.ContainsKey(name) ? Items[name].Type : null;
            }
            set
            {
                Items[name] = new TypeAlias(name, value);
            }
        }

        public bool IsRegistered(string name)
        {
            return Items.ContainsKey(name);
        }

        public string FindType(Type type)
        {
            var found = Items.Values.FirstOrDefault(t => t.Type == type);
            if (found != null)
                return found.Name;
            return null;
        }

        public string SuggestName(Type type)
        {
            var found = FindType(type);
            if (found != null)
                return found;

            return new[] { GetTypeShortname(type) }
                .Concat(Enumerable.Range(1, 1000)
                    .Select(idx => string.Format("{0}:{1}", GetTypeShortname(type), idx)))
                .First(n => !IsRegistered(n));
        }

        public TypeAliasMap Read(XElement src)
        {

            var items = src.Elements()
                .Select(tm => new TypeAlias().Read(tm))
                .ToList();
            Items.Clear();
            items.ForEach(it => Items.Add(it.Name, it));
            return this;
        }

        public XElement Render()
        {
            return new XElement(NodeName,
                Items
                    .OrderBy(it => it.Key)
                    .Select(it => it.Value.Render())
                    .ToArray());
        }

        private string GetTypeShortname(Type type)
        {
            if (type.IsGenericType)
            {
                var typeArgs = type.GetGenericArguments();
                var typeArgNames = typeArgs.Select(GetTypeShortname).ToList();
                var typeArgString = string.Join(", ", typeArgNames);
                var typeName = type.Name.Split('`')[0];
                return string.Format("{0}[{1}]", typeName, typeArgString);
            }
            return type.Name;
        }
    }
}
