namespace NAdapter
{
    /// <summary>
    /// An adapter
    /// </summary>
    /// <typeparam name="T">The type being adapted</typeparam>
    public interface IAdapter<T> where T : class
    {
        /// <summary>
        /// The component being adapted
        /// </summary>
        T Source { get; set; }
    }
}
