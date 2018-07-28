using System;
using System.Reflection.Emit;

namespace NAdapter
{
    /// <summary>
    /// Parameter for a delegate representing a pass-through of an argument from the adapter method
    /// </summary>
    internal class ArgumentParam : Param
    {
        int _index;
        bool _set = false;
        string _name = null;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="index">The 1-based index of the argument from the adapter method</param>
        /// <param name="type">Type of parameter</param>
        internal ArgumentParam(int index, Type type)
        {
            _index = index;
            _set = true;
            Type = type;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The name of the argument from the adapter method</param>
        /// <param name="type">Type of parameter</param>
        internal ArgumentParam(string name, Type type)
        {
            _name = name;
            _set = false;
            Type = type;
        }

        public override string ToString() => $"Param: Argument at {_index}.";

        /// <summary>
        /// Type of parameter
        /// </summary>
        internal override Type Type { get; }

        /// <summary>
        /// Sets the index if only a name is known so far, and ensures the index is
        /// in a valid range.
        /// </summary>
        /// <param name="paramSettings">The method parameter settings containers</param>
        internal override void Validate(ParamSettingsContainer[] paramSettings)
        {
            if (!_set)
            {
                for (int i = 0; i < paramSettings.Length; ++i)
                    if (paramSettings[i].Settings.Name == _name)
                        _index = i + 1;
            }

            if (_index < 1 || _index > paramSettings.Length)
                throw new InvalidParameterSpecificationException(_name, $"Index {_index} out of bounds");
            if (!Type.IsAssignableFrom(paramSettings[_index - 1].Type))
                throw new InvalidParameterSpecificationException(_name, $"Type {paramSettings[_index - 1].Type} cannot be converted to {Type}.");
        }

        /// <summary>
        /// No-op
        /// </summary>
        /// <param name="validation">Validation</param>
        internal override void Validate(SubparameterValidationResult validation) { }

        /// <summary>
        /// Emits the IL for this parameter
        /// </summary>
        /// <param name="il">IL Generator</param>
        /// <param name="sourceField">Field containing the source component</param>
        /// <param name="typeBeingBuilt">The type being built</param>
        internal override void Emit(ILGenerator il, FieldBuilder sourceField, Type typeBeingBuilt)
        {
            il.Emit(OpCodes.Ldarg, _index);
        }
    }
}
