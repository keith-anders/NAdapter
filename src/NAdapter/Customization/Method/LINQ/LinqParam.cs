using System;
using System.Linq;
using System.Linq.Expressions;

namespace NAdapter
{
    /// <summary>
    /// Container for Linq conversion information
    /// </summary>
    internal static class LinqParam
    {
        /// <summary>
        /// Converts a method call expression into an expression that can be compiled into the
        /// method
        /// </summary>
        /// <typeparam name="TComponent">The type of component being adapted</typeparam>
        /// <param name="meth">The expression to be converted</param>
        /// <param name="converter">Expression converter</param>
        /// <param name="container">Parameter container, if a parameter needs to be
        /// added to the method</param>
        /// <returns>Converted expression</returns>
        internal static Expression ConvertMethodCall<TComponent>(MethodCallExpression meth, LinqExpressionConverter<TComponent> converter, out ParameterExpressionContainer container)
            where TComponent : class
        {
            container = null;
            Expression result = meth;
            if (meth.Object != null && meth.Object.Type.IsGenericType && meth.Object.Type.GetGenericTypeDefinition() == typeof(LinqParam<>))
                switch (meth.Method.Name)
                {
                    case nameof(LinqParam<string>.Arg):
                        {
                            var p = Expression.Parameter(meth.Method.ReturnType, converter.NewParam());
                            if (meth.Method.GetParameters().Single().ParameterType == typeof(int))
                                container = new ParameterExpressionContainer(new ArgumentParam((int)Expression.Lambda(typeof(Func<int>), meth.Arguments[0]).Compile().DynamicInvoke(), meth.Method.ReturnType), p);
                            else
                                container = new ParameterExpressionContainer(new ArgumentParam((string)Expression.Lambda(typeof(Func<string>), meth.Arguments[0]).Compile().DynamicInvoke(), meth.Method.ReturnType), p);
                            return p;
                        }
                    case nameof(LinqParam<string>.Getter):
                        {
                            var propertyBehavior = (IPropertyBehaviorInternal<TComponent>)Expression.Lambda(typeof(Func<IPropertyBehaviorInternal<TComponent>>), meth.Arguments[0]).Compile().DynamicInvoke();
                            var p = Expression.Parameter(typeof(Func<>).MakeGenericType(propertyBehavior.PropertyType), converter.NewParam());
                            container = new ParameterExpressionContainer(new PropertyGetterParam<TComponent>(propertyBehavior, propertyBehavior.PropertyType), p);
                            return Expression.Invoke(p);
                        }
                    case nameof(LinqParam<string>.Setter):
                        {
                            var propertyBehavior = (IPropertyBehaviorInternal<TComponent>)Expression.Lambda(typeof(Func<IPropertyBehaviorInternal<TComponent>>), meth.Arguments[0]).Compile().DynamicInvoke();
                            var p = Expression.Parameter(typeof(Action<>).MakeGenericType(propertyBehavior.PropertyType), converter.NewParam());
                            container = new ParameterExpressionContainer(new PropertySetterParam<TComponent>(propertyBehavior, propertyBehavior.PropertyType), p);
                            return Expression.Invoke(p, converter.Visit(meth.Arguments[1]));
                        }
                    case nameof(LinqParam<string>.Action):
                    case nameof(LinqParam<string>.Function):
                        {
                            var methodBehavior = (IMethodBehavior<TComponent>)Expression.Lambda(typeof(Func<IMethodBehavior<TComponent>>), meth.Arguments[0]).Compile().DynamicInvoke();
                            var methodDelegateType = methodBehavior.DelegateType;
                            var p = Expression.Parameter(methodDelegateType, converter.NewParam());
                            container = new ParameterExpressionContainer(new MethodParam<TComponent>(methodBehavior, methodDelegateType), p);
                            var returnType = meth.Method.ReturnType;
                            return Expression.Invoke(p, meth.Arguments.Skip(1).Select(converter.Visit).ToArray());
                        }
                }
            return meth;
        }
    }

    /// <summary>
    /// Linq parameter
    /// </summary>
    /// <typeparam name="TComponent">The type of component being adapted</typeparam>
    public class LinqParam<TComponent> where TComponent : class
    {
        LinqParam() { }

        /// <summary>
        /// Call a previously-defined function on the adapter
        /// </summary>
        /// <typeparam name="TReturn">The type returned by the function</typeparam>
        /// <param name="behavior">The behavior of the function to call</param>
        /// <param name="args">The arguments of that function</param>
        /// <returns>The result of the function call</returns>
        public TReturn Function<TReturn>(IMethodBehavior<TComponent> behavior) => default(TReturn);

        public TReturn Function<T1, TReturn>(IMethodBehavior<TComponent> behavior, T1 arg1) => default(TReturn);

