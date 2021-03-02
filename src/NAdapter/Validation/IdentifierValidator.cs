using System;
using System.Collections.Generic;
using System.Linq;

namespace NAdapter
{
    /// <summary>
    /// Validation logic for identifiers
    /// </summary>
    internal class IdentifierValidator
    {
        static readonly HashSet<string> _csharpKeywords = new HashSet<string>()
        {
            // The ones commented out are allowed by the csharp compiler even without an @ in front.

            "abstract",
            //"add",
            "as",
            //"ascending",
            //"async",
            //"await",
            "base",
            "bool",
            "break",
            //"by",
            "byte",
            "case",
            "catch",
            "char",
            "checked",
            "class",
            "const",
            "continue",
            "decimal",
            "default",
            "delegate",
            //"descending",
            "do",
            "double",
            //"dynamic",
            "else",
            "enum",
            //"equals",
            "explicit",
            "extern",
            "false",
            "finally",
            "fixed",
            "float",
            "for",
            "foreach",
            //"from",
            //"get",
            //"global",
            "goto",
            //"group",
            "if",
            "implicit",
            "in",
            "int",
            "interface",
            "internal",
            //"into",
            "is",
            //"join",
            //"let",
            "lock",
            "long",
            "namespace",
            "new",
            "null",
            "object",
            //"on",
            "operator",
            //"orderby",
            "out",
            "override",
            "params",
            //"partial",
            "private",
            "protected",
            "public",
            "readonly",
            "ref",
            //"remove",
            "return",
            "sbyte",
            "sealed",
            //"select",
            //"set",
            "short",
            "sizeof",
            "stackalloc",
            "static",
            "string",
            "struct",
            "switch",
            "this",
            "throw",
            "true",
            "try",
            "typeof",
            "uint",
            "ulong",
            "unchecked",
            "unsafe",
            "ushort",
            "using",
            //"value",
            //"var",
            "virtual",
            "void",
            "volatile",
            //"where",
            "while"
            //"yield"
        };

        int _displayClasses = 0;
        int _displayMethods = 0;
        HashSet<string> _identifiers = new HashSet<string>();

        /// <summary>
        /// Validator for members of a given type
        /// </summary>
        /// <param name="typeName">The type name</param>
        internal IdentifierValidator(string typeName) => TypeName = typeName;

        /// <summary>
        /// Type name
        /// </summary>
        internal string TypeName { get; set; }
        
        /// <summary>
        /// Indicates whether an identifier is valid
        /// </summary>
        /// <param name="identifier">Identifier to check</param>
        /// <param name="forType">True if the identifier is a type identifier,
        /// false if another kind of code element</param>
        /// <returns>True if valid, false if invalid</returns>
        internal static bool IsValidIdentifier(string identifier, bool forType)
        {
            if (string.IsNullOrEmpty(identifier))
                return false;

            if (forType)
                return identifier.Split('.').All(i => IsValidIdentifier(i, false));

            if (identifier[0] == '@')
            {
                string identifierNoAt = identifier.Substring(1);
                return IsKeyword(identifierNoAt) || IsValidIdentifierNoAt(identifierNoAt);
            }

            return IsValidIdentifierNoAt(identifier);
        }

        /// <summary>
        /// Validates an identifier and adds it to the list of identifiers consumed
        /// by this type for checking duplicates
        /// </summary>
        /// <param name="identifier">Identifier</param>
        /// <param name="forType">True if type, false if another kind of code element</param>
        /// <returns>True if valid, false if invalid</returns>
        internal bool ValidateIdentifier(string identifier, bool forType)
            => IsValidIdentifier(identifier, forType) && _identifiers.Add(identifier);

        /// <summary>
        /// Gets an identifier for a new nested type
        /// </summary>
        /// <returns>Validator for the new class</returns>
        internal IdentifierValidator GetDisplayClassIdentifier()
        {
            string typeName = GetAndAdd($"<>c__DisplayClass{++_displayClasses}");
            return new IdentifierValidator(typeName);
        }

        /// <summary>
        /// Gets an identifier for a backing field for a property
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <returns>Backing field identifier</returns>
        internal string GetBackingFieldIdentifier(string propertyName)
        {
            return GetAndAdd($"<{propertyName}>k__BackingField");
        }
        
        /// <summary>
        /// Gets an identifier for a backing field getter
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <returns>Backing getter field identifier</returns>
        internal string GetBackingGetterIdentifier(string propertyName)
        {
            return GetAndAdd($"<{propertyName}>k__BackingGetter");
        }

        /// <summary>
        /// Gets an identifier for a backing field, guaranteed
        /// to be unique.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <returns>Backing field identifier</returns>
        internal string GetUniqueBackingFieldIdentifier(string propertyName)
        {
            for (int i = 0; true; ++i)
            {
                string fieldIdentifier = $"<{propertyName}_{i}>k__BackingField";
                if (_identifiers.Add(fieldIdentifier))
                    return fieldIdentifier;
            }
        }

        /// <summary>
        /// Gets an identifier for a backing field setter
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <returns>Backing setter field identifier</returns>
        internal string GetBackingSetterIdentifier(string propertyName)
        {
            return GetAndAdd($"<{propertyName}>k__BackingSetter");
        }

        /// <summary>
        /// Gets an identifier for a backing method.
        /// </summary>
        /// <param name="methodName">Method name</param>
        /// <returns>Backing method identifier</returns>
        internal string GetLambdaIdentifier(string methodName)
        {
            return GetAndAdd($"<{methodName}>b__{++_displayMethods}");
        }

        static bool IsKeyword(string identifier)
        {
            return _csharpKeywords.Contains(identifier);
        }

        static bool IsValidIdentifierNoAt(string identifier)
        {
            if (identifier.Length == 0)
                return false;

            return IsValidInitial(identifier[0]) && AreValidCharacters(identifier) && !IsKeyword(identifier);
        }

        static bool IsValidInitial(char c)
        {
            return c == '_' || Char.IsLetter(c);
        }

        static bool AreValidCharacters(string s)
        {
            return s.All(c => c == '_' || Char.IsLetterOrDigit(c));
        }

        string GetAndAdd(string identifier)
        {
            if (_identifiers.Add(identifier))
                return identifier;
            throw new ArgumentException($"Duplicate identifier: {identifier}");
        }
    }
}
