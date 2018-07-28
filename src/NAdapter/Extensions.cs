using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace NAdapter
{
    /// <summary>
    /// Extension methods for linq
    /// </summary>
    internal static class LinqExtensions
    {
        /// <summary>
        /// Gets a PropertyInfo described by a property access expression
        /// </summary>
        /// <typeparam name="TSource">Type which declares the property</typeparam>
        /// <typeparam name="TValue">Property type</typeparam>
        /// <param name="expr">Expression describing the property</param>
        /// <returns>PropertyInfo</returns>
        internal static PropertyInfo GetProperty<TSource, TValue>(this Expression<Func<TSource, TValue>> expr)
        {
            PropertyInfo propertyInfo = null;
            if (expr == null)
                throw new ArgumentNullException(nameof(expr));
            MemberExpression memExp = null;
            var body = expr.Body;
            if (body.NodeType == ExpressionType.MemberAccess)
                memExp = body as MemberExpression;
            else if (body.NodeType == ExpressionType.Convert)
            {
                var binding = body as UnaryExpression;
                memExp = binding?.Operand as MemberExpression;
            }

            if (memExp == null)
                throw new ArgumentException("Linq expression did not evaluate to a member accessor.", nameof(expr));

            propertyInfo = memExp.Member as PropertyInfo;
            if (propertyInfo == null)
                throw new ArgumentException("Linq expression did not evaluate to a property accessor.", nameof(expr));

            return propertyInfo;
        }
    }

    /// <summary>
    /// Extensions for building class structure
    /// </summary>
    internal static class ClassStructureExtensions
    {
        /// <summary>
        /// Indicates whether a type represents a compile-time constant.
        /// </summary>
        /// <param name="t">The type to check</param>
        /// <returns>True if constant, false if not</returns>
        internal static bool TypeIsConstant(this Type t) => t != null && (t == typeof(String) || t.IsPrimitive || t.IsEnum);

        /// <summary>
        /// Sets the value of the field corresponding to a <see cref="System.Reflection.Emit.FieldBuilder"/>.
        /// </summary>
        /// <param name="field">The FieldBuilder to set</param>
        /// <param name="toBindTo">The instance to bind to, if an instance field</param>
        /// <param name="value">The value to give the field</param>
        /// <remarks>FieldBuilder.SetValue throws an exception. msdn recommends
        /// searching for the field on the built type manually, thus necessitating
        /// a utility like this.</remarks>
        internal static void SafeSetValue(this FieldBuilder field, object toBindTo, object value)
        {
            var actualField = field.DeclaringType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
                .FirstOrDefault(f => f.Name == field.Name);
            actualField.SetValue(toBindTo, value);
        }

        /// <summary>
        /// Indicates whether attributes of a given attribute class are valid on
        /// a particular code element type.
        /// </summary>
        /// <param name="targets">Target to which the attribute wants to be added</param>
        /// <param name="attributeType">Attribute class</param>
        /// <param name="actualValidOn">Outputs the targets which the
        /// attribute class actually accepts</param>
        /// <returns>True if attribute can be added, false otherwise</returns>
        internal static bool ValidOn(this AttributeTargets targets, Type attributeType, out AttributeTargets actualValidOn)
        {
            var att = attributeType.GetCustomAttributes<AttributeUsageAttribute>(true).FirstOrDefault();
            actualValidOn = att == null ? AttributeTargets.All : att.ValidOn;
            return (actualValidOn & targets) != 0;
        }
    }

    /// <summary>
    /// Extensions for NAdapter types
    /// </summary>
    internal static class NAdapterExtensions
    {
        /// <summary>
        /// Converts a MethodAttributes back into its access modifier for display
        /// in the Validation.
        /// </summary>
        /// <param name="attributes">Attributes to convert</param>
        /// <returns>Access modifier, or null if the member is hidden</returns>
        internal static Access? Convert(this MethodAttributes? attributes)
        {
            if (attributes == null)
                return null;

            var toConvert = attributes.Value;
            if (toConvert.HasFlag(MethodAttributes.Public))
                return Access.Public;
            if (toConvert.HasFlag(MethodAttributes.Family))
                return Access.Protected;
            return Access.Private;
        }

        /// <summary>
        /// Common logic for handling the data on an IAttributeConverter and applying it to a member
        /// </summary>
        /// <param name="converter">The IAttributeConverter whose data is being applied</param>
        /// <param name="targets">The target to which the attribute is being applied</param>
        /// <param name="adder">A function to add an attribute to the member. Necessary because
        /// SetCustomAttribute is not on a common interface on the member Builder types.</param>
        /// <param name="customAttributeData">The data on the attributes to convert</param>
        internal static void HandleAttributes(this IAttributeConverter converter, AttributeTargets targets, Action<CustomAttributeBuilder> adder, CustomAttributeData[] customAttributeData)
        {
            foreach (var attData in customAttributeData)
            {
                var cBuilder = new CustomAttributeBuilderBuilder(attData);
                var att = cBuilder.BuildAttribute();
                if (converter.Convert(cBuilder, true, targets, out CustomAttributeBuilderBuilder aBuilder))
                {
                    if (aBuilder != null)
                        adder(aBuilder.Build());
                }
                else
                    adder(cBuilder.Build());
            }

            foreach (var newAttBuilder in converter.GetNewAttributes())
                adder(newAttBuilder.Build());
        }

        /// <summary>
        /// Parses a Behavior enum into constitutent data
        /// </summary>
        /// <param name="behavior">The behavior to parse</param>
        /// <param name="canGet">True if getting an existing member is allowed by the behavior</param>
        /// <param name="canAdd">True if adding a new member is allowed by the behavior</param>
        /// <param name="throwIfNull">True if the finder is expected to throw an exception
        /// if the member cannot be retrieved in the specified way</param>
        internal static void Parse(this Behavior behavior, out bool canGet, out bool canAdd, out bool throwIfNull)
        {
            canGet = false;
            canAdd = false;
            throwIfNull = false;

            switch (behavior)
            {
                case Behavior.Add: canAdd = true; return;
                case Behavior.AddOrGet: canAdd = true; canGet = true; return;
                case Behavior.AddOrThrow: canAdd = true; throwIfNull = true; return;
                case Behavior.Get: canGet = true; return;
                case Behavior.GetOrThrow: canGet = true; throwIfNull = true; return;
                default: throw new ArgumentException(string.Format("Unrecognized behavior: {0}", behavior), nameof(behavior));
            }
        }

        /// <summary>
        /// Converts to a MethodAttributes
        /// </summary>
        /// <param name="access">The Access enum to convert</param>
        /// <returns>MethodAttributes</returns>
        internal static MethodAttributes Convert(this Access access)
        {
            switch (access)
            {
                case Access.Public: return MethodAttributes.Public;
                case Access.Protected: return MethodAttributes.Family;
                case Access.Private: return MethodAttributes.Private;
                default: return default(MethodAttributes);
            }
        }
    }

    /// <summary>
    /// Extensions for delegates
    /// </summary>
    internal static class DelegateExtensions
    {
        /// <summary>
        /// Gets the type of a delegate which returns void and takes the given types as paramters
        /// </summary>
        /// <param name="types">The parameter types</param>
        /// <returns>Delegate type</returns>
        internal static Type GetActionWithGenerics(this Type[] types)
        {
            Type delType = null;
            switch (types.Length)
            {
                case 0: delType = typeof(Action); break;
                case 1: delType = typeof(Action<>); break;
                case 2: delType = typeof(Action<,>); break;
                case 3: delType = typeof(Action<,,>); break;
                case 4: delType = typeof(Action<,,,>); break;
                case 5: delType = typeof(Action<,,,,>); break;
                case 6: delType = typeof(Action<,,,,,>); break;
                case 7: delType = typeof(Action<,,,,,,>); break;
                case 8: delType = typeof(Action<,,,,,,,>); break;
                case 9: delType = typeof(Action<,,,,,,,,>); break;
                case 10: delType = typeof(Action<,,,,,,,,,>); break;
                case 11: delType = typeof(Action<,,,,,,,,,,>); break;
                case 12: delType = typeof(Action<,,,,,,,,,,,>); break;
                case 13: delType = typeof(Action<,,,,,,,,,,,,>); break;
                case 14: delType = typeof(Action<,,,,,,,,,,,,,>); break;
                case 15: delType = typeof(Action<,,,,,,,,,,,,,,>); break;
                case 16: delType = typeof(Action<,,,,,,,,,,,,,,,>); break;
                default: throw new ArgumentException("Can only handle up to 16 types in an action.");
            }
            return delType.MakeGenericType(types);
        }

        /// <summary>
        /// Gets the type of a delegate with a given non-void return type and takes the given types
        /// as parameters
        /// </summary>
        /// <param name="types">Parameter types</param>
        /// <param name="returnType">The return type</param>
        /// <returns>Delegate type</returns>
        internal static Type GetFuncWithGenerics(this Type[] types, Type returnType)
        {
            if (returnType == null || returnType == typeof(void))
                throw new ArgumentException("Func cannot return void.", nameof(returnType));
            types = types.Concat(new[] { returnType }).ToArray();
            Type delType = null;
            switch (types.Length)
            {
                case 1: delType = typeof(Func<>); break;
                case 2: delType = typeof(Func<,>); break;
                case 3: delType = typeof(Func<,,>); break;
                case 4: delType = typeof(Func<,,,>); break;
                case 5: delType = typeof(Func<,,,,>); break;
                case 6: delType = typeof(Func<,,,,,>); break;
                case 7: delType = typeof(Func<,,,,,,>); break;
                case 8: delType = typeof(Func<,,,,,,,>); break;
                case 9: delType = typeof(Func<,,,,,,,,>); break;
                case 10: delType = typeof(Func<,,,,,,,,,>); break;
                case 11: delType = typeof(Func<,,,,,,,,,,>); break;
                case 12: delType = typeof(Func<,,,,,,,,,,,>); break;
                case 13: delType = typeof(Func<,,,,,,,,,,,,>); break;
                case 14: delType = typeof(Func<,,,,,,,,,,,,,>); break;
                case 15: delType = typeof(Func<,,,,,,,,,,,,,,>); break;
                case 16: delType = typeof(Func<,,,,,,,,,,,,,,,>); break;
                default: throw new ArgumentException("Can only handle up to 16 types in a func.");
            }
            return delType.MakeGenericType(types);
        }
    }
}
