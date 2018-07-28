using System;
using System.Reflection;

namespace NAdapter
{
    /// <summary>
    /// Param for a property setter
    /// </summary>
    /// <typeparam name="TComponent">Type of component being decorated</typeparam>
    internal class PropertySetterParam<TComponent> : PropertyParamBase<TComponent> where TComponent : class
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="behavior">Property to access</param>
        /// <param name="type">Type of property</param>
        internal PropertySetterParam(IPropertyBehaviorInternal<TComponent> behavior, Type type) :
            base(behavior, typeof(Action<>).MakeGenericType(type))
        { }

        /// <summary>
        /// Set accessor
        /// </summary>
        /// <returns>Set accessor</returns>
        protected override MethodInfo Accessor() => Behavior.ResultPropertyInfo.GetSetMethod();

        /// <summary>
        /// Delegate type for the set accessor
        /// </summary>
        /// <param name="propertyType">Property type</param>
        /// <returns>Delegate type</returns>
        protected override Type DelegateType(Type propertyType) => typeof(Action<>).MakeGenericType(propertyType);

        public override string ToString()
        {
            return $"Param Property Setter {Behavior.Name}";
        }
    }
}
