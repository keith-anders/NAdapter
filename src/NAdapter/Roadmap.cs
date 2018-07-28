using System;
using System.Linq;
using System.Reflection;

namespace NAdapter
{
    /// <summary>
    /// Product roadmap
    /// </summary>
    internal static class Roadmap
    {
        /// <summary>
        /// Indicates the version associated with a given release
        /// </summary>
        [AttributeUsage(AttributeTargets.Field)]
        class ReleaseAttribute : Attribute
        {
            /// <summary>
            /// Version
            /// </summary>
            internal string Version { get; private set; }

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="version">Version</param>
            internal ReleaseAttribute(string version) => Version = version;
        }

        /// <summary>
        /// Planned features, ordered by release
        /// </summary>
        internal enum Features
        {
            DotNet45,
            AttributesOnPropertyGetter,
            AttributesOnPropertySetter,
            IndexProperties,
            AttributesOnIndexPropertyParameters,
            AttributesOnReturnTypes,
            RefParams,
            OutParams,
            GenericMethods,
            AttributesOnGenericTypes,
            [Release("0.1.1")]
            Version011Release,
            ConfigurableNullBehavior,
            [Release("0.1.2")]
            Version012Release,
            Events,
            AttributesOnEvents,
            StaticMethods,
            StaticProperties,
            [Release("0.1.3")]
            Version013Release
        }

        /// <summary>
        /// Gets the earliest version in which the given feature is planned to be released
        /// </summary>
        /// <param name="feature">The feature to search for</param>
        /// <returns>Earliest version which will contain that feature</returns>
        internal static string Version(this Features feature)
        {
            return (from f in Enum.GetValues(typeof(Features)).Cast<Features>()
                    let fName = Enum.GetName(typeof(Features), f)
                    let version = typeof(Features).GetField(fName).GetCustomAttributes<ReleaseAttribute>().FirstOrDefault()?.Version
                    where version != null && f > feature
                    select version).Min();
        }
    }
}
