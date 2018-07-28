using System;
using System.Reflection;

namespace NAdapter
{
    /// <summary>
    /// Decoration for a property
    /// </summary>
    public class PropertyDecoration
    {
        PropertyBehavior _behavior;
        MethodAttributes? _getterAccess;
        MethodAttributes? _setterAccess;

        /// <summary>
        /// The property's least restrictive access modifier
        /// </summary>
        public Access AccessModifier { get; set; }

        /// <summary>
        /// Name of the property
        /// </summary>
        public string PublicName { get; set; }

        /// <summary>
        /// Set to true in order to prevent the property from being created
        /// </summary>
        public bool IsHidden { get; set; }
        
        /// <summary>
        /// Property should be virtual
        /// </summary>
        public bool IsVirtual { get; set; }

        /// <summary>
        /// Property's attributes
        /// </summary>
        public AttributeSpecification Attributes { get; set; } = new AttributeSpecification(AttributeTargets.Property);

        /// <summary>
        /// Sets the property to be readonly
        /// </summary>
        public void SpecifyReadOnly()
        {
            GetterAccess = AccessModifier.Convert();
            SetterAccess = null;
        }

        /// <summary>
        /// Sets the property's getter to be private
        /// </summary>
        public void SpecifyPrivateGetter() => GetterAccess = MethodAttributes.Private;

        /// <summary>
        /// Sets the property's getter to the property's access modifier
        /// </summary>
        public void SpecifyPublicGetter() => GetterAccess = AccessModifier.Convert();

        /// <summary>
        /// Sets the property to be writeonly
        /// </summary>
        public void SpecifyWriteOnly()
        {
            GetterAccess = null;
            SetterAccess = AccessModifier.Convert();
        }

        /// <summary>
        /// Sets the property's setter to be private
        /// </summary>
        public void SpecifyPrivateSetter() => SetterAccess = MethodAttributes.Private;

        /// <summary>
        /// Sets the property's setter to the property's access modifier
        /// </summary>
        public void SpecifyPublicSetter() => SetterAccess = AccessModifier.Convert();

        /// <summary>
        /// Sets the property to be both readable and writable
        /// </summary>
        public void SpecifyReadWrite()
        {
            GetterAccess = AccessModifier.Convert();
            SetterAccess = AccessModifier.Convert();
        }

        /// <summary>
        /// Whether a getter should be created
        /// </summary>
        internal bool HasGetter => GetterAccess != null;

        /// <summary>
        /// Whether a setter should be created
        /// </summary>
        internal bool HasSetter => SetterAccess != null;

        /// <summary>
        /// Property behavior which determines whether the property is readable and/or writeable.
        /// </summary>
        internal PropertyBehavior Behavior
        {
            private get { return _behavior; }
            set
            {
                _behavior = value;
                if (!value.CanRead)
                    GetterAccess = null;
                else
                    GetterAccess = AccessModifier.Convert();

                if (!value.CanWrite)
                    SetterAccess = null;
                else
                    SetterAccess = AccessModifier.Convert();
            }
        }

        /// <summary>
        /// Access modifier for the getter, or null if no getter
        /// </summary>
        internal MethodAttributes? GetterAccess
        {
            get { return _getterAccess; }
            private set
            {
                if (value == null || Behavior.CanRead)
                    _getterAccess = value;
                else
                    throw new InvalidPropertySpecificationException(PublicName, "Property behavior is unreadable, so property cannot be set to readable.");
            }
        }
        
        /// <summary>
        /// Access modifier for the setter, or null if no setter
        /// </summary>
        internal MethodAttributes? SetterAccess
        {
            get { return _setterAccess; }
            private set
            {
                if (value == null || Behavior.CanWrite)
                    _setterAccess = value;
                else
                    throw new InvalidPropertySpecificationException(PublicName, "Property behavior is unwriteable, so property cannot be set to writeable.");
            }
        }
    }
}
