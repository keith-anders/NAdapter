namespace NAdapter
{
    /// <summary>
    /// A user-defined specification for the adapter
    /// </summary>
    /// <typeparam name="TComponent">The component to adapt</typeparam>
    public interface ISpecification<TComponent> where TComponent : class
    {
        /// <summary>
        /// Will be called for each property in the specification before the class
        /// is built
        /// </summary>
        /// <param name="behavior">Property behavior</param>
        void OnProperty<TValue>(PropertyBehavior<TComponent, TValue> behavior);

        /// <summary>
        /// Will be called for each method in the specification before the class
        /// is built
        /// </summary>
        /// <param name="behavior">Method behavior</param>
        void OnMethod<TBehavior>(TBehavior behavior) where TBehavior : MethodBehaviorBase<TComponent, TBehavior>;

        /// <summary>
        /// Will be called once after all methods and properties have been intercepted.
        /// Offers a chance to add any additional members whose need may have been
        /// discovered during interception, but the whole featureset of the
        /// <see cref="NAdapter.Specification{TComponent}"/> is available.
        /// </summary>
        /// <param name="specification">Specification</param>
        void OnMembersFinished(Specification<TComponent> specification);
    }
}
