using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace NAdapter
{
    /// <summary>
    /// Settings for a method parameter
    /// </summary>
    public class ParamSettings
    {
        ParamSettings() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="buildFrom">Parameter Info to build this setting from</param>
        internal ParamSettings(ParameterInfo buildFrom): this() => LoadFrom(null, buildFrom);

        /// <summary>
        /// Backing parameter, if any
        /// </summary>
        public ParameterInfo BackingParameterInfo { get; private set; }

        /// <summary>
        /// Attributes on the parameter
        /// </summary>
        public AttributeSpecification Attributes { get; private set; } = new AttributeSpecification(AttributeTargets.Parameter);

        /// <summary>
        /// Specifies the name of the parameter
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <returns>ParamSettings</returns>
        public static ParamSettings WithName(string name) => new ParamSettings() { Name = name };

        /// <summary>
        /// Specifies the default value of the parameter
        /// </summary>
        /// <param name="defaultValue">Default value</param>
        /// <returns>this. For chained method calls.</returns>
        public ParamSettings WithDefault(object defaultValue)
        {
            Default = new DefaultValue() { Value = defaultValue };
            return this;
        }

        /// <summary>
        /// Parameter name
        /// </summary>
        internal string Name { get; set; }

        /// <summary>
        /// Parameter default value
        /// </summary>
        internal DefaultValue Default { get; private set; }
        
        /// <summary>
        /// Builds the parameter
        /// </summary>
        /// <param name="method">The method to receive the parameter</param>
        /// <param name="i">Index of the parameter</param>
        /// <param name="converter">Attributes</param>
        /// <param name="unusedParamName">Index to build the next needed parameter name</param>
        internal void BuildParameter(MethodBuilder method, int i, IAttributeConverter converter, ref int unusedParamName)
        {
            converter = new AttributeConverterAggregate(Attributes, converter);

            string name = Name;
            if (string.IsNullOrEmpty(name))
                name = $"_{++unusedParamName}";

            ParameterBuilder pBuilder;
            if (Default == null)
                pBuilder = method.DefineParameter(i + 1, ParameterAttributes.None, name);
            else
            {
                pBuilder = method.DefineParameter(i + 1, ParameterAttributes.HasDefault | ParameterAttributes.Optional, name);
                pBuilder.SetConstant(Default.Value);
            }

            converter.HandleAttributes(AttributeTargets.Parameter, pBuilder.SetCustomAttribute, BackingParameterInfo != null ? CustomAttributeData.GetCustomAttributes(BackingParameterInfo).ToArray() : new CustomAttributeData[0]);
        }

        /// <summary>
        /// Loads the settings from a <see cref="System.Reflection.ParameterInfo"/>
        /// </summary>
        /// <param name="other">Other settings to load</param>
        /// <param name="info">Other info to load</param>
        internal void LoadFrom(ParamSettings other, ParameterInfo info)
        {
            BackingParameterInfo = info;
            Name = other?.Name ?? Name ?? info.Name;
            Default = other?.Default ?? Default ?? (info.HasDefaultValue ? new DefaultValue() { Value = info.DefaultValue } : null);
            Attributes = other?.Attributes;
        }
    }
}
