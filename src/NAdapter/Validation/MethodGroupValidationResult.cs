using System;
using System.Collections.Generic;
using System.Linq;

namespace NAdapter
{
    /// <summary>
    /// Method validation result
    /// </summary>
    public class MethodGroupValidationResult : ValidationResult
    {
        List<MethodValidationResult> _overloads = new List<MethodValidationResult>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="methodName">Method name</param>
        /// <param name="returnType">Return type</param>
        internal MethodGroupValidationResult(string methodName) => MethodName = methodName;

        /// <summary>
        /// Method name
        /// </summary>
        public string MethodName { get; private set; }
        
        /// <summary>
        /// The overloads with this method name
        /// </summary>
        public IEnumerable<MethodValidationResult> MethodOverloads { get { return _overloads; } }

        /// <summary>
        /// Method name
        /// </summary>
        internal override string Identifier => MethodName;
     
        /// <summary>
        /// Adds an overload to this method group
        /// </summary>
        /// <param name="returnType">Return type of the overload</param>
        /// <returns>Validation</returns>
        internal MethodValidationResult ValidateOverload(Type returnType)
        {
            var child = new MethodValidationResult(MethodName, returnType);
            _overloads.Add(child);
            RegisterChild(child);
            return child;
        }

        /// <summary>
        /// Finishes the validation of this method group by making sure it does not define
        /// multiple overloads with the same signature.
        /// </summary>
        internal void Finish()
        {
            HashSet<MethodValidationResult> unique = new HashSet<MethodValidationResult>();
            HashSet<MethodValidationResult> duplicates = new HashSet<MethodValidationResult>();
            foreach (var method in _overloads)
                if (!unique.Add(method))
                    duplicates.Add(method);

            foreach (var duplicate in duplicates)
                AddError($"Multiple overloads found with same signature: {duplicate}");
        }
    }

    /// <summary>
    /// Validation of a particular method overload
    /// </summary>
    public class MethodValidationResult : ValidationResult
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Method Name</param>
        /// <param name="returnType">Method return type</param>
        internal MethodValidationResult(string name, Type returnType)
        {
            MethodName = name;
            ReturnType = returnType;
        }

        /// <summary>
        /// Method name
        /// </summary>
        public string MethodName { get; private set; }

        /// <summary>
        /// Return type
        /// </summary>
        public Type ReturnType { get; private set; }
        
        /// <summary>
        /// Parameters on this method
        /// </summary>
        public IEnumerable<ParameterValidationResult> Parameters { get { return Children.OfType<ParameterValidationResult>().Reverse(); } }

        /// <summary>
        /// Subparameters on this method
        /// </summary>
        public IEnumerable<SubparameterValidationResult> Subparameters { get { return Children.OfType<SubparameterValidationResult>(); } }
        
        public override string ToString()
        {
            string paramTypes = String.Join(", ", ParameterTypes);
            return $"{ReturnType} {MethodName}({paramTypes})";
        }

        public override bool Equals(object obj)
        {
            var otherMethod = obj as MethodValidationResult;
            if (otherMethod == null)
                return false;

            if (!otherMethod.MethodName.Equals(MethodName))
                return false;

            return ParameterTypes.SequenceEqual(otherMethod.ParameterTypes);
        }

        public override int GetHashCode()
        {
            int hash = 0;

            hash += 11 * MethodName.GetHashCode() + 13;

            foreach (var type in ParameterTypes)
                hash += 11 * type.GetHashCode() + 13;

            return hash;
        }

        /// <summary>
        /// Method name
        /// </summary>
        internal override string Identifier => MethodName;

        /// <summary>
        /// No need to validate the identifier since the method group was already validated
        /// </summary>
        internal override bool NeedsValidIdentifier => false;

        /// <summary>
        /// Parameter types
        /// </summary>
        internal IEnumerable<Type> ParameterTypes { get { return Parameters.Select(p => p.ParameterType); } }

        /// <summary>
        /// Adds a parameter validation
        /// </summary>
        /// <param name="parameterName">Name</param>
        /// <param name="hasDefaultValue">Parameter has default value</param>
        /// <param name="defaultValue">Parameter default value</param>
        /// <param name="parameterType">Parameter type</param>
        /// <returns>Parameter validation</returns>
        internal ParameterValidationResult ValidateParameter(string parameterName, bool hasDefaultValue, object defaultValue, Type parameterType)
        {
            var child = new ParameterValidationResult(parameterName, hasDefaultValue, defaultValue, parameterType);
            RegisterChild(child);
            return child;
        }

        /// <summary>
        /// Adds a subparameter validation
        /// </summary>
        /// <param name="type">Parameter type</param>
        /// <param name="description">Parameter description</param>
        /// <returns>Subparameter validation</returns>
        internal SubparameterValidationResult ValidateSubparameter(Type type, string description)
        {
            var child = new SubparameterValidationResult(type, description);
            RegisterChild(child);
            return child;
        }
    }
}
