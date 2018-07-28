using System;

namespace NAdapter.Test
{
    public class AdapterTestConvertedAttribute : Attribute
    {
        public AdapterTestConvertedAttribute() { }

        public AdapterTestConvertedAttribute(string title) => Value = title;

        public string Value { get; private set; }
    }
}
