using System;
using System.Linq.Expressions;
using System.Reflection;

namespace NAdapter
{
    /// <summary>
    /// Linq expression visitor which replaces references to a given parameter expression
    /// with references to a Lazy loaded version of another parameter expression
    /// </summary>
    /// <typeparam name="T">Type being replaced</typeparam>
    internal class LazyVisitor<T> : ExpressionVisitor
    {
        ParameterExpression _toReplace;
        ParameterExpression _toReplaceWith;
        PropertyInfo _value;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="toReplace">Expression to replace</param>
        /// <param name="toReplaceWith">Expression to substitute</param>
        internal LazyVisitor(ParameterExpression toReplace, ParameterExpression toReplaceWith)
        {
            _toReplace = toReplace;
            _toReplaceWith = toReplaceWith;
            _value = typeof(Lazy<T>).GetProperty(nameof(Lazy<string>.Value));
        }

        /// <summary>
        /// Visits ParameterExpressions
        /// </summary>
        /// <param name="node">Parameter expression</param>
        /// <returns>Visited expression</returns>
        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node == _toReplace)
                return Expression.MakeMemberAccess(_toReplaceWith, _value);
            return base.VisitParameter(node);
        }
    }
}
