using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NAdapter
{
    /// <summary>
    /// Specification for methods to be defined on the adapter
    /// </summary>
    /// <typeparam name="TComponent">Type of component being adapted</typeparam>
    internal class MethodsSpecification<TComponent> : IEnumerable<IMethodBehaviorInternal<TComponent>> where TComponent : class
    {
        static MethodsSpecification()
        {
            _actualMethods = (from p in typeof(TComponent).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                              where !p.IsSpecialName && p.DeclaringType != typeof(Object)
                              group p by p.Name)
                              .ToDictionary(k => k.Key, k => k.ToArray());
        }

        static Dictionary<string, MethodInfo[]> _actualMethods = new Dictionary<string, MethodInfo[]>();

        List<IMethodBehaviorInternal<TComponent>> _methods = new List<IMethodBehaviorInternal<TComponent>>();

        /// <summary>
        /// Constructor.
        /// </summary>
        internal MethodsSpecification()
            => _methods.AddRange(_actualMethods.Values
                                    .SelectMany(p => p)
                                    .Select(p =>
                                    {
                                        var result = MethodBehaviorBase<TComponent>.Make<IMethodBehaviorInternal<TComponent>>(p);
                                        if (result.IsValid)
                                            result.SpecifyMethod(p);
                                        return result;
                                    }).ToArray());

        /// <summary>
        /// Gets a method's behavior
        /// </summary>
        /// <param name="canGet">True if getting an existing method is allowed, false if disallowed</param>
        /// <param name="canAdd">True if adding a new method is allowed, false if disallowed</param>
        /// <param name="name">Name of the method</param>
        /// <param name="access">The method's access modifier</param>
        /// <param name="types">Method's parameter types</param>
        /// <returns>Method's behavior</returns>
        internal T Method<T>(bool canGet, bool canAdd, string name, Access access, ParamSettingsContainer[] types)
            where T : MethodBehaviorBase<TComponent, T>
        {
            T found = _methods
                .Where(m => m.IsValid)
                .OfType<T>()
                .FirstOrDefault(p => p.Name == name);

            if (found == null)
            {
                // Might have mismatched because of return type.
                var result = _methods
                    .FirstOrDefault(m =>
                        m.IsValid &&
                        m.Name == name &&
                        m.Parameters.Select(pi => pi.Type).SequenceEqual(types.Select(t => t.Type)));
                if (result != null)
                {
                    if (canGet)
                    {
                        _methods.Remove(result);
                        found = Method<T>(canGet, canAdd, name, access, types);
                    }
                }
                else if (canAdd)
                {
                    found = MethodBehaviorBase<TComponent>.Make<T>(name, types);
                    found.Decoration.AccessModifier = access;
                    _methods.Add(found);
                }
            }
            else if (!canGet)
                found = null;
            else
            {
                found.Decoration.AccessModifier = access;
                found.SetParamSettings(types);
            }

            return found;
        }

        IEnumerator<IMethodBehaviorInternal<TComponent>> IEnumerable<IMethodBehaviorInternal<TComponent>>.GetEnumerator()
            => _methods.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => ((IEnumerable<IMethodBehaviorInternal<TComponent>>)this).GetEnumerator();
    }
}
