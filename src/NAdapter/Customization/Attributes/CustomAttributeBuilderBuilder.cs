using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace NAdapter
{
    /// <summary>
    /// Builder for a <see cref="System.Reflection.Emit.CustomAttributeBuilder"/>
    /// </summary>
    internal class CustomAttributeBuilderBuilder
    {
        ConstructorInfo _ctor;
        List<object> _args = new List<object>();
        List<FieldInfo> _fields = new List<FieldInfo>();
        List<object> _fieldValues = new List<object>();
        List<PropertyInfo> _properties = new List<PropertyInfo>();
        List<object> _propertyValues = new List<object>();
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public CustomAttributeBuilderBuilder() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="data">CustomAttributeData on which to base this builder</param>
        internal CustomAttributeBuilderBuilder(CustomAttributeData data)
        {
            SetConstructor(data.Constructor);
            foreach (var arg in data.ConstructorArguments)
                AddConstructorArg(ConvertToArrayIfNecessary(arg));

            foreach (var arg in data.NamedArguments)
            {
                if (arg.IsField)
                    AddField(arg.MemberInfo as FieldInfo, ConvertToArrayIfNecessary(arg.TypedValue));
                else
                    AddProperty(arg.MemberInfo as PropertyInfo, ConvertToArrayIfNecessary(arg.TypedValue));
            }
        }

        /// <summary>
        /// Adds a constructor argument
        /// </summary>
        /// <param name="o">The argument to add</param>
        public void AddConstructorArg(object o) => _args.Add(o);

        /// <summary>
        /// Adds a field
        /// </summary>
        /// <param name="field">Field</param>
        /// <param name="value">Field value</param>
        public void AddField(FieldInfo field, object value)
        {
            _fields.Add(field);
            _fieldValues.Add(value);
        }

        /// <summary>
        /// Adds a property
        /// </summary>
        /// <param name="prop">Property</param>
        /// <param name="value">Property value</param>
        public void AddProperty(PropertyInfo prop, object value)
        {
            _properties.Add(prop);
            _propertyValues.Add(value);
        }

        /// <summary>
        /// Sets the constructor
        /// </summary>
        /// <param name="ctor">Constructor</param>
        public void SetConstructor(ConstructorInfo ctor) => _ctor = ctor;

        /// <summary>
        /// Builds an attribute class from the current builder state
        /// </summary>
        /// <returns>Attribute</returns>
        public Attribute BuildAttribute()
        {
            var att = (Attribute)_ctor.Invoke(_args.ToArray());
            for (int i = 0; i < _fields.Count; ++i)
                _fields[i].SetValue(att, _fieldValues[i]);
            for (int i = 0; i < _properties.Count; ++i)
                _properties[i].SetValue(att, _propertyValues[i]);
            return att;
        }

        /// <summary>
        /// Builds a <see cref="System.Reflection.Emit.CustomAttributeBuilder"/>
        /// </summary>
        /// <returns>CustomAttributeBuilder</returns>
        public CustomAttributeBuilder Build()
        {
            return new CustomAttributeBuilder(_ctor,
                _args.ToArray(),
                _properties.ToArray(),
                _propertyValues.ToArray(),
                _fields.ToArray(),
                _fieldValues.ToArray());
        }

        /// <summary>
        /// Type of attribute
        /// </summary>
        internal Type AttributeType { get { return _ctor.DeclaringType; } }

        static T[] ConvertToArrayOfT<T>(ReadOnlyCollection<CustomAttributeTypedArgument> collection)
        {
            return collection.Select(arg => (T)ConvertToArrayIfNecessary(arg)).ToArray();
        }

        static MethodInfo ConvertTemplate = typeof(CustomAttributeBuilderBuilder)
            .GetMethod(nameof(ConvertToArrayOfT), BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { typeof(ReadOnlyCollection<CustomAttributeTypedArgument>) }, null);

        static object ConvertToArray(Type argType, ReadOnlyCollection<CustomAttributeTypedArgument> readonlyCollection)
        {
            return ConvertTemplate.MakeGenericMethod(argType).Invoke(null, new object[] { readonlyCollection });
        }

        static object ConvertToArray(CustomAttributeTypedArgument data)
        {
            var argType = data.ArgumentType.GetElementType();
            if (data.Value is ReadOnlyCollection<CustomAttributeTypedArgument> collection)
                return ConvertToArray(argType, collection);
            else if (data.Value == null)
                return null;
            else
                throw new ArgumentException("Non-array lists are invalid in attribute initializers.", nameof(data));
        }

        static object ConvertToArrayIfNecessary(CustomAttributeTypedArgument data)
        {
            if (data.ArgumentType.IsArray)
                return ConvertToArray(data);
            return data.Value;
        }
    }
}
