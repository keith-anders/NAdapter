using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NAdapter
{
    /// <summary>
    /// Container for logic related to converting attributes
    /// </summary>
    public class AttributeSpecification: IAttributeConverter
    {
        /// <summary>
        /// Converter for an attribute
        /// </summary>
        class Converter
        {
            Type _atributeTypeToConvert;
            Func<CustomAttributeBuilderBuilder, CustomAttributeBuilderBuilder> _conversion;
            AttributeConversionBehavior _behavior;
            List<Converter> _children;
            
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="attributeType">Type of attribute to convert</param>
            /// <param name="conversion">Conversion delegate</param>
            /// <param name="behavior">Conversion behavior</param>
            internal Converter(Type attributeType, Func<CustomAttributeBuilderBuilder, CustomAttributeBuilderBuilder> conversion, AttributeConversionBehavior behavior)
            {
                _atributeTypeToConvert = attributeType;
                _conversion = conversion;
                _behavior = behavior;
                _children = new List<Converter>();
            }
            
            /// <summary>
            /// Adds a converter
            /// </summary>
            /// <param name="converter">Child converter</param>
            internal void Add(Converter converter)
            {
                if (converter._atributeTypeToConvert == _atributeTypeToConvert)
                {
                    _conversion = converter._conversion;
                    _behavior = converter._behavior;
                    foreach (Converter child in converter._children)
                        Add(child);
                }
                else
                {
                    Type t = converter._atributeTypeToConvert;
                    Converter child = _children.FirstOrDefault(c => c._atributeTypeToConvert.IsAssignableFrom(t));
                    if (child == null)
                    {
                        var grandChild = _children.FirstOrDefault(c => t.IsAssignableFrom(c._atributeTypeToConvert));
                        if (grandChild != null)
                        {
                            _children.Add(converter);
                            converter._children.Add(grandChild);
                            _children.Remove(grandChild);
                        }
                        else
                            _children.Add(converter);
                    }
                    else
                        child.Add(converter);
                }
            }
            
            /// <summary>
            /// Converts an attribute
            /// </summary>
            /// <param name="a">Builder to convert</param>
            /// <param name="isThisMemberType">Whether the conversion is to be applied
            /// to the member type it was defined on</param>
            /// <param name="currentTarget">The member type to be applied on</param>
            /// <param name="builder">Output builder</param>
            /// <returns>True if converted, false if could not convert</returns>
            internal bool Convert(CustomAttributeBuilderBuilder a, bool isThisMemberType, AttributeTargets currentTarget, out CustomAttributeBuilderBuilder builder)
            {
                var att = a.BuildAttribute();
                var converter = GetConverter(att.GetType(), isThisMemberType);
                var convertFunc = converter?._conversion;
                if (convertFunc != null)
                {
                    builder = convertFunc(a);
                    if (builder != null && !currentTarget.ValidOn(builder.AttributeType, out _))
                        builder = null;
                }
                else
                    builder = null;

                return convertFunc != null;
            }
            
            Converter GetConverter(Type t, bool isThisMemberType)
            {
                if (!_atributeTypeToConvert.IsAssignableFrom(t))
                    return null;
                if (t == _atributeTypeToConvert && (isThisMemberType || !_behavior.HasFlag(AttributeConversionBehavior.ThisMemberTypeOnly)))
                    return this;
                return _children.Select(c => c.GetConverter(t, isThisMemberType)).FirstOrDefault(c => c != null)
                    ?? (_behavior.HasFlag(AttributeConversionBehavior.ThisAttributeTypeOnly) ? null :
                        (_behavior.HasFlag(AttributeConversionBehavior.ThisMemberTypeOnly) && !isThisMemberType ? null : this));
            }
        }

        AttributeTargets _validTargets;
        Converter _attributeConverter = new Converter(typeof(Attribute), null, AttributeConversionBehavior.None);
        List<CustomAttributeBuilderBuilder> _builders = new List<CustomAttributeBuilderBuilder>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="validTargets">Target of this member type</param>
        internal AttributeSpecification(AttributeTargets validTargets) => _validTargets = validTargets;
        
        /// <summary>
        /// Sets an attribute to be added
        /// </summary>
        /// <typeparam name="T">The type of attribute to be added</typeparam>
        /// <param name="expr">An expression resolving to the type of attribute to be added</param>
        public void AddAttribute<T>(Expression<Func<T>> expr) where T : Attribute
        {
            if (expr == null)
                throw new ArgumentNullException(nameof(expr));

            if (_validTargets.ValidOn(typeof(T), out AttributeTargets actualValidOn))
                _builders.Add(AttributeExpressionParser.Parse(expr).Get(null));
            else
                throw new InvalidAttributeSpecificationException(typeof(T).FullName, actualValidOn, _validTargets);
        }

        /// <summary>
        /// Hides attributes of a given type
        /// </summary>
        /// <typeparam name="TOriginal">The type of attribute to hide</typeparam>
        /// <param name="behavior">The behavior to apply to the conversion</param>
        public void HideAttributesOfType<TOriginal>(AttributeConversionBehavior behavior = AttributeConversionBehavior.None) where TOriginal : Attribute
            => _attributeConverter.Add(new Converter(typeof(TOriginal), a => null, behavior));

        /// <summary>
        /// Registers an expression for converting an attribute to a different attribute
        /// </summary>
        /// <typeparam name="T">The type of attribute to be converted</typeparam>
        /// <param name="conversion">The expression detailing the means of conversion</param>
        /// <param name="behavior">The behavior to apply to the conversion</param>
        public void RegisterAttributeConversion<T>(Expression<Func<T, Attribute>> conversion, AttributeConversionBehavior behavior = AttributeConversionBehavior.None) where T : Attribute
        {
            var parser = AttributeExpressionParser.Parse(conversion);
            var converter = new Converter(typeof(T), a => parser.Get(a), behavior);
            _attributeConverter.Add(converter);
        }

        IEnumerable<CustomAttributeBuilderBuilder> IAttributeConverter.GetNewAttributes() => _builders;

        bool IAttributeConverter.Convert(CustomAttributeBuilderBuilder a, bool isThisMemberType, AttributeTargets targets, out CustomAttributeBuilderBuilder customAttributeBuilder)
            => _attributeConverter.Convert(a, isThisMemberType, targets, out customAttributeBuilder);
    }
}
