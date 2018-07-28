using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace NAdapter
{
    /// <summary>
    /// Base for parameters that represent property accessors
    /// </summary>
    /// <typeparam name="TComponent">The type of component being decorated</typeparam>
    internal abstract class PropertyParamBase<TComponent> : Param where TComponent : class
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="behavior">Property behavior to access</param>
        /// <param name="type">Type of property</param>
        internal PropertyParamBase(IPropertyBehaviorInternal<TComponent> behavior, Type type)
        {
            Type = type;
            Behavior = behavior;
        }

        /// <summary>
        /// Type to return
        /// </summary>
        internal override Type Type { get; }
        
        /// <summary>
        /// Emits the IL for this parameter
        /// </summary>
        /// <param name="il">IL generator</param>
        /// <param name="sourceField">The field containing the source component</param>
        /// <param name="typeBeingBuilt">The type being built</param>
        internal override void Emit(ILGenerator il, FieldBuilder sourceField, Type typeBeingBuilt)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldftn, Accessor());
            il.Emit(OpCodes.Newobj, DelegateType(Behavior.PropertyType).GetConstructors().Single());
        }

        /// <summary>
        /// Validates this parameter
        /// </summary>
        /// <param name="validation">Validation</param>
        internal override void Validate(SubparameterValidationResult validation)
        {
            if (Behavior == null)
                validation.AddError("Null behavior");
            {
                Behavior = Behavior.GetFinalDescendant();
                if (Type != DelegateType(Behavior.PropertyType))
                    validation.AddError("Property no longer of type.");
                else if (Behavior.Decoration.IsHidden)
                    validation.AddError("Cannot get hidden property.");
            }
        }

        /// <summary>
        /// Property behavior
        /// </summary>
        protected IPropertyBehaviorInternal<TComponent> Behavior { get; private set; }

        /// <summary>
        /// Accessor (get or set)
        /// </summary>
        /// <returns>MethodInfo of the accessor</returns>
        protected abstract MethodInfo Accessor();

        /// <summary>
        /// Type of the delegate of this accessor
        /// </summary>
        /// <param name="propertyType">Type of property</param>
        /// <returns>Delegate type</returns>
        protected abstract Type DelegateType(Type propertyType);
    }
}
