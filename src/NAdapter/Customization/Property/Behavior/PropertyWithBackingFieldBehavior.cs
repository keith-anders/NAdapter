using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace NAdapter
{
    /// <summary>
    /// Behavior for a property with a backing field
    /// </summary>
    /// <typeparam name="TField">Property and field type</typeparam>
    internal class PropertyWithBackingFieldBehavior<TField> : PropertyBehavior
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Property name</param>
        internal PropertyWithBackingFieldBehavior() { }

        /// <summary>
        /// Can be read
        /// </summary>
        internal override bool CanRead => true;

        /// <summary>
        /// Can be written
        /// </summary>
        internal override bool CanWrite => true;
        
        /// <summary>
        /// Emits the IL for a getter
        /// </summary>
        /// <param name="getterIL">ILGenerator</param>
        /// <param name="sourceField">The field where the source component is stored</param>
        /// <param name="nestedType">Builder for a nested type in case this property needs to make one</param>
        /// <param name="thisType">Builder for the declaring type</param>
        protected override void ProtectedEmitGetter(ILGenerator getterIL, FieldBuilder sourceField, Lazy<TypeSpecifier> nestedType, TypeSpecifier thisType, string name)
        {
            BackingField = thisType.TypeBuilder.DefineField(thisType.GetBackingFieldIdentifier(name), typeof(TField), FieldAttributes.Private);
            BackingField.SetCustomAttribute(Specification<string>.GeneratedCode);

            getterIL.Emit(OpCodes.Ldarg_0);
            getterIL.Emit(OpCodes.Ldfld, BackingField);
        }

        /// <summary>
        /// Emits the IL for a setter
        /// </summary>
        /// <param name="setterIL">ILGenerator</param>
        /// <param name="sourceField">The field where the source component is stored</param>
        /// <param name="nestedType">Builder for a nested type in case this property needs to make one</param>
        /// <param name="thisType">Builder for the declaring type</param>
        protected override void ProtectedEmitSetter(ILGenerator setterIL, FieldBuilder sourceField, Lazy<TypeSpecifier> nestedType, TypeSpecifier thisType, string name)
        {
            setterIL.Emit(OpCodes.Ldarg_0);
            setterIL.Emit(OpCodes.Ldloc_0);
            setterIL.Emit(OpCodes.Stfld, BackingField);
        }

        protected override Type PropertyType => typeof(TField);
    }
}
