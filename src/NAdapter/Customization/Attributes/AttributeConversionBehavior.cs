using System;

namespace NAdapter
{
    [Flags]
    public enum AttributeConversionBehavior
    {
        None = 1,
        
        /// <summary>
        /// Convert members of this type only. Do not apply this conversion
        /// to child members.
        /// </summary>
        ThisMemberTypeOnly = 2,

        /// <summary>
        /// Convert members of this attribute type only. Do not apply this conversion
        /// to derived classes.
        /// </summary>
        ThisAttributeTypeOnly = 4
    }
}
