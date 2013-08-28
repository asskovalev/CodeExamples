using System;

namespace FastReflection
{
    /// <summary>
    /// Qualifies the serialization of a public property or field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    sealed class SerializationAttribute : Attribute
    {
        /// <summary>
        /// Override the serialized name of this value.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Ignore this value when serializing.
        /// </summary>
        public bool Ignore { get; set; }

        public bool Required { get; set; }
    }
}
