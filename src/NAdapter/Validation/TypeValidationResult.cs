using System;
using System.Collections.Generic;
using System.Linq;

namespace NAdapter
{
    /// <summary>
    /// Result of validating a type
    /// </summary>
    public class TypeValidationResult : ValidationResult
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="typeName">Type name</param>
        internal TypeValidationResult(string typeName) => TypeName = typeName;
        
        /// <summary>
        /// Type name
        /// </summary>
        public string TypeName { get; private set; }

        /// <summary>
        /// Properties
        /// </summary>
        public IEnumerable<PropertyValidationResult> Properties { get { return Children.OfType<PropertyValidationResult>(); } }

        /// <summary>
        /// Methods
        /// </summary>
        public IEnumerable<MethodGroupValidationResult> MethodGroups { get { return Children.OfType<MethodGroupValidationResult>(); ; } }

        /// <summary>
        /// Type name
        /// </summary>
        internal override string Identifier => TypeName;

        /// <summary>
        /// Adds a property validation
        /// </summary>
        /// <param name="name">Property name</param>
        /// <param name="propertyType">Property type</param>
        /// <param name="getterAccess">Getter access modifier</param>
        /// <param name="setterAccess">Setter access modifier</param>
        /// <returns>Property validation</returns>
        internal PropertyValidationResult ValidateProperty(string name, Type propertyType, Access? getterAccess, Access? setterAccess)
        {
            var child = new PropertyValidationResult(name, propertyType, getterAccess, setterAccess);
            RegisterChild(child);
            return child;
        }

        /// <summary>
        /// Adds a method validation
        /// </summary>
        /// <param name="name">Method name</param>
        /// <returns>Method validation</returns>
        internal MethodGroupValidationResult ValidateMethod(string name)
        {
            var found = MethodGroups.FirstOrDefault(m => m.MethodName == name);
            if (found != null)
                return found;
            found = new MethodGroupValidationResult(name);
            RegisterChild(found);
            return found;
        }
    }
}
