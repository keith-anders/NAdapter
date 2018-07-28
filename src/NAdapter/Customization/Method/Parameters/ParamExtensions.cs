namespace NAdapter
{
    /// <summary>
    /// Extensions for <see cref="NAdapter.ParamSettings"/>
    /// </summary>
    internal static class ParamExtensions
    {
        /// <summary>
        /// Gets a container for the given settings. Null-safe.
        /// </summary>
        /// <typeparam name="T">Type of parameter</typeparam>
        /// <param name="settings">Settings</param>
        /// <returns>Container</returns>
        internal static ParamSettingsContainer OfType<T>(this ParamSettings settings)
            => new ParamSettingsContainer()
            {
                Type = typeof(T),
                Settings = settings
            };
    }
}
