using System;
using System.Reflection;
using System.Reflection.Emit;

namespace NAdapter
{
    /// <summary>
    /// Property behavior for a property which has a backing property on the adapted component
    /// </summary>
    internal class PropertyWithBackingComponentProperty : PropertyBehavior
    {
        PropertyInfo _componentProperty;
        Delegate _invokeGetter;
        Type _getterType;
        Delegate _invokeSetter;
        Type _setterType;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="info">The backing component property</param>
        internal PropertyWithBackingComponentProperty(PropertyInfo info)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            _componentProperty = info;
            
            var getter = info.GetGetMethod(true);
            HandleAccessor(getter, typeof(Func<,>).MakeGenericType(info.DeclaringType, info.PropertyType), ref _invokeGetter, ref _getterType);
            
            var setter = info.GetSetMethod(true);
            HandleAccessor(setter, typeof(Action<,>).MakeGenericType(info.DeclaringType, info.PropertyType), ref _invokeSetter, ref _setterType);
        }
        
        /// <summary>
        /// Indicates whether the backing property can be read
        /// </summary>
        internal override bool CanRead => _componentProperty?.CanRead == true;

        /// <summary>
        /// Indicates whether the backing property can be written
        /// </summary>
        internal override bool CanWrite => _componentProperty?.CanWrite == true;
        
        /// <summary>
        /// Emits the IL for getting a backing component property
        /// </summary>
        /// <param name="getterIL">ILGenerator</param>
        /// <param name="sourceField">The field where the source component is stored</param>
        /// <param name="nestedType">Builder for a nested type, in case this property needs to
        /// make one</param>
        /// <param name="thisType">Builder for the current type</param>
        protected override void ProtectedEmitGetter(ILGenerator getterIL, FieldBuilder sourceField, Lazy<TypeSpecifier> nestedType, TypeSpecifier thisType, string name)
        {
            if (_invokeGetter == null)
            {
                getterIL.Emit(OpCodes.Ldarg_0);
                getterIL.Emit(OpCodes.Ldfld, sourceField);
                var meth = _componentProperty.GetGetMethod(true);
                getterIL.Emit(OpCodes.Callvirt, meth);
            }
            else
            {
                var nested = nestedType.Value;
                var backingGetter = nested.TypeBuilder.DefineField(nested.GetBackingGetterIdentifier(name),
                    _getterType,
                    FieldAttributes.Public | FieldAttributes.Static);

                thisType.RunAfterCreation(() =>
                {
                    backingGetter.SafeSetValue(null, _invokeGetter);
                });

                getterIL.Emit(OpCodes.Ldsfld, backingGetter);
                getterIL.Emit(OpCodes.Ldarg_0);
                getterIL.Emit(OpCodes.Ldfld, sourceField);
                getterIL.Emit(OpCodes.Callvirt, _getterType.GetMethod("Invoke"));
            }
        }

        /// <summary>
        /// Emits the IL for setting a backing component property
        /// </summary>
        /// <param name="setterIL">ILGenerator</param>
        /// <param name="sourceField">The field where the source component is stored</param>
        /// <param name="nestedType">Builder for a nested type, in case this property needs to
        /// make one</param>
        /// <param name="thisType">Builder for the current type</param>
        protected override void ProtectedEmitSetter(ILGenerator setterIL, FieldBuilder sourceField, Lazy<TypeSpecifier> nestedType, TypeSpecifier thisType, string name)
        {
            if (_invokeSetter == null)
            {
                setterIL.Emit(OpCodes.Ldarg_0);
                setterIL.Emit(OpCodes.Ldfld, sourceField);
                setterIL.Emit(OpCodes.Ldloc_0);
                setterIL.Emit(OpCodes.Callvirt, _componentProperty.GetSetMethod(true));
            }
            else
            {
                var nested = nestedType.Value;
                var backingSetter = nested.TypeBuilder.DefineField(nested.GetBackingSetterIdentifier(name),
                    _setterType,
                    FieldAttributes.Public | FieldAttributes.Static);

                thisType.RunAfterCreation(() =>
                {
                    backingSetter.SafeSetValue(null, _invokeSetter);
                });

                setterIL.Emit(OpCodes.Ldsfld, backingSetter);
                setterIL.Emit(OpCodes.Ldarg_0);
                setterIL.Emit(OpCodes.Ldfld, sourceField);
                setterIL.Emit(OpCodes.Ldloc_0);
                setterIL.Emit(OpCodes.Callvirt, _setterType.GetMethod("Invoke"));
            }
        }

        void HandleAccessor(MethodInfo info, Type delegateType, ref Delegate accessor, ref Type accessorType)
        {
            if (info != null)
            {
                if (info.IsPublic)
                {
                    accessor = null;
                    accessorType = null;
                }
                else
                {
                    // Necessary because it will throw an exception if our class simply tries
                    // to access a private member directly, so we have to go through a delegate.
                    accessorType = delegateType;
                    accessor = info.CreateDelegate(delegateType);
                }
            }
        }

        protected override Type PropertyType => _componentProperty.PropertyType;
    }
}
