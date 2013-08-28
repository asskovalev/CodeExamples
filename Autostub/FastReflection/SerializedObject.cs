using System;
using System.Collections.Generic;
using System.Collections;

namespace FastReflection
{
    public abstract class SerializedObject
    {
        /// <summary>
        /// The name of the object inside its host type.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The runtime/instance type of this object, if different from the declared type.
        /// </summary>
        public Type Type { get; set; }
    }

    public class SerializedCollection : SerializedObject, IEnumerable<SerializedObject>
    {
        public SerializedCollection()
        {
            Items = new List<SerializedObject>();
        }

        public List<SerializedObject> Items { get; private set; }

        public IEnumerator<SerializedObject> GetEnumerator()
        {
            return Items.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }
    }

    public class SerializedAggregate : SerializedObject
    {
        public SerializedAggregate()
        {
            Children = new Dictionary<object, SerializedObject>();
        }

        public Dictionary<object, SerializedObject> Children { get; private set; }

        public SerializedObject this[object key]
        {
            get { return Children[key]; }
        }
    }

    public class SerializedAtom : SerializedObject
    {
        public object Value { get; set; }
    }

    public class SerializedXml : SerializedObject
    {
        public string Value { get; set; }
    }
}
