﻿using System;
using System.Reflection;

namespace FastReflection
{
    public class DirectReflector : BaseReflector
    {
        public override object Instantiate(Type type)
        {
            if (type.IsArray)
                return Array.CreateInstance(type.GetElementType(), 0);

            return type.GetConstructor(Type.EmptyTypes) == null ? null : Activator.CreateInstance(type);
        }

        public override object GetValue(MemberInfo member, object instance)
        {
            if (member is PropertyInfo)
                return (member as PropertyInfo).GetGetMethod().Invoke(instance, null);
            if (member is FieldInfo)
                return (member as FieldInfo).GetValue(instance);
            throw new NotImplementedException();
        }

        public override void SetValue(MemberInfo member, object instance, object value)
        {
            if (member is PropertyInfo)
                (member as PropertyInfo).GetSetMethod().Invoke(instance, new[] { value });
            else if (member is FieldInfo)
                (member as FieldInfo).SetValue(instance, value);
            else throw new NotImplementedException();
        }

        public override MethodHandler GetDelegate(MethodBase method)
        {
            return (instance, args) => method.Invoke(instance, args);
        }
    }
}
