using System;
using System.Collections.Generic;

namespace NAdapter
{
    /// <summary>
    /// Converter for attributes
    /// </summary>
    internal interface IAttributeConverter
    {
        /// <summary>
        /// Converts an attribute
        /// </summary>
        /// <param name="builder">The attribute builder to convert</param>
        /// <param name="isThisMemberType">Whether the attributes being converted are those
        /// for the specific member to which this converter was applied</param>
        /// <param name="targets">Targets to which the attribute is being applied</param>
        /// <param name="aBuilder">The builder which was created, or null if the attribute is to be hidden</param>
        /// <returns>True if a conversion was found, false if not</returns>
        bool Convert(CustomAttributeBuilderBuilder builder, bool isThisMemberType, AttributeTargets targets, out CustomAttributeBuilderBuilder aBuilder);

        /// <summary>
        /// Gets the new attributes that are to be added
        /// </summary>
        /// <returns>Attribute builders for the new attributes</returns>
        IEnumerable<CustomAttributeBuilderBuilder> GetNewAttributes();
    }
}
