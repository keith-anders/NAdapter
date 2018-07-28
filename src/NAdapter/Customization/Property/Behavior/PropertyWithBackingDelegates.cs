using System;
using System.Reflection;
using System.Reflection.Emit;

namespace NAdapter
{
    /// <summary>
    /// Behavior for a property with delegates to run for getter and setter
    /// </summary>
    /// <typeparam name="TComponent">The type of component being adapted</typeparam>
    /// <typeparam name="TValue">Property type</typeparam>
    internal class PropertyWithBackingDelegates<TComponent, TValue> : PropertyBehavior
    {
        Func<TComponent, TValue> _getter;
        Action<TComponent, TValue> _setter;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Property name</param>
        /// <param name="getter">Delegate for property getter</param>
        /// <param name="setter">Delegate for property setter</param>
        internal PropertyWithBackingDelegates(string name, Func<TComponent, TValue> getter, Action<TComponent, TValue> setter)
        {
            if (getter == null && setter == null)
                throw new InvalidPropertySpecificationException(name, "Cannot have a property with null getter and null setter.");
            _getter = getter;
            _setter = setter;
        }

        /// <summary>
        /// Can be read if the getter delegate exists
        /// </summary>
        internal override bool CanRead => _getter != null;

        /// <summary>
        /// Can be written if the setter delegate exists
        /// </summary>
        internal override bool CanWrite => _setter != null;
        
        /// <summary>
        /// Emits the IL for the getter
        /// </summary>
        /// <param name="getterIL">ILGenerator</param>
        /// <param name="sourceField">The field where the source component is stored</param>
        /// <param name="nestedType">Builder for nested type in case this property needs
        /// to make one</param>
        /// <param name="thisType">Builder for the declaring type</param>
        protected override void ProtectedEmitGetter(ILGenerator getterIL, FieldBuilder sourceField, Lazy<TypeSpecifier> nestedType, TypeSpecifier thisType, string name)
        {
            var backingGetter = nestedType.Value.TypeBuilder.DefineField(
                nestedType.Value.GetBackingGetterIdentifier(name),
                _getter.GetType(),
                FieldAttributes.Public | FieldAttributes.Static);

            nestedType.Value.RunAfterCreation(() =>
            {
                backingGetter.SafeSetValue(null, _getter);
            });
            
            getterIL.Emit(OpCodes.Ldsfld, backingGetter);
            getterIL.Emit(OpCodes.Ldarg_0);
            getterIL.Emit(OpCodes.Ldfld, sourceField);
            getterIL.Emit(OpCodes.Callvirt, typeof(Func<TComponent, TValue>).GetMethod("Invoke"));
        }

        /// <summary>
        /// Emits the IL for the setter
        /// </summary>
        /// <param name="setterIL">ILGenerator</param>
        /// <param name="sourceField">The field where the source component is stored</param>
        /// <param name="nestedType">Builder for nested type in case this property needs
        /// to make one</param>
        /// <param name="thisType">Builder for the declaring type</param>
        protected override void ProtectedEmitSetter(ILGenerator setterIL, FieldBuilder sourceField, Lazy<TypeSpecifier> nestedType, TypeSpecifier thisType, string name)
        {
            var backingSetter = nestedType.Value.TypeBuilder.DefineField(
                nestedType.Value.GetBackingSetterIdentifier(name),
                _setter.GetType(),
                FieldAttributes.Public | FieldAttributes.Static);

            thisType.RunAfterCreation(() =>
            {
                backingSetter.SafeSetValue(null, _setter);
            });
            
            setterIL.Emit(OpCodes.Ldsfld, backingSetter);
            setterIL.Emit(OpCodes.Ldarg_0);
            setterIL.Emit(OpCodes.Ldfld, sourceField);
            setterIL.Emit(OpCodes.Ldloc_0);
            setterIL.Emit(OpCodes.Callvirt, typeof(Action<TComponent, TValue>).GetMethod("Invoke"));
        }

        protected override Type PropertyType => typeof(TValue);
    }
}
