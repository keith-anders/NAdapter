using System;
using System.Text;

namespace NAdapter
{
    /// <summary>
    /// Container for the settings related to a method parameter
    /// </summary>
    public class ParamSettingsContainer
    {
        /// <summary>
        /// Parameter type
        /// </summary>
        public Type Type { get; internal set; }

        /// <summary>
        /// Parameter settings
        /// </summary>
        public ParamSettings Settings { get; internal set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Type);
            
            if (Settings != null)
            {
                sb.Append(" ");
                sb.Append(Settings.Name);

                if (Settings.Default != null)
                {
                    sb.Append(" = ");
                    sb.Append(Settings.Default.Value);
                }
            }

            return sb.ToString();
        }
    }
}
