namespace NAdapter
{
    /// <summary>
    /// Factory for adapters
    /// </summary>
    /// <typeparam name="TComponent">The type being adapted</typeparam>
    public abstract class AdapterFactory<TComponent> where TComponent : class
    {
        /// <summary>
        /// Creates an adapter
        /// </summary>
        /// <param name="source">The component being adapted</param>
        /// <returns>The adapter</returns>
        public abstract IAdapter<TComponent> Create(TComponent source = null);
    }

    /// <summary>
    /// Factory for a particular type of adapter
    /// </summary>
    /// <typeparam name="TComponent">The type of component being adapted</typeparam>
    /// <typeparam name="TAdapter">The type of adapter</typeparam>
    internal class AdapterFactory<TComponent, TAdapter> : AdapterFactory<TComponent>
        where TComponent : class
        where TAdapter : IAdapter<TComponent>, new()
    {
        /// <summary>
        /// Creates an adapter
        /// </summary>
        /// <param name="source">The component being adapted</param>
        /// <returns>Adapter</returns>
        public override IAdapter<TComponent> Create(TComponent source = null)
            => new TAdapter() { Source = source };
    }
}
