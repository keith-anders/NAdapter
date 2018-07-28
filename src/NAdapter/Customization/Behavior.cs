namespace NAdapter
{
    /// <summary>
    /// Behavior to employ when specifying a member
    /// </summary>
    public enum Behavior
    {
        /// <summary>
        /// Add a new member or else return null
        /// </summary>
        Add,

        /// <summary>
        /// Add a new member or else get an existing member
        /// </summary>
        AddOrGet,

        /// <summary>
        /// Add a new member or else throw an UnexpectedMemberFindBehaviorException
        /// </summary>
        AddOrThrow,

        /// <summary>
        /// Get an existing member or else return null
        /// </summary>
        Get,

        /// <summary>
        /// Get an existing member or else throw an UnexpectedMemberFindBehaviorException
        /// </summary>
        GetOrThrow
    }
}
