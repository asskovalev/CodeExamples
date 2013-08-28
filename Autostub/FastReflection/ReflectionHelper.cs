using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FastReflection
{
    public static class ReflectionHelper
    {
        public static readonly BindingFlags PublicInstanceMembers = BindingFlags.Public | BindingFlags.Instance;

        public static Type GetMemberType(this MemberInfo member)
        {
            if (member is PropertyInfo)
                return (member as PropertyInfo).PropertyType;
            if (member is FieldInfo)
                return (member as FieldInfo).FieldType;
            throw new NotImplementedException();
        }

        public static bool IsHashSet(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(HashSet<>);
        }

        public static bool IsGenericList(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
        }

        public static bool IsGenericCollection(this Type type)
        {
            return type.GetInterfaces()
                .Concat(new[] {type})
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        }

        public static bool IsGenericDictionary(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
        }

        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static bool HasParameterlessConstructor(this Type type)
        {
            return type.GetConstructor(Type.EmptyTypes) != null;
        }
    }
}
