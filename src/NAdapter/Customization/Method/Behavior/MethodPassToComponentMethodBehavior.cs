using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace NAdapter
{
    /// <summary>
    /// Method behavior for a method which is a direct call-through to a component method.
    /// </summary>
    internal class MethodPassToComponentMethodBehavior : MethodBehavior
    {
        MethodInfo _componentMethod;
        string _name;
        Type _componentType;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="methodInfo">The <see cref="System.Reflection.MethodInfo"/> from the
        /// component type to which calls should be passed</param>
        internal MethodPassToComponentMethodBehavior(MethodInfo methodInfo, string name, Type componentType)
        {
            _componentMethod = methodInfo;
            _name = name;
            _componentType = componentType;
        }
        
        /// <summary>
        /// Emits the IL for the method
        /// </summary>
        /// <param name="il">The ILGenerator</param>
        /// <param name="sourceField">The field for the source component</param>
        /// <param name="typeBeingBuilt">The type under construction</param>
        internal override void Emit(ILGenerator il, FieldBuilder sourceField, TypeSpecifier typeBeingBuilt)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, sourceField);
            foreach (int i in Enumerable.Range(1, _componentMethod.GetParameters().Length))
                il.Emit(OpCodes.Ldarg, i);
            il.Emit(OpCodes.Callvirt, _componentMethod);
            il.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Validates this method.
        /// </summary>
        /// <param name="validation">Validation</param>
        internal override void Validate(MethodValidationResult validation)
        {
            if (_componentMethod == null)
                validation.AddError($"{_name} has no component method.");
            else if (_componentMethod.DeclaringType != _componentType)
                validation.AddError($"{_name} is not declared by the type {_componentType}");
        }
    }
}
