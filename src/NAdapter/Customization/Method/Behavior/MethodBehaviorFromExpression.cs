using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace NAdapter
{
#if NETFRAMEWORK
    /// <summary>
    /// Behavior of a method defined from a LINQ expression
    /// </summary>
    /// <typeparam name="TComponent">Type of component being adapted</typeparam>
    internal class MethodBehaviorFromExpression<TComponent> : MethodBehavior where TComponent : class
    {
        LambdaExpression _expr;
        string _name;
        Param[] _params;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="expr">Lambda expression upon which this method is based</param>
        /// <param name="name">Name of the method</param>
        /// <param name="parameters">Parameters for the method</param>
        internal MethodBehaviorFromExpression(LambdaExpression expr, string name, Param[] parameters)
        {
            _expr = expr;
            _name = name;
            _params = parameters;
        }
        
        /// <summary>
        /// Emits the IL for this method. Adds a backing static method to which the expression is compiled,
        /// then calls that method with appropriate arguments
        /// </summary>
        /// <param name="il">IL generator for the current method</param>
        /// <param name="sourceField">Field representing the source component</param>
        /// <param name="typeBeingBuilt">The type being built</param>
        internal override void Emit(ILGenerator il, FieldBuilder sourceField, TypeSpecifier typeBeingBuilt)
        {
            var pTypes = _expr.Parameters.Select(p => p.Type).ToArray();
            var nestedClass = typeBeingBuilt.CreateDisplayClass();
            var lambda = nestedClass.TypeBuilder.DefineMethod(nestedClass.GetMethodName(_name),
                MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.Public,
                _expr.ReturnType,
                pTypes);

            // We'd really like to compile this straight into our method,
            // but `LambdaExpression.CompileToMethod` only takes static methods,
            // so the next best thing is to use this attribute to tell the JITter
            // that it should be inlined.
            lambda.SetCustomAttribute(new CustomAttributeBuilder(
                typeof(MethodImplAttribute).GetConstructor(new Type[] { typeof(MethodImplOptions) }),
                new object[1] { MethodImplOptions.AggressiveInlining }));

            _expr.CompileToMethod(lambda);

            foreach (var p in _params)
                p.Emit(il, sourceField, typeBeingBuilt.TypeBuilder);

            il.Emit(OpCodes.Call, lambda);
            il.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Validates thie method
        /// </summary>
        /// <param name="validation">Validation</param>
        internal override void Validate(MethodValidationResult validation)
        {
            foreach (var p in _params)
                p.Validate(validation.ValidateSubparameter(p.Type, p.ToString()));
            if (_expr == null)
                validation.AddError($"{_name} has no linq expression.");
        }
    }
#endif
}
