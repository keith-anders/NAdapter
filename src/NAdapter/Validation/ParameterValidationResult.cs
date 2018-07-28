using System;

namespace NAdapter
{
    /// <summary>
    /// Parameter validation
    /// </summary>
    public class ParameterValidationResult : ValidationResult
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parameterName">Parameter name</param>
        /// <param name="hasDefaultValue">Has default value</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="parameterType">Parameter type</param>
        internal ParameterValidationResult(string parameterName, bool hasDefaultValue, object defaultValue, Type parameterType)
        {
            ParameterName = parameterName;
            HasDefaultValue = hasDefaultValue;
            DefaultValue = defaultValue;
            ParameterType = parameterType;
        }

        /// <summary>
        /// Has default value
        /// </summary>
        public bool HasDefaultValue { get; private set; }

        /// <summary>
        /// Default value
        /// </summary>
        public object DefaultValue { get; private set; }

        /// <summary>
        /// Parameter type
        /// </summary>
        public Type ParameterType { get; private set; }

        /// <summary>
        /// Parameter name
        /// </summary>
        public string ParameterName { get; private set; }

        /// <summary>
        /// Parameter name
        /// </summary>
        internal override string Identifier => ParameterName;
    }
}
