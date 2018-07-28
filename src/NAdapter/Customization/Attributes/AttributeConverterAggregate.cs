using System;
using System.Collections.Generic;
using System.Linq;

namespace NAdapter
{
    /// <summary>
    /// Aggregator for prioritizing multiple IAttributeConverters
    /// </summary>
    internal class AttributeConverterAggregate: IAttributeConverter
    {
        IAttributeConverter[] _components;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="components">The component converters</param>
        internal AttributeConverterAggregate(params IAttributeConverter[] components) => _components = components;
        
        IEnumerable<CustomAttributeBuilderBuilder> IAttributeConverter.GetNewAttributes() => _components.FirstOrDefault()?.GetNewAttributes() ?? new CustomAttributeBuilderBuilder[0];

        bool IAttributeConverter.Convert(CustomAttributeBuilderBuilder a, bool isThisMemberType, AttributeTargets targets, out CustomAttributeBuilderBuilder customAttributeBuilder)
        {
            customAttributeBuilder = null;
            for (int i = 0; i < _components.Length; ++i)
            {
                var converter = _components[i];
                if (converter != null && converter.Convert(a, isThisMemberType && i == 0, targets, out customAttributeBuilder))
                    return true;
            }

            customAttributeBuilder = a;
            return true;
        }
    }
}
