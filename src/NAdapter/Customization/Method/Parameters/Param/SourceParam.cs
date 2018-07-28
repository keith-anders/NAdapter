using System;
using System.Reflection.Emit;

namespace NAdapter
{
    /// <summary>
    /// Parameter representing the component being adapted
    /// </summary>
    /// <typeparam name="T">Type being adapted</typeparam>
    internal class SourceParam<T> : Param<T>
    {
        /// <summary>
        /// Emits the IL for this parameter
        /// </summary>
        /// <param name="il">IL Generator</param>
        /// <param name="sourceField">The field containing the source component</param>
        /// <param name="typeBeingBuilt">The type being built</param>
        internal override void Emit(ILGenerator il, FieldBuilder sourceField, Type typeBeingBuilt)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, sourceField);
        }

        /// <summary>
        /// No-op
        /// </summary>
        /// <param name="validation"></param>
        internal override void Validate(SubparameterValidationResult validation) { }

        public override string ToString() => "Param: Source Component";
    }
}
