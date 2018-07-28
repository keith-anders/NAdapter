using System;

namespace NAdapter
{
    /// <summary>
    /// An error was made during the specification of a proprety
    /// </summary>
    public class InvalidPropertySpecificationException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="explanation">Explanation of the error</param>
        internal InvalidPropertySpecificationException(string propertyName, string explanation):
            base($"Invalid property {propertyName}: {explanation}")
        { }
    }
}
