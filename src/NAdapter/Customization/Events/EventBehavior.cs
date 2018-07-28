using System.Reflection;

namespace NAdapter
{
    /// <summary>
    /// Event behavior. Placeholder for future version.
    /// </summary>
    /// <typeparam name="TComponent">Type of component being adapted</typeparam>
    internal class EventBehavior<TComponent> where TComponent : class
    {
        EventInfo _info;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="info">Event info</param>
        internal EventBehavior(EventInfo info) => _info = info;

        /// <summary>
        /// Performs validation for the event
        /// </summary>
        /// <param name="validation"></param>
        internal void Validate(TypeValidationResult validation)
        {
            if (_info != null)
                validation.AddWarning($"Event {_info.Name} will be skipped. That feature will be added in {Roadmap.Features.Events.Version()}.");
        }
    }
}
