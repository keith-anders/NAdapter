using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace NAdapter
{
    /// <summary>
    /// Specifier for a type to be created
    /// </summary>
    internal class TypeSpecifier
    {
        /// <summary>
        /// Identifiers in this type
        /// </summary>
        IdentifierValidator _identifiers;

        /// <summary>
        /// Nested classes
        /// </summary>
        List<TypeSpecifier> _nestedClasses = new List<TypeSpecifier>();

        /// <summary>
        /// Actions to be run after all members are declared.
        /// </summary>
        List<Action> _actionsForAfterCreation = new List<Action>();
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tb">TypeBuilder</param>
        internal TypeSpecifier(TypeBuilder tb)
        {
            _identifiers = new IdentifierValidator(tb.FullName);
            TypeBuilder = tb;
        }
        
        /// <summary>
        /// TypeBuilder
        /// </summary>
        internal TypeBuilder TypeBuilder { get; private set; }

        /// <summary>
        /// Creates a nested class
        /// </summary>
        /// <returns>TypeSpecifier for a new nested class</returns>
        internal TypeSpecifier CreateDisplayClass()
        {
            var identifier = _identifiers.GetDisplayClassIdentifier();
            var nestedTb = TypeBuilder.DefineNestedType(identifier.TypeName, TypeAttributes.NestedPrivate | TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.Abstract);

            nestedTb.SetCustomAttribute(Specification<string>.GeneratedCode);

            var ts = new TypeSpecifier(nestedTb)
            {
                _identifiers = identifier
            };
            _nestedClasses.Add(ts);
            return ts;
        }

        /// <summary>
        /// Finishes the type
        /// </summary>
        internal void Finish()
        {
            foreach (var nested in _nestedClasses)
            {
                try { nested.TypeBuilder.CreateType(); } catch { }
                nested.Finish();
            }
            foreach (var action in _actionsForAfterCreation)
                action();
        }

        /// <summary>
        /// Gets the name of a new method name based on a given method
        /// </summary>
        /// <param name="name">Method name</param>
        /// <returns>New method name</returns>
        internal string GetInvokeName(string name)
        {
            return _identifiers.GetLambdaIdentifier(name);
        }

        /// <summary>
        /// Gets the name of a new field to contain a backing getter delegate
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <returns>Field name</returns>
        internal string GetBackingGetterIdentifier(string propertyName)
        {
            return _identifiers.GetBackingGetterIdentifier(propertyName);
        }

        /// <summary>
        /// Gets the name of a new field to contain a backing field,
        /// guaranteed to be unique.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <returns>Field name</returns>
        internal string GetUniqueBackingFieldIdentifier(string propertyName)
        {
            return _identifiers.GetUniqueBackingFieldIdentifier(propertyName);
        }

        /// <summary>
        /// Gets the name of a new field to contain a backing setter delegate
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <returns>Field name</returns>
        internal string GetBackingSetterIdentifier(string propertyName)
        {
            return _identifiers.GetBackingSetterIdentifier(propertyName);
        }

        /// <summary>
        /// Gets the name of a new method based on a given method
        /// </summary>
        /// <param name="methodName">Method name</param>
        /// <returns>New method name</returns>
        internal string GetMethodName(string methodName)
        {
            return _identifiers.GetLambdaIdentifier(methodName);
        }
        
        /// <summary>
        /// Gets the name of a backing field based on a given property
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <returns>New field name</returns>
        internal string GetBackingFieldIdentifier(string propertyName)
        {
            return _identifiers.GetBackingFieldIdentifier(propertyName);
        }

        /// <summary>
        /// Registers a delegate to be run after the type is defined. Will set
        /// any private static fields and the like.
        /// </summary>
        /// <param name="a">Action</param>
        internal void RunAfterCreation(Action a)
        {
            _actionsForAfterCreation.Add(a);
        }
    }
}
