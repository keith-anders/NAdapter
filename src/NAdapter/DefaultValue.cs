using System;

namespace NAdapter
{
    /// <summary>
    /// Container for a default value
    /// </summary>
    public class DefaultValue
    {
        object _value;

        /// <summary>
        /// The default value
        /// </summary>
        public object Value
        {
            get { return _value; }
            set
            {
                if (value == null || value.GetType().TypeIsConstant())
                    _value = value;
                else
                    throw new ArgumentException("A parameter's default value must be a constant.", nameof(value));
            }
        }
    }
}
