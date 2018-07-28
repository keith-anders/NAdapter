using System;

namespace NAdapter
{
    /// <summary>
    /// An error was made during the specification of a parameter
    /// </summary>
    public class InvalidParameterSpecificationException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parameterName">Parameter name</param>
        /// <param name="explanation">Explanation of error</param>
        internal InvalidParameterSpecificationException(string parameterName, string explanation):
            base($"Invalid parameter {parameterName}: {explanation}.")
        { }
    }
}
