using System;

namespace NAdapter
{
    /// <summary>
    /// Exception thrown when a behavior of AddOrThrow or GetOrThrow is specified
    /// and the specified means of member retrieval cannot be performed.
    /// </summary>
    public class UnexpectedMemberFindBehaviorException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Message to display</param>
        internal UnexpectedMemberFindBehaviorException(string message) : base(message) { }
    }
}
