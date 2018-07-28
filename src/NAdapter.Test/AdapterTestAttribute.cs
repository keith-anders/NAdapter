using System;

namespace NAdapter.Test
{
    public class AdapterTestAttribute : Attribute
    {
        string _name;

        public AdapterTestAttribute() { }

        public AdapterTestAttribute(string name) => _name = name;

        public string Name { get => _name; }
    }
}
