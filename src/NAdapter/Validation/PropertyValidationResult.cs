using System;

namespace NAdapter
{
    /// <summary>
    /// Property validation
    /// </summary>
    public class PropertyValidationResult : ValidationResult
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="propertyType">Property type</param>
        /// <param name="getter">Getter access modifier</param>
        /// <param name="setter">Setter access modifier</param>
        internal PropertyValidationResult(string propertyName, Type propertyType, Access? getter, Access? setter)
        {
            PropertyName = propertyName;
            PropertyType = propertyType;
            GetterAccess = getter;
            SetterAccess = setter;
        }

        /// <summary>
        /// Property name
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// Property type
        /// </summary>
        public Type PropertyType { get; private set; }

        /// <summary>
        /// Getter access modifier. Null if getter will not exist.
        /// </summary>
        public Access? GetterAccess { get; private set; }

        /// <summary>
        /// Setter access modifier. Null if setter will not exist.
        /// </summary>
        public Access? SetterAccess { get; private set; }

        /// <summary>
        /// Property name
        /// </summary>
        internal override string Identifier => PropertyName;
    }
}
