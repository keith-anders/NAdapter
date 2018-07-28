using System;
using System.Reflection;

namespace NAdapter
{
    /// <summary>
    /// Param for a property getter
    /// </summary>
    /// <typeparam name="TComponent">Type of component being decorated</typeparam>
    internal class PropertyGetterParam<TComponent> : PropertyParamBase<TComponent> where TComponent : class
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="behavior">Property to get</param>
        /// <param name="type">Property type</param>
        internal PropertyGetterParam(IPropertyBehaviorInternal<TComponent> behavior, Type type) :
            base(behavior, typeof(Func<>).MakeGenericType(type))
        { }

        /// <summary>
        /// Property's Get accessor
        /// </summary>
        /// <returns>Get accessor</returns>
        protected override MethodInfo Accessor() => Behavior.ResultPropertyInfo.GetGetMethod();

        /// <summary>
        /// Delegate type
        /// </summary>
        /// <param name="propertyType">Type of delegate</param>
        /// <returns>Delegate type</returns>
        protected override Type DelegateType(Type propertyType) => typeof(Func<>).MakeGenericType(propertyType);

        public override string ToString() => $"Param Property Getter {Behavior.Name}";
    }
}
