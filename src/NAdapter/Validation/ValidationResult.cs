using System.Collections.Generic;
using System.Linq;

namespace NAdapter
{
    /// <summary>
    /// Result of a validation
    /// </summary>
    public abstract class ValidationResult
    {
        List<string> _warnings = new List<string>();
        List<string> _errors = new List<string>();
        List<ValidationResult> _children = new List<ValidationResult>();

        /// <summary>
        /// Warnings of this member and all children.
        /// </summary>
        public string[] Warnings { get { return _warnings.Concat(_children.SelectMany(c => c.Warnings).Select(w => $"{Identifier}: {w}")).ToArray(); } }

        /// <summary>
        /// Errors of this member and all children.
        /// </summary>
        public string[] Errors { get { return _errors.Concat(_children.SelectMany(c => c.Errors).Select(e => $"{Identifier}: {e}")).ToArray(); } }
        
        /// <summary>
        /// Identifier for this member
        /// </summary>
        internal abstract string Identifier { get; }

        /// <summary>
        /// Indicates whether this member type requires a valid identifier.
        /// </summary>
        internal virtual bool NeedsValidIdentifier { get { return true; } }
        
        /// <summary>
        /// Adds a warning
        /// </summary>
        /// <param name="warning">Warning</param>
        internal void AddWarning(string warning) => _warnings.Add(warning);

        /// <summary>
        /// Adds an error
        /// </summary>
        /// <param name="error">Error</param>
        internal void AddError(string error) => _errors.Add(error);

        /// <summary>
        /// Validates an identifier
        /// </summary>
        /// <param name="proposedIdentifier">The identifier to validate</param>
        /// <param name="forType">Indicates whether this is a type name. Periods
        /// are allowed in type names because of namespaces but not anywhere else.</param>
        internal void ValidateIdentifier(string proposedIdentifier, bool forType)
        {
            if (!IdentifierValidator.IsValidIdentifier(proposedIdentifier, forType))
                _errors.Add($"Identifier {proposedIdentifier} is not valid.");
        }

        /// <summary>
        /// Children
        /// </summary>
        protected IEnumerable<ValidationResult> Children { get { return _children; } }

        /// <summary>
        /// Registers a child
        /// </summary>
        /// <param name="child">Child</param>
        protected void RegisterChild(ValidationResult child)
        {
            if (child.NeedsValidIdentifier)
            {
                ValidateIdentifier(child.Identifier, false);
                if (_children.Any(c => c.Identifier == child.Identifier))
                    AddError($"Multiple members with name {child.Identifier}");
            }
            _children.Add(child);
        }
    }
}