        public TReturn Function<T1, T2, TReturn>(IMethodBehavior<TComponent> behavior, T1 arg1, T2 arg2) => default(TReturn);

        public TReturn Function<T1, T2, T3, TReturn>(IMethodBehavior<TComponent> behavior, T1 arg1, T2 arg2, T3 arg3) => default(TReturn);

        public TReturn Function<T1, T2, T3, T4, TReturn>(IMethodBehavior<TComponent> behavior, T1 arg1, T2 arg2, T3 arg3, T4 arg4) => default(TReturn);

        public TReturn Function<TReturn>(MethodFunctionBehavior<TComponent, TReturn> behavior) => default(TReturn);

        public TReturn Function<T1, TReturn>(MethodFunctionBehavior<TComponent, T1, TReturn> behavior, T1 arg1) => default(TReturn);

        public TReturn Function<T1, T2, TReturn>(MethodFunctionBehavior<TComponent, T1, T2, TReturn> behavior, T1 arg1, T2 arg2) => default(TReturn);

        public TReturn Function<T1, T2, T3, TReturn>(MethodFunctionBehavior<TComponent, T1, T2, T3, TReturn> behavior, T1 arg1, T2 arg2, T3 arg3) => default(TReturn);

        public TReturn Function<T1, T2, T3, T4, TReturn>(MethodFunctionBehavior<TComponent, T1, T2, T3, T4, TReturn> behavior, T1 arg1, T2 arg2, T3 arg3, T4 arg4) => default(TReturn);

        public TReturn Function<T1, T2, T3, T4, T5, TReturn>(MethodFunctionBehavior<TComponent, T1, T2, T3, T4, T5, TReturn> behavior, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) => default(TReturn);

        /// <summary>
        /// Call a previously-defined action on the adapter
        /// </summary>
        /// <param name="behavior">The behavior of the action to call</param>
        /// <param name="args">The arguments of that action</param>
        public void Action(IMethodBehavior<TComponent> behavior) { }

        public void Action<T1>(IMethodBehavior<TComponent> behavior, T1 arg1) { }

        public void Action<T1, T2>(IMethodBehavior<TComponent> behavior, T1 arg1, T2 arg2) { }

        public void Action<T1, T2, T3>(IMethodBehavior<TComponent> behavior, T1 arg1, T2 arg2, T3 arg3) { }

        public void Action<T1, T2, T3, T4>(IMethodBehavior<TComponent> behavior, T1 arg1, T2 arg2, T3 arg3, T4 arg4) { }

        public void Action<T1>(MethodActionBehavior<TComponent, T1> behavior, T1 arg1) { }

        public void Action<T1, T2>(MethodActionBehavior<TComponent, T1, T2> behavior, T1 arg1, T2 arg2) { }

        public void Action<T1, T2, T3>(MethodActionBehavior<TComponent, T1, T2, T3> behavior, T1 arg1, T2 arg2, T3 arg3) { }

        public void Action<T1, T2, T3, T4>(MethodActionBehavior<TComponent, T1, T2, T3, T4> behavior, T1 arg1, T2 arg2, T3 arg3, T4 arg4) { }

        public void Action<T1, T2, T3, T4, T5>(MethodActionBehavior<TComponent, T1, T2, T3, T4, T5> behavior, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) { }

        /// <summary>
        /// Return the method's parameter of a given index
        /// </summary>
        /// <typeparam name="T">The type of parameter</typeparam>
        /// <param name="index">The index of the parameter</param>
        /// <returns>The type of argument being returned</returns>
        public T Arg<T>(int index) => default(T);

        /// <summary>
        /// Return the method's parameter of a given name
        /// </summary>
        /// <typeparam name="T">The type of parameter</typeparam>
        /// <param name="name">The name of the parameter</param>
        /// <returns>The type of argument being returned</returns>
        public T Arg<T>(string name) => default(T);

        /// <summary>
        /// Get the value of a previously-defined property on the adapter
        /// </summary>
        /// <typeparam name="T">The type of property to get</typeparam>
        /// <param name="behavior">The property's behavior</param>
        /// <returns>The value</returns>
        public T Getter<T>(IPropertyBehavior<TComponent> behavior) => default(T);

        public T Getter<T>(PropertyBehavior<TComponent, T> behavior) => default(T);

        /// <summary>
        /// Sets the value of a previously-defined property on the adapter
        /// </summary>
        /// <typeparam name="T">The type of property to set</typeparam>
        /// <param name="behavior">The property's behavior</param>
        /// <param name="value">The value to set</param>
        public void Setter<T>(IPropertyBehavior<TComponent> behavior, T value) { }

        public void Setter<T>(PropertyBehavior<TComponent, T> behavior, T value) { }
    }
}
