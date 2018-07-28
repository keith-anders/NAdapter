using System;
using System.Reflection.Emit;

namespace NAdapter
{
    /// <summary>
    /// Decorator for a param, to allow behavior from weakly typed Param subclasses
    /// to be passed to places where Param of T is required.
    /// </summary>
    /// <typeparam name="T">Parameter type</typeparam>
    internal class ParamDecorator<T> : Param<T>
    {
        Param _base;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="toDecorate">Parameter to decorate</param>
        internal ParamDecorator(Param toDecorate)
        {
            _base = toDecorate;
            if (_base.Type != typeof(T))
                throw new ArgumentException("Mismatched type.", nameof(toDecorate));
        }

        public override string ToString() => _base.ToString();

        /// <summary>
        /// Validates this parameter
        /// </summary>
        /// <param name="paramSettings">Settings to validate against</param>
        internal override void Validate(ParamSettingsContainer[] paramSettings) => _base.Validate(paramSettings);

        /// <summary>
        /// Validates this parameter
        /// </summary>
        /// <param name="validation">Validation</param>
        internal override void Validate(SubparameterValidationResult validation) => _base.Validate(validation);

        /// <summary>
        /// Emits IL to load the value required by this parameter onto the evaluation stack
        /// </summary>
        /// <param name="il">ILGenerator</param>
        /// <param name="sourceField">Field containing the source component</param>
        /// <param name="typeBeingBuilt">Type being built</param>
        internal override void Emit(ILGenerator il, FieldBuilder sourceField, Type typeBeingBuilt) => _base.Emit(il, sourceField, typeBeingBuilt);
    }
}
