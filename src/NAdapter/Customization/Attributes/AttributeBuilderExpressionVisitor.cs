using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NAdapter
{
    /// <summary>
    /// Parses linq expression into an attribute builder
    /// </summary>
    internal class AttributeExpressionParser
    {
        Func<CustomAttributeBuilderBuilder, CustomAttributeBuilderBuilder> _getter;

        /// <summary>
        /// Parses an expression for converting one attribute into another.
        /// </summary>
        /// <typeparam name="TIn">The type of attribute to convert</typeparam>
        /// <param name="expr">Conversion expression</param>
        /// <returns>Parser</returns>
        internal static AttributeExpressionParser Parse<TIn>(Expression<Func<TIn,Attribute>> expr)
            where TIn: Attribute
        {
            return Parse<TIn>((LambdaExpression)expr);
        }

        /// <summary>
        /// Parses an expression for creating an attribute.
        /// </summary>
        /// <typeparam name="T">Type of attribute to create</typeparam>
        /// <param name="expr">Creation expression</param>
        /// <returns>Parser</returns>
        internal static AttributeExpressionParser Parse<T>(Expression<Func<T>> expr)
            where T: Attribute
        {
            return Parse<Attribute>(expr);
        }

        /// <summary>
        /// Gets an attribute builder from a builder that specifies an existing attribute,
        /// applying any conversion along the way
        /// </summary>
        /// <param name="builder">Builder to convert</param>
        /// <returns>Builder to apply to the adapter member</returns>
        internal CustomAttributeBuilderBuilder Get(CustomAttributeBuilderBuilder builder) => _getter(builder);
        
        static AttributeExpressionParser Parse<T>(LambdaExpression expr) where T: Attribute
        {
            if (expr == null)
                throw new ArgumentNullException(nameof(expr));
            
            var builderParam = Expression.Parameter(typeof(CustomAttributeBuilderBuilder), "_0");
            var newAttParam = Expression.Parameter(typeof(Lazy<T>), "_1");
            var oldAttParam = expr.Parameters.FirstOrDefault();
            var p = new[] { newAttParam, builderParam };
            var built = PrivateToAttributeBuilderBuilder(expr.Body, builderParam, oldAttParam);
            built = new LazyVisitor<T>(oldAttParam, newAttParam).Visit(built);
            var compiled = (Func<Lazy<T>, CustomAttributeBuilderBuilder, CustomAttributeBuilderBuilder>)Expression.Lambda(built, p).Compile();

            return new AttributeExpressionParser()
            {
                _getter = cabb =>
                {
                    return compiled(new Lazy<T>(() => (T)cabb?.BuildAttribute()), cabb);
                }
            };
        }
        
        static Expression PrivateToAttributeBuilderBuilder(Expression expr, ParameterExpression paramOriginalBuilder, ParameterExpression paramAttribute)
        {
            switch (expr)
            {
                case NewExpression newExp:
                    var parsed = ParseBuilder(newExp, out ParameterExpression result);
                    return Expression.Block(new[] { result }, parsed);
                case ConditionalExpression conditionalExp:
                    return Expression.Condition(conditionalExp.Test,
                        PrivateToAttributeBuilderBuilder(conditionalExp.IfTrue, paramOriginalBuilder, paramAttribute),
                        PrivateToAttributeBuilderBuilder(conditionalExp.IfFalse, paramOriginalBuilder, paramAttribute));
                case MemberInitExpression initExp: return ParseBuilder(initExp);
                case UnaryExpression unaryExp:
                    if (unaryExp.NodeType == ExpressionType.Convert)
                        return PrivateToAttributeBuilderBuilder(unaryExp.Operand, paramOriginalBuilder, paramAttribute);
                    break;
                case ConstantExpression constExp:
                    if (constExp.Value == null)
                        return Expression.Constant(null, typeof(CustomAttributeBuilderBuilder));
                    break;
                case ParameterExpression paramExp: return paramOriginalBuilder;
            }

            throw new ArgumentException("Constructor expression must be constructor or conditional", nameof(expr));
        }

        static List<Expression> ParseBuilder(NewExpression expr, out ParameterExpression result)
        {
            var addArg = typeof(CustomAttributeBuilderBuilder).GetMethod(nameof(CustomAttributeBuilderBuilder.AddConstructorArg));
            List<Expression> initializations = new List<Expression>();
            result = Expression.Variable(typeof(CustomAttributeBuilderBuilder), "result");
            initializations.Add(result);
            var ctor = typeof(CustomAttributeBuilderBuilder).GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null);
            initializations.Add(Expression.Assign(result, Expression.New(ctor)));
            initializations.Add(Expression.Call(result, typeof(CustomAttributeBuilderBuilder).GetMethod("SetConstructor"), Expression.Constant(expr.Constructor)));
            foreach (var arg in expr.Arguments)
                initializations.Add(Expression.Call(result, addArg, arg));
            initializations.Add(result);
            return initializations;
        }
        
        static Expression ParseBuilder(MemberInitExpression expr)
        {
            var initializations = ParseBuilder(expr.NewExpression, out ParameterExpression result);
            var addField = typeof(CustomAttributeBuilderBuilder).GetMethod(nameof(CustomAttributeBuilderBuilder.AddField));
            var addProp = typeof(CustomAttributeBuilderBuilder).GetMethod(nameof(CustomAttributeBuilderBuilder.AddProperty));

            foreach (var binding in expr.Bindings)
            {
                if (binding is MemberAssignment asn)
                {
                    try
                    {
                        switch (asn.Member)
                        {
                            case FieldInfo field:
                                initializations.Add(Expression.Call(result, addField, Expression.Constant(field), asn.Expression));
                                break;
                            case PropertyInfo prop:
                                initializations.Add(Expression.Call(result, addProp, Expression.Constant(prop), asn.Expression));
                                break;
                            default:
                                throw new ArgumentException("Cannot initialize members other than fields or properties.", nameof(expr));
                        }
                    }
                    catch (ArgumentException ex)
                    {
                        throw new ArgumentException("Invalid member initialization expression.", nameof(expr), ex);
                    }
                }
                else if (binding is MemberBinding)
                    throw new ArgumentException("Cannot initialize recursive init expression; use constructor instead.");
                else if (binding is MemberListBinding)
                    throw new ArgumentException("Cannot initialize recursive list init expression; use constructor instead.");
                else
                    throw new ArgumentException("Cannot initialize unknown member init expression type.");
            }

            initializations.Add(result);

            return Expression.Block(new[] { result }, initializations);
        }
    }
}
