using System;

namespace NAdapter
{
    /// <summary>
    /// Validation of a parameter to a backing method or delegate or expression.
    /// </summary>
    public class SubparameterValidationResult : ValidationResult
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parameterType">Type of parameter</param>
        /// <param name="description">Description of parameter behavior</param>
        public SubparameterValidationResult(Type parameterType, string description)
        {
            ParameterType = parameterType;
            ParameterDescription = description;
        }

        /// <summary>
        /// Type of parameter
        /// </summary>
        public Type ParameterType { get; private set; }

        /// <summary>
        /// Description of parameter
        /// </summary>
        public string ParameterDescription { get; private set; }
        
        /// <summary>
        /// Identifier: empty. Subparameters do not need an identifier
        /// </summary>
        internal override string Identifier => String.Empty;

        /// <summary>
        /// False
        /// </summary>
        internal override bool NeedsValidIdentifier => false;

        
    }
}
