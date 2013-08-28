using System.Collections.Generic;

namespace FastReflection
{
    public class Entity
    {
        #region Atoms

        public string AtomicField;
        public float AtomicProperty { get; set; }

        public double? Nullable { get; set; }
        public EntityKind Enumeration { get; set; }

        #endregion

        #region Aggregates

        public Entity AggregateClass { get; set; }
        public SomeStruct AggregateStruct { get; set; }
        public object Polymorphic { get; set; }

        [Serialization(Required = true)]
        public Entity Required { get; set; }

        [Serialization(Name = "AnotherName")]
        public Entity Renamed { get; set; }

        #endregion

        #region Collections

        public List<string> AtomicList { get; set; }
        public List<Entity> AggregateList { get; set; }
        public string[] AtomicArray { get; set; }
        public Entity[] AggregateArray { get; set; }
        public HashSet<string> AtomicSet { get; set; }
        public HashSet<Entity> AggregateSet { get; set; }
        public Dictionary<string, int> AtomicDictionary { get; set; }
        public Dictionary<string, Entity> HybridDictionary { get; set; }
        public Dictionary<Entity, Entity> AggregateDictionary { get; set; }

        #endregion

        #region Non-Serializable Members

        int PrivateField;
        protected int ProtectedField;
        internal int InternalField;

        [Serialization(Ignore = true)]
        public int Ignored { get; set; }

        public int NonWritableAuto { get; private set; }
        public int NonReadableAuto { private get; set; }

        int nonReadable;
        public int NonReadable
        {
            set { nonReadable = value; }
        }

        int nonWritable;
        public int NonWritable
        {
            get { return nonReadable; }
        }

        readonly Dictionary<int, string> indexer = new Dictionary<int, string>();
        public string this[int index]
        {
            get { return indexer[index]; }
            set { indexer[index] = value; }
        }

        #endregion
    }

    public enum EntityKind { None, Fooish, Barish }
    public struct SomeStruct { public int A, B; }
}