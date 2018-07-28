using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace NAdapter
{
    /// <summary>
    /// Rewrites an expression tree to replace calls to <see cref="NAdapter.LinqParam{TComponent}"/>
    /// </summary>
    internal class LinqExpressionConverter<TComponent> : ExpressionVisitor where TComponent : class
    {
        int _index = 0;
        List<ParameterExpressionContainer> _parameters = new List<ParameterExpressionContainer>();

        /// <summary>
        /// Gets the parameters to be added to the new expression
        /// </summary>
        /// <returns>Parameters</returns>
        internal IEnumerable<ParameterExpressionContainer> GetParameters() => _parameters;
        
        /// <summary>
        /// Gets a string indicating the next parameter name available in this expression
        /// </summary>
        /// <returns>The next parameter name</returns>
        internal string NewParam() => $"_{++_index}";

        /// <summary>
        /// Converts method calls on LinqParam objects to expressions that can be resolved
        /// at runtime
        /// </summary>
        /// <param name="expr">Method call to convert</param>
        /// <returns>Converted expression</returns>
        protected override Expression VisitMethodCall(MethodCallExpression expr)
        {
            var result = LinqParam.ConvertMethodCall(expr, this, out ParameterExpressionContainer container);
            if (container != null)
                _parameters.Add(container);
            else
                return base.VisitMethodCall(expr);
            return result;
        }

        /// <summary>
        /// Checks a member expression for references to local variables, compiles
        /// the local variables into constants if possible and throws an exception
        /// if not.
        /// </summary>
        /// <param name="expr">MemberExpression to check</param>
        /// <returns>Converted expression</returns>
        protected override Expression VisitMember(MemberExpression expr)
        {
            if (expr.Expression != null && expr.Expression.Type.CustomAttributes.Any(a => a.AttributeType == typeof(CompilerGeneratedAttribute)))
            {
                var visited = Visit(expr.Expression);
                var member = Expression.MakeMemberAccess(visited, expr.Member);
                var lambda = Expression.Lambda(member);
                if (lambda.ReturnType.TypeIsConstant())
                {
                    var c = lambda.Compile().DynamicInvoke();
                    return Expression.Constant(c);
                }
                else
                    throw new ArgumentException($"Cannot compile a local variable of type {lambda.ReturnType} into a method.", nameof(expr));
            }
            return base.VisitMember(expr);
        }
    }
}
