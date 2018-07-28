using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NAdapter
{
    /// <summary>
    /// Container for property behaviors
    /// </summary>
    /// <typeparam name="TComponent">The component being adapted</typeparam>
    internal class PropertiesSpecification<TComponent> : IEnumerable<IPropertyBehaviorInternal<TComponent>> where TComponent : class
    {
        static PropertiesSpecification()
            => _actualProperties = (from p in typeof(TComponent).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static)
                                 group p by p.Name)
                                 .ToDictionary(k => k.Key, k => k.ToArray());

        static Dictionary<string, PropertyInfo[]> _actualProperties = new Dictionary<string, PropertyInfo[]>();

        List<PropertyBehavior<TComponent>> _properties = new List<PropertyBehavior<TComponent>>();

        /// <summary>
        /// Constructor.
        /// </summary>
        internal PropertiesSpecification()
        {
            _properties.AddRange(_actualProperties.Values.SelectMany(p => p).Select(p =>
            {
                var propertyBehavior = Make(p.Name, p.GetAccessors().Any() ? Access.Public : Access.Private, p.PropertyType);
                propertyBehavior = (PropertyBehavior<TComponent>)propertyBehavior.SpecifyBackingComponentProperty(p);
                propertyBehavior.SetParent(this);
                return propertyBehavior;
            }));
        }

        static PropertyBehavior<TComponent> Make(string name, Access access, Type propertyType)
            => (PropertyBehavior<TComponent>)
                typeof(PropertyBehavior<,>)
                    .MakeGenericType(typeof(TComponent), propertyType)
                    .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
                    .Single()
                    .Invoke(new object[]
                    {
                        name,
                        access
                    });

        /// <summary>
        /// Retrieves the specified property behavior
        /// </summary>
        /// <param name="canGet">Indicates whether getting an existing property is allowed</param>
        /// <param name="canAdd">Indicates whether adding a new property is allowed</param>
        /// <param name="name">The name of the property to search for</param>
        /// <param name="access">Access modifier</param>
        /// <returns>Property's behavior, or null if not found</returns>
        internal PropertyBehavior<TComponent, T> Property<T>(bool canGet, bool canAdd, string name, Access access)
        {
            var found = Property(canGet, canAdd, name, access);

            return null;
        }

        internal PropertyBehavior<TComponent> Property(bool canGet, bool canAdd, string name, Access access)
        {
            PropertyBehavior<TComponent> found = _properties.FirstOrDefault(p => p.Name == name);

            if (found == null)
            {
                if (canAdd)
                {
                    found = new PropertyBehavior<TComponent>(name, access);
                    found.SetParent(this);
                    _properties.Add(found);
                }
            }
            else if (!canGet)
                found = null;
            else
                found.Decoration.AccessModifier = access;

            return found;
        }

        internal void Replaced(PropertyBehavior<TComponent> old, PropertyBehavior<TComponent> newer)
        {
            old.SetParent(null);
            newer.SetParent(this);
            bool removed = _properties.Remove(old);
            _properties.Add(newer);
        }

        IEnumerator<IPropertyBehaviorInternal<TComponent>> IEnumerable<IPropertyBehaviorInternal<TComponent>>.GetEnumerator() => _properties.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => (this as IEnumerable<IPropertyBehaviorInternal<TComponent>>).GetEnumerator();
    }
}
