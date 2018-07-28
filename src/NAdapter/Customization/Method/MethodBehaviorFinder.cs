using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NAdapter
{
    /// <summary>
    /// Finder for a method's behavior
    /// </summary>
    /// <typeparam name="TComponent">Type of component being decorated</typeparam>
    public class MethodBehaviorFinder<TComponent> where TComponent : class
    {
        MethodsSpecification<TComponent> _methods;
        Behavior _behavior;
        Access _access;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="methods">Methods currently defined</param>
        /// <param name="behavior">Behavior to employ when searching</param>
        /// <param name="access">Access modifier to apply to the method</param>
        internal MethodBehaviorFinder(MethodsSpecification<TComponent> methods, Behavior behavior, Access access)
        {
            _methods = methods;
            _behavior = behavior;
            _access = access;
        }

        /// <summary>
        /// Specifies a method with the specified name, returning void, and taking no parameters
        /// </summary>
        /// <param name="name">Name of the method</param>
        /// <returns>Method's behavior</returns>
        public MethodActionBehavior<TComponent> WithActionSignature(string name)
            => WithSignature<MethodActionBehavior<TComponent>>(name);

        /// <summary>
        /// Specifies a method with the specified name, returning void, and taking the given parameter type
        /// </summary>
        /// <param name="name">Name of the method</param>
        /// <param name="param1">Settings for first method parameter</param>
        /// <typeparam name="T1">Type of the method's first parameter</typeparam>
        /// <returns>Method's behavior</returns>
        public MethodActionBehavior<TComponent, T1> WithActionSignature<T1>(string name, ParamSettings param1 = null)
            => WithSignature<MethodActionBehavior<TComponent, T1>>(name, param1.OfType<T1>());

        /// <summary>
        /// Specifies a method with the specified name, returning void, and taking the given parameter types
        /// </summary>
        /// <param name="name">Name of the method</param>
        /// <param name="param1">Settings for the first method parameter</param>
        /// <param name="param2">Settings for the second method parameter</param>
        /// <typeparam name="T1">Method's first parameter type</typeparam>
        /// <typeparam name="T2">Method's second parameter type</typeparam>
        /// <returns>Method's behavior</returns>
        public MethodActionBehavior<TComponent, T1, T2> WithActionSignature<T1, T2>(string name, ParamSettings param1 = null, ParamSettings param2 = null)
            => WithSignature<MethodActionBehavior<TComponent, T1, T2>>(name, param1.OfType<T1>(), param2.OfType<T2>());

        /// <summary>
        /// Specifies a method with the specified name, returning void, and taking the given parameter types
        /// </summary>
        /// <param name="name">Name of the method</param>
        /// <param name="param1">Settings for the first method parameter</param>
        /// <param name="param2">Settings for the second method parameter</param>
        /// <param name="param3">Settings for the third method parameter</param>
        /// <typeparam name="T1">Method's first parameter type</typeparam>
        /// <typeparam name="T2">Method's second parameter type</typeparam>
        /// <typeparam name="T3">Method's third parameter type</typeparam>
        /// <returns>Method's behavior</returns>
        public MethodActionBehavior<TComponent, T1, T2, T3> WithActionSignature<T1, T2, T3>(string name, ParamSettings param1 = null, ParamSettings param2 = null, ParamSettings param3 = null)
            => WithSignature<MethodActionBehavior<TComponent, T1, T2, T3>>(name, param1.OfType<T1>(), param2.OfType<T2>(), param3.OfType<T3>());

        /// <summary>
        /// Specifies a method with the specified name, returning void, and taking the given parameter types
        /// </summary>
        /// <param name="name">Name of the method</param>
        /// <param name="param1">Settings for the first method parameter</param>
        /// <param name="param2">Settings for the second method parameter</param>
        /// <param name="param3">Settings for the third method parameter</param>
        /// <param name="param4">Settings for the fourth method parameter</param>
        /// <typeparam name="T1">Method's first parameter type</typeparam>
        /// <typeparam name="T2">Method's second parameter type</typeparam>
        /// <typeparam name="T3">Method's third parameter type</typeparam>
        /// <typeparam name="T4">Method's fourth parameter type</typeparam>
        /// <returns>Method's behavior</returns>
        public MethodActionBehavior<TComponent, T1, T2, T3, T4> WithActionSignature<T1, T2, T3, T4>(string name, ParamSettings param1 = null, ParamSettings param2 = null, ParamSettings param3 = null, ParamSettings param4 = null)
            => WithSignature<MethodActionBehavior<TComponent, T1, T2, T3, T4>>(name, param1.OfType<T1>(), param2.OfType<T2>(), param3.OfType<T3>(), param4.OfType<T4>());

        /// <summary>
        /// Specifies a method with the specified name, returning void, and taking the given parameter types
        /// </summary>
        /// <param name="name">Name of the method</param>
        /// <param name="param1">Settings for the first method parameter</param>
        /// <param name="param2">Settings for the second method parameter</param>
        /// <param name="param3">Settings for the third method parameter</param>
        /// <param name="param4">Settings for the fourth method parameter</param>
        /// <param name="param5">Settings for the fifth method parameter</param>
        /// <typeparam name="T1">Method's first parameter type</typeparam>
        /// <typeparam name="T2">Method's second parameter type</typeparam>
        /// <typeparam name="T3">Method's third parameter type</typeparam>
        /// <typeparam name="T4">Method's fourth parameter type</typeparam>
        /// <typeparam name="T5">Method's fifth parameter type</typeparam>
        /// <returns>Method's behavior</returns>
        public MethodActionBehavior<TComponent, T1, T2, T3, T4, T5> WithActionSignature<T1, T2, T3, T4, T5>(string name, ParamSettings param1 = null, ParamSettings param2 = null, ParamSettings param3 = null, ParamSettings param4 = null, ParamSettings param5 = null)
            => WithSignature<MethodActionBehavior<TComponent, T1, T2, T3, T4, T5>>(name, param1.OfType<T1>(), param2.OfType<T2>(), param3.OfType<T3>(), param4.OfType<T4>(), param5.OfType<T5>());

        /// <summary>
        /// Specifies a method with the specified name, returning void, and taking the given parameter types
        /// </summary>
        /// <param name="name">Name of the method</param>
        /// <param name="param1">Settings for the first method parameter</param>
        /// <param name="param2">Settings for the second method parameter</param>
        /// <param name="param3">Settings for the third method parameter</param>
        /// <param name="param4">Settings for the fourth method parameter</param>
        /// <param name="param5">Settings for the fifth method parameter</param>
        /// <param name="param6">Settings for the sixth method parameter</param>
        /// <typeparam name="T1">Method's first parameter type</typeparam>
        /// <typeparam name="T2">Method's second parameter type</typeparam>
        /// <typeparam name="T3">Method's third parameter type</typeparam>
        /// <typeparam name="T4">Method's fourth parameter type</typeparam>
        /// <typeparam name="T5">Method's fifth parameter type</typeparam>
        /// <typeparam name="T6">Method's sixth parameter type</typeparam>
        /// <returns>Method's behavior</returns>
        public MethodActionBehavior<TComponent, T1, T2, T3, T4, T5, T6> WithActionSignature<T1, T2, T3, T4, T5, T6>(string name, ParamSettings param1 = null, ParamSettings param2 = null, ParamSettings param3 = null, ParamSettings param4 = null, ParamSettings param5 = null, ParamSettings param6 = null)
            => WithSignature<MethodActionBehavior<TComponent, T1, T2, T3, T4, T5, T6>>(name, param1.OfType<T1>(), param2.OfType<T2>(), param3.OfType<T3>(), param4.OfType<T4>(), param5.OfType<T5>(), param6.OfType<T6>());

        /// <summary>
        /// Specifies a method with the specified name, returning a given type, and taking no parameters
        /// </summary>
        /// <param name="name">Name of the method</param>
        /// <typeparam name="TReturn">Method's return type</typeparam>
        /// <returns>Method's behavior</returns>
        public MethodFunctionBehavior<TComponent, TReturn> WithFunctionSignature<TReturn>(string name)
            => WithSignature<MethodFunctionBehavior<TComponent, TReturn>>(name);

        /// <summary>
        /// Specifies a method with the specified name, returning a given type, and taking the given parameter type
        /// </summary>
        /// <param name="name">Name of the method</param>
        /// <param name="param1">Settings for the first method parameter</param>
        /// <typeparam name="T1">Method's first parameter type</typeparam>
        /// <typeparam name="TReturn">Method's return type</typeparam>
        /// <returns>Method's behavior</returns>
        public MethodFunctionBehavior<TComponent, T1, TReturn> WithFunctionSignature<T1, TReturn>(string name, ParamSettings param1 = null)
            => WithSignature<MethodFunctionBehavior<TComponent, T1, TReturn>>(name, param1.OfType<T1>());

        /// <summary>
        /// Specifies a method with the specified name, returning a given type, and taking the given parameter types
        /// </summary>
        /// <param name="name">Name of the method</param>
        /// <param name="param1">Settings for the first method parameter</param>
        /// <param name="param2">Settings for the second method parameter</param>
        /// <typeparam name="T1">Method's first parameter type</typeparam>
        /// <typeparam name="T2">Method's second parameter type</typeparam>
        /// <typeparam name="TReturn">Method's return type</typeparam>
        /// <returns>Method's behavior</returns>
        public MethodFunctionBehavior<TComponent, T1, T2, TReturn> WithFunctionSignature<T1, T2, TReturn>(string name, ParamSettings param1 = null, ParamSettings param2 = null)
            => WithSignature<MethodFunctionBehavior<TComponent, T1, T2, TReturn>>(name, param1.OfType<T1>(), param2.OfType<T2>());

        /// <summary>
        /// Specifies a method with the specified name, returning a given type, and taking the given parameter types
        /// </summary>
        /// <param name="name">Name of the method</param>
        /// <param name="param1">Settings for the first method parameter</param>
        /// <param name="param2">Settings for the second method parameter</param>
        /// <param name="param3">Settings for the third method parameter</param>
        /// <typeparam name="T1">Method's first parameter type</typeparam>
        /// <typeparam name="T2">Method's second parameter type</typeparam>
        /// <typeparam name="T3">Method's third parameter type</typeparam>
        /// <typeparam name="TReturn">Method's return type</typeparam>
        /// <returns>Method's behavior</returns>
        public MethodFunctionBehavior<TComponent, T1, T2, T3, TReturn> WithFunctionSignature<T1, T2, T3, TReturn>(string name, ParamSettings param1 = null, ParamSettings param2 = null, ParamSettings param3 = null)
            => WithSignature<MethodFunctionBehavior<TComponent, T1, T2, T3, TReturn>>(name, param1.OfType<T1>(), param2.OfType<T2>(), param3.OfType<T3>());

        /// <summary>
        /// Specifies a method with the specified name, returning a given type, and taking the given parameter types
        /// </summary>
        /// <param name="name">Name of the method</param>
        /// <param name="param1">Settings for the first method parameter</param>
        /// <param name="param2">Settings for the second method parameter</param>
        /// <param name="param3">Settings for the third method parameter</param>
        /// <param name="param4">Settings for the fourth method parameter</param>
        /// <typeparam name="T1">Method's first parameter type</typeparam>
        /// <typeparam name="T2">Method's second parameter type</typeparam>
        /// <typeparam name="T3">Method's third parameter type</typeparam>
        /// <typeparam name="T4">Method's fourth parameter type</typeparam>
        /// <typeparam name="TReturn">Method's return type</typeparam>
        /// <returns>Method's behavior</returns>
        public MethodFunctionBehavior<TComponent, T1, T2, T3, T4, TReturn> WithFunctionSignature<T1, T2, T3, T4, TReturn>(string name, ParamSettings param1 = null, ParamSettings param2 = null, ParamSettings param3 = null, ParamSettings param4 = null)
            => WithSignature<MethodFunctionBehavior<TComponent, T1, T2, T3, T4, TReturn>>(name, param1.OfType<T1>(), param2.OfType<T2>(), param3.OfType<T3>(), param4.OfType<T4>());

        /// <summary>
        /// Specifies a method with the specified name, returning a given type, and taking the given parameter types
        /// </summary>
        /// <param name="name">Name of the method</param>
        /// <param name="param1">Settings for the first method parameter</param>
        /// <param name="param2">Settings for the second method parameter</param>
        /// <param name="param3">Settings for the third method parameter</param>
        /// <param name="param4">Settings for the fourth method parameter</param>
        /// <param name="param5">Settings for the fifth method parameter</param>
        /// <typeparam name="T1">Method's first parameter type</typeparam>
        /// <typeparam name="T2">Method's second parameter type</typeparam>
        /// <typeparam name="T3">Method's third parameter type</typeparam>
        /// <typeparam name="T4">Method's fourth parameter type</typeparam>
        /// <typeparam name="T5">Method's fifth parameter type</typeparam>
        /// <typeparam name="TReturn">Method's return type</typeparam>
        /// <returns>Method's behavior</returns>
        public MethodFunctionBehavior<TComponent, T1, T2, T3, T4, T5, TReturn> WithFunctionSignature<T1, T2, T3, T4, T5, TReturn>(string name, ParamSettings param1 = null, ParamSettings param2 = null, ParamSettings param3 = null, ParamSettings param4 = null, ParamSettings param5 = null)
            => WithSignature<MethodFunctionBehavior<TComponent, T1, T2, T3, T4, T5, TReturn>>(name, param1.OfType<T1>(), param2.OfType<T2>(), param3.OfType<T3>(), param4.OfType<T4>(), param5.OfType<T5>());

        /// <summary>
        /// Specifies a method with the specified name, returning a given type, and taking the given parameter types
        /// </summary>
        /// <param name="name">Name of the method</param>
        /// <param name="param1">Settings for the first method parameter</param>
        /// <param name="param2">Settings for the second method parameter</param>
        /// <param name="param3">Settings for the third method parameter</param>
        /// <param name="param4">Settings for the fourth method parameter</param>
        /// <param name="param5">Settings for the fifth method parameter</param>
        /// <param name="param6">Settings for the sixth method parameter</param>
        /// <typeparam name="T1">Method's first parameter type</typeparam>
        /// <typeparam name="T2">Method's second parameter type</typeparam>
        /// <typeparam name="T3">Method's third parameter type</typeparam>
        /// <typeparam name="T4">Method's fourth parameter type</typeparam>
        /// <typeparam name="T5">Method's fifth parameter type</typeparam>
        /// <typeparam name="T6">Method's sixth parameter type</typeparam>
        /// <typeparam name="TReturn">Method's return type</typeparam>
        /// <returns>Method's behavior</returns>
        public MethodFunctionBehavior<TComponent, T1, T2, T3, T4, T5, T6, TReturn> WithFunctionSignature<T1, T2, T3, T4, T5, T6, TReturn>(string name, ParamSettings param1 = null, ParamSettings param2 = null, ParamSettings param3 = null, ParamSettings param4 = null, ParamSettings param5 = null, ParamSettings param6 = null)
            => WithSignature<MethodFunctionBehavior<TComponent, T1, T2, T3, T4, T5, T6, TReturn>>(name, param1.OfType<T1>(), param2.OfType<T2>(), param3.OfType<T3>(), param4.OfType<T4>(), param5.OfType<T5>(), param6.OfType<T6>());

        /// <summary>
        /// Specifies a method whose signature matches the given Func expression
        /// </summary>
        /// <param name="expr">The expression which specifies the method</param>
        /// <returns>Method's behavior</returns>
        public MethodFunctionBehavior<TComponent, TReturn> WithFunctionSignature<TReturn>(Expression<Func<TComponent, TReturn>> expr)
            => FromExpression<MethodFunctionBehavior<TComponent, TReturn>>(expr);

        public MethodFunctionBehavior<TComponent, T1, TReturn> WithFunctionSignature<T1, TReturn>(Expression<Func<TComponent, TReturn>> expr)
            => FromExpression<MethodFunctionBehavior<TComponent, T1, TReturn>>(expr);

        public MethodFunctionBehavior<TComponent, T1, T2, TReturn> WithFunctionSignature<T1, T2, TReturn>(Expression<Func<TComponent, TReturn>> expr)
            => FromExpression<MethodFunctionBehavior<TComponent, T1, T2, TReturn>>(expr);

        public MethodFunctionBehavior<TComponent, T1, T2, T3, TReturn> WithFunctionSignature<T1, T2, T3, TReturn>(Expression<Func<TComponent, TReturn>> expr)
            => FromExpression<MethodFunctionBehavior<TComponent, T1, T2, T3, TReturn>>(expr);

        public MethodFunctionBehavior<TComponent, T1, T2, T3, T4, TReturn> WithFunctionSignature<T1, T2, T3, T4, TReturn>(Expression<Func<TComponent, TReturn>> expr)
            => FromExpression<MethodFunctionBehavior<TComponent, T1, T2, T3, T4, TReturn>>(expr);

        public MethodFunctionBehavior<TComponent, T1, T2, T3, T4, T5, TReturn> WithFunctionSignature<T1, T2, T3, T4, T5, TReturn>(Expression<Func<TComponent, TReturn>> expr)
            => FromExpression<MethodFunctionBehavior<TComponent, T1, T2, T3, T4, T5, TReturn>>(expr);

        public MethodFunctionBehavior<TComponent, T1, T2, T3, T4, T5, T6, TReturn> WithFunctionSignature<T1, T2, T3, T4, T5, T6, TReturn>(Expression<Func<TComponent, TReturn>> expr)
            => FromExpression<MethodFunctionBehavior<TComponent, T1, T2, T3, T4, T5, T6, TReturn>>(expr);
        
        /// <summary>
        /// Specifies a method whose signature matches the given Action expression
        /// </summary>
        /// <param name="expr">The expression which specifies the method</param>
        /// <returns>Method's behavior</returns>
        public MethodActionBehavior<TComponent> WithActionSignature(Expression<Action<TComponent>> expr)
            => FromExpression<MethodActionBehavior<TComponent>>(expr);

        public MethodActionBehavior<TComponent, T1> WithActionSignature<T1>(Expression<Action<TComponent>> expr)
            => FromExpression<MethodActionBehavior<TComponent, T1>>(expr);

        public MethodActionBehavior<TComponent, T1, T2> WithActionSignature<T1, T2>(Expression<Action<TComponent>> expr)
            => FromExpression<MethodActionBehavior<TComponent, T1, T2>>(expr);

        public MethodActionBehavior<TComponent, T1, T2, T3> WithActionSignature<T1, T2, T3>(Expression<Action<TComponent>> expr)
            => FromExpression<MethodActionBehavior<TComponent, T1, T2, T3>>(expr);

        public MethodActionBehavior<TComponent, T1, T2, T3, T4> WithActionSignature<T1, T2, T3, T4>(Expression<Action<TComponent>> expr)
            => FromExpression<MethodActionBehavior<TComponent, T1, T2, T3, T4>>(expr);

        public MethodActionBehavior<TComponent, T1, T2, T3, T4, T5> WithActionSignature<T1, T2, T3, T4, T5>(Expression<Action<TComponent>> expr)
            => FromExpression<MethodActionBehavior<TComponent, T1, T2, T3, T4, T5>>(expr);

        public MethodActionBehavior<TComponent, T1, T2, T3, T4, T5, T6> WithActionSignature<T1, T2, T3, T4, T5, T6>(Expression<Action<TComponent>> expr)
            => FromExpression<MethodActionBehavior<TComponent, T1, T2, T3, T4, T5, T6>>(expr);

        TBehavior FromExpression<TBehavior>(LambdaExpression expr)
            where TBehavior : MethodBehaviorBase<TComponent, TBehavior>
        {
            if (expr == null)
                throw new ArgumentNullException(nameof(expr));
            if (expr.Body is MethodCallExpression meth && typeof(TComponent).IsAssignableFrom(meth.Method.DeclaringType))
                return WithSignature(meth.Method) as TBehavior ?? throw new ArgumentException("Expression mismatched with type inputs.", nameof(expr));
            throw new UnexpectedMemberFindBehaviorException("Cannot resolve method from expression");
        }

        TThis WithMethodInfo<TThis>(MethodInfo info)
            where TThis : MethodBehaviorBase<TComponent, TThis>
            => (TThis)WithSignature(info);

        IMethodBehaviorInternal<TComponent> WithSignature(MethodInfo info)
            => (IMethodBehaviorInternal<TComponent>)WithSignature(info.Name, info.ReturnType,
                    info.GetParameters()
                        .Select(p =>
                            new ParamSettingsContainer()
                            {
                                Settings = null,
                                Type = p.ParameterType
                            })
                        .ToArray());

        T WithSignature<T>(string name, params ParamSettingsContainer[] paramTypes)
            where T: MethodBehaviorBase<TComponent, T>
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (paramTypes.Length > 6)
                throw new ArgumentException("Only six parameters to a function are supported at this time.");

            _behavior.Parse(out bool canGet, out bool canAdd, out bool throwIfNull);
            var result = _methods.Method<T>(canGet, canAdd, name, _access, paramTypes);

            if (throwIfNull && result == null)
                throw new UnexpectedMemberFindBehaviorException(string.Format("Could not {0} method {1}", canGet ? "get" : "add", name));

            return result;
        }

        IMethodBehavior<TComponent> WithSignature(string name, Type returnType, ParamSettingsContainer[] paramTypes)
        {
            return (IMethodBehavior<TComponent>)GetType()
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(m => m.Name == nameof(WithSignature) && m.IsGenericMethod && m.GetParameters().Length == 2)
                .MakeGenericMethod(MethodBehaviorBase<TComponent>.GetType(returnType, paramTypes, out _))
                .Invoke(this, new object[] { name, paramTypes });
        }
    }
}
