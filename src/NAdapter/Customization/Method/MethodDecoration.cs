using System;

namespace NAdapter
{
    /// <summary>
    /// Decoration for a method
    /// </summary>
    public class MethodDecoration
    {
        /// <summary>
        /// Method's access modifier
        /// </summary>
        public Access AccessModifier { get; set; }

        /// <summary>
        /// Method's name
        /// </summary>
        public string PublicName { get; set; }

        /// <summary>
        /// Set to true to prevent the method from being created
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// Indicates to make the method virtual
        /// </summary>
        public bool IsVirtual { get; set; }

        /// <summary>
        /// Method's attributes
        /// </summary>
        public AttributeSpecification Attributes { get; } = new AttributeSpecification(AttributeTargets.Method);
    }
}
