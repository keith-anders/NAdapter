using System.Linq.Expressions;

namespace NAdapter
{
    /// <summary>
    /// Container for the data related to a parameter of a LINQ expression
    /// </summary>
    internal class ParameterExpressionContainer
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parameter">Parameter data</param>
        /// <param name="expr">Expression</param>
        internal ParameterExpressionContainer(Param parameter, ParameterExpression expr)
        {
            Expression = expr;
            Parameter = parameter;
        }

        /// <summary>
        /// Expression
        /// </summary>
        internal ParameterExpression Expression { get; private set; }

        /// <summary>
        /// Parameter data
        /// </summary>
        internal Param Parameter { get; private set; }
    }
}
