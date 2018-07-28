using System;
using System.Reflection;
using System.Reflection.Emit;

namespace NAdapter
{
    /// <summary>
    /// Behavior of a method which passes information to a delegate
    /// </summary>
    internal class MethodFromDelegateBehavior : MethodBehavior
    {
        Delegate _invoke;
        Param[] _params;
        string _name;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="invoke">The delegate to invoke</param>
        /// <param name="name">Method name</param>
        /// <param name="paramTypes">The parameters for the delegate</param>
        internal MethodFromDelegateBehavior(Delegate invoke, string name, Param[] paramTypes)
        {
            _name = name;
            _invoke = invoke;
            _params = paramTypes;
        }

        /// <summary>
        /// Emits the IL for this method
        /// </summary>
        /// <param name="il">IL Generator</param>
        /// <param name="sourceField">The field containing the source component</param>
        /// <param name="typeBeingBuilt">The type being built</param>
        internal override void Emit(ILGenerator il, FieldBuilder sourceField, TypeSpecifier typeBeingBuilt)
        {
            var nestedType = typeBeingBuilt.CreateDisplayClass();
            var backingField = nestedType.TypeBuilder.DefineField(nestedType.GetInvokeName(_name),
                _invoke.GetType(),
                FieldAttributes.Static | FieldAttributes.Public);

            typeBeingBuilt.RunAfterCreation(() =>
            {
                backingField.SafeSetValue(null, _invoke);
            });
            
            il.Emit(OpCodes.Ldsfld, backingField);

            foreach (var p in _params)
                p.Emit(il, sourceField, typeBeingBuilt.TypeBuilder);

            il.Emit(OpCodes.Callvirt, _invoke.GetType().GetMethod("Invoke"));
            il.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Validates this method
        /// </summary>
        /// <param name="validation">Validation</param>
        internal override void Validate(MethodValidationResult validation)
        {
            foreach (var p in _params)
                p.Validate(validation.ValidateSubparameter(p.Type, p.ToString()));
            if (_invoke == null)
                validation.AddError($"{_name} has no backing delegate.");
        }
    }
}
