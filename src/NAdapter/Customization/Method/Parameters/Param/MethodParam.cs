using System;
using System.Linq;
using System.Reflection.Emit;

namespace NAdapter
{
    /// <summary>
    /// Parameter representing a method call
    /// </summary>
    /// <typeparam name="TComponent">Type of component being decorated</typeparam>
    internal class MethodParam<TComponent> : Param where TComponent : class
    {
        IMethodBehavior<TComponent> _behavior;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="behavior">Method to call</param>
        /// <param name="type">Type of delegate</param>
        internal MethodParam(IMethodBehavior<TComponent> behavior, Type type)
        {
            Type = type;
            _behavior = behavior;
        }

        public override string ToString() => $"Param Method Call {_behavior}";

        /// <summary>
        /// Type of delegate
        /// </summary>
        internal override Type Type { get; }
                
        /// <summary>
        /// Emits IL to load a pointer to this method onto the evaluation stack
        /// </summary>
        /// <param name="il">IL generator</param>
        /// <param name="sourceField">The field containing the source component</param>
        /// <param name="typeBeingBuilt">The type being built</param>
        internal override void Emit(ILGenerator il, FieldBuilder sourceField, Type typeBeingBuilt)
        {
            var ctors = _behavior.DelegateType.GetConstructors();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldftn, _behavior.BuiltMethodInfo);
            il.Emit(OpCodes.Newobj, _behavior.DelegateType.GetConstructors().Single());
        }

        /// <summary>
        /// Validates this parameter
        /// </summary>
        /// <param name="validation">Validation</param>
        internal override void Validate(SubparameterValidationResult validation)
        {
            if (_behavior == null)
                validation.AddError("Null behavior.");
            else if (_behavior.DelegateType != Type)
                validation.AddError("Method no longer of correct type.");
            else if (_behavior.Decoration.IsHidden)
                validation.AddError("Cannot call hidden method.");
        }
    }
}
