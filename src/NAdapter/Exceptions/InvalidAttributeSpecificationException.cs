using System;

namespace NAdapter
{
    /// <summary>
    /// Exception indicating an error was made during the specification
    /// of attributes: an attribute was tried to be added to a type of member
    /// that the attribute's <see cref="System.AttributeUsageAttribute"/> does
    /// not accept.
    /// </summary>
    public class InvalidAttributeSpecificationException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="attributeTypeName">Name of the attribute's type</param>
        /// <param name="acceptable">Acceptable targets</param>
        /// <param name="actual">Actual target</param>
        internal InvalidAttributeSpecificationException(string attributeTypeName, AttributeTargets acceptable, AttributeTargets actual) :
            base($"Cannot add attribute of type {attributeTypeName} to {actual}. Type is only valid on {acceptable}.")
        { }
    }
}
