using System;

namespace NAdapter
{
    /// <summary>
    /// Exception to be thrown when a finished specification was finished again.
    /// </summary>
    public class SpecificationAlreadyFinishedException : Exception
    {
        /// <summary>
        /// Constructed.
        /// </summary>
        /// <param name="typeName">Type name</param>
        internal SpecificationAlreadyFinishedException(string typeName) :
            base($"The {typeName} specification has already been finished and cannot be amended.")
        { }
    }
}
