using System;

namespace NAdapter
{
    /// <summary>
    /// An error was made during the specification of a type
    /// </summary>
    public class InvalidTypeSpecificationException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="errors">The errors found in the specification</param>
        internal InvalidTypeSpecificationException(string[] errors) :
            base(string.Format("Errors while validating type: {0}", String.Join("\r\n", errors)))
        { }
    }
}
