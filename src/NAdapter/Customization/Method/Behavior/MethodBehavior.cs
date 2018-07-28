using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace NAdapter
{
    public interface IMethodBehavior<TComponent> where TComponent : class
    {
        string Name { get; }
        Type DelegateType { get; }
        MethodInfo BuiltMethodInfo { get; }
        void SpecifyMethod(MethodInfo info);
        MethodDecoration Decoration { get; }
        IEnumerable<ParamSettingsContainer> Parameters { get; }
    }

    internal interface IMethodBehaviorInternal<TComponent>: IMethodBehavior<TComponent>
        where TComponent : class
    {
        bool IsValid { get; }
        void Validate(MethodGroupValidationResult validation);
        void InvokeOn(ISpecification<TComponent> specification);
        IEnumerable<Action> DeclareMethod(TypeSpecifier tb, FieldBuilder sourceField, IAttributeConverter attributeConverter);
    }

    internal static class MethodBehaviorBase<TComponent>
        where TComponent : class
    {
        internal static TThis Make<TThis>(MethodInfo m)
            where TThis : IMethodBehaviorInternal<TComponent>
        {
            return Make<TThis>(m.Name, m.GetParameters()
                .Select(p => new ParamSettingsContainer()
                {
                    Settings = new ParamSettings(p),
                    Type = p.ParameterType
                }).ToArray(),
                m.ReturnType);
        }

        internal static TThis Make<TThis>(string name, ParamSettingsContainer[] parameters)
            where TThis : IMethodBehaviorInternal<TComponent>
        {
            Type returnType;

            if (typeof(TThis).Name.StartsWith("MethodActionBehavior"))
                returnType = typeof(void);
            else if (typeof(TThis).Name.StartsWith("MethodFunctionBehavior"))
                returnType = typeof(TThis).GetGenericArguments().Last();
            else
                throw new InvalidOperationException("Unknown subclass of MethodBehavior was requested.");

            return Make<TThis>(name, parameters, returnType);
        }

        internal static Type GetType(Type returnType, ParamSettingsContainer[] paramTypes, out string error)
        {
            Type template;
            List<Type> types = new List<Type>();
            types.Add(typeof(TComponent));
            types.AddRange(paramTypes.Select(p => p.Type.IsByRef ? p.Type.GetElementType() : p.Type));
            if (returnType == typeof(void))
            {
                switch (paramTypes.Length)
                {
                    case 0: template = typeof(MethodActionBehavior<>); break;
                    case 1: template = typeof(MethodActionBehavior<,>); break;
                    case 2: template = typeof(MethodActionBehavior<,,>); break;
                    case 3: template = typeof(MethodActionBehavior<,,,>); break;
                    case 4: template = typeof(MethodActionBehavior<,,,,>); break;
                    case 5: template = typeof(MethodActionBehavior<,,,,,>); break;
                    case 6: template = typeof(MethodActionBehavior<,,,,,,>); break;
                    default: error = "Too many types in method overload. Not supported yet."; return null;
                }
            }
            else
            {
                switch (paramTypes.Length)
                {
                    case 0: template = typeof(MethodFunctionBehavior<,>); break;
                    case 1: template = typeof(MethodFunctionBehavior<,,>); break;
                    case 2: template = typeof(MethodFunctionBehavior<,,,>); break;
                    case 3: template = typeof(MethodFunctionBehavior<,,,,>); break;
                    case 4: template = typeof(MethodFunctionBehavior<,,,,,>); break;
                    case 5: template = typeof(MethodFunctionBehavior<,,,,,,>); break;
                    case 6: template = typeof(MethodFunctionBehavior<,,,,,,,>); break;
                    default: error = "Too many types in method overload. Not supported yet."; return null;
                }

                types.Add(returnType);
            }
            error = null;
            return template.MakeGenericType(types.ToArray());
        }

        private static TThis Make<TThis>(string name, ParamSettingsContainer[] parameters, Type returnType)
            where TThis : IMethodBehaviorInternal<TComponent>
        {
            List<object> args = new List<object>();
            args.Add(name);
            args.AddRange(parameters);

            var genType = GetType(returnType, parameters, out string error);
            if (genType == null)
                return (TThis)(IMethodBehaviorInternal<TComponent>)(new MethodErrorPlaceholder<TComponent>(error));
            return (TThis)Activator.CreateInstance(genType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, args.ToArray(), CultureInfo.CurrentCulture);
        }
    }

    internal class MethodErrorPlaceholder<TComponent> : IMethodBehaviorInternal<TComponent> where TComponent : class
    {
        string _warning;
        
        internal MethodErrorPlaceholder(string warning) => _warning = warning;

        string IMethodBehavior<TComponent>.Name => null;

        bool IMethodBehaviorInternal<TComponent>.IsValid { get => false; }

        Type IMethodBehavior<TComponent>.DelegateType => throw new NotImplementedException();

        MethodInfo IMethodBehavior<TComponent>.BuiltMethodInfo => throw new NotImplementedException();

        MethodDecoration IMethodBehavior<TComponent>.Decoration => throw new NotImplementedException();

        IEnumerable<ParamSettingsContainer> IMethodBehavior<TComponent>.Parameters => throw new NotImplementedException();

        IEnumerable<Action> IMethodBehaviorInternal<TComponent>.DeclareMethod(TypeSpecifier tb, FieldBuilder sourceField, IAttributeConverter attributeConverter)
        {
            throw new NotImplementedException();
        }

        void IMethodBehaviorInternal<TComponent>.InvokeOn(ISpecification<TComponent> specification)
        {
            throw new NotImplementedException();
        }

        void IMethodBehavior<TComponent>.SpecifyMethod(MethodInfo info)
        {
            throw new NotImplementedException();
        }

        void IMethodBehaviorInternal<TComponent>.Validate(MethodGroupValidationResult validation)
        {
            validation.AddWarning(_warning);
        }
    }

    /// <summary>
    /// Behavior for a method to be defined on the adapter
    /// </summary>
    /// <typeparam name="TComponent">The type of component being adapted</typeparam>
    public abstract class MethodBehaviorBase<TComponent, TThis> : IMethodBehaviorInternal<TComponent>
        where TComponent : class
        where TThis : MethodBehaviorBase<TComponent, TThis>
    {
        MethodDecoration _decoration = new MethodDecoration();
        MethodBehavior _behavior;
        Type _returnType;
        ParamSettingsContainer[] _paramSettings;

        bool IMethodBehaviorInternal<TComponent>.IsValid { get => true; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of the method</param>
        /// <param name="paramSettings">Settings for the parameters of the method</param>
        internal MethodBehaviorBase(string name, Type returnType, ParamSettingsContainer[] paramSettings)
        {
            _returnType = returnType;
            _decoration.PublicName = name;
            _paramSettings = paramSettings;
        }

        public Type ReturnType { get { return _returnType; } }
        
        public IEnumerable<ParamSettingsContainer> Parameters { get { return _paramSettings; } }

        public MethodInfo BackingMethodInfo { get; private set; }

        /// <summary>
        /// Decoration
        /// </summary>
        /// <returns>Decoration</returns>
        public MethodDecoration Decoration { get { return _decoration; } }

        /// <summary>
        /// Method name
        /// </summary>
        public string Name { get { return _decoration.PublicName; } }
        
        public override string ToString()
            => string.Format("{0} {1}({2})", ReturnType, Name, String.Join(", ", (object[])_paramSettings));

        /// <summary>
        /// Specifies a Func LINQ expression upon which the method's behavior should be based
        /// </summary>
        /// <typeparam name="T">Type returned by the method</typeparam>
        /// <param name="expr">The LINQ expression which defines the method behavior</param>
        /// <returns>this. For chained method calls.</returns>
        public TThis SpecifyLinq<T>(Expression<Func<TComponent, T>> expr)
            => SpecifyLinq((LambdaExpression)expr);

        /// <summary>
        /// Specifies an Action LINQ expression upon which the method's behavior should be used
        /// </summary>
        /// <param name="expr">The LINQ expression which defines the method behavior</param>
        /// <returns>this. For chained method calls.</returns>
        public TThis SpecifyLinq(Expression<Action<TComponent>> expr)
            => SpecifyLinq((LambdaExpression)expr);

        /// <summary>
        /// Specifies the method to invoke a given delegate
        /// </summary>
        /// <param name="invoke">The delegate to invoke</param>
        /// <returns>this. For chained method calls.</returns>
        public TThis SpecifyDelegate(Action invoke)
            => SpecifyDelegate(invoke, typeof(void));

        /// <summary>
        /// Specifies the method to invoke a given delegate
        /// </summary>
        /// <typeparam name="T">Type of the delegate's only parameter</typeparam>
        /// <param name="param1">Specification for how to fill out the delegate's parameter</param>
        /// <param name="invoke">The delegate</param>
        /// <returns>this. For chained method calls.</returns>
        public TThis SpecifyDelegate<T>(Param<T> param1, Action<T> invoke)
            => SpecifyDelegate(invoke, typeof(void), param1);

        /// <summary>
        /// Specifies the method to invoke a given delegate
        /// </summary>
        /// <typeparam name="T1">Type of the delegate's first parameter</typeparam>
        /// <typeparam name="T2">Type of the delegate's second parameter</typeparam>
        /// <param name="param1">Specification for how to fill out the delegate's first parameter</param>
        /// <param name="param2">Specification for how to fill out the delegate's second parameter</param>
        /// <param name="invoke">The delegate</param>
        /// <returns>this. For chained method calls.</returns>
        public TThis SpecifyDelegate<T1, T2>(Param<T1> param1, Param<T2> param2, Action<T1, T2> invoke)
            => SpecifyDelegate(invoke, typeof(void), param1, param2);

        /// <summary>
        /// Specifies the method to invoke a given delegate
        /// </summary>
        /// <typeparam name="T1">Type of the delegate's first parameter</typeparam>
        /// <typeparam name="T2">Type of the delegate's second parameter</typeparam>
        /// <typeparam name="T3">Type of the delegate's third parameter</typeparam>
        /// <param name="param1">Specification for how to fill out the delegate's first parameter</param>
        /// <param name="param2">Specification for how to fill out the delegate's second parameter</param>
        /// <param name="param3">Specification for how to fill out the delegate's third parameter</param>
        /// <param name="invoke">The delegate</param>
        /// <returns>this. For chained method calls.</returns>
        public TThis SpecifyDelegate<T1, T2, T3>(Param<T1> param1, Param<T2> param2, Param<T3> param3, Action<T1, T2, T3> invoke)
            => SpecifyDelegate(invoke, typeof(void), param1, param2, param3);

        /// <summary>
        /// Specifies the method to invoke a given delegate
        /// </summary>
        /// <typeparam name="T1">Type of the delegate's first parameter</typeparam>
        /// <typeparam name="T2">Type of the delegate's second parameter</typeparam>
        /// <typeparam name="T3">Type of the delegate's third parameter</typeparam>
        /// <typeparam name="T4">Type of the delegate's fourth parameter</typeparam>
        /// <param name="param1">Specification for how to fill out the delegate's first parameter</param>
        /// <param name="param2">Specification for how to fill out the delegate's second parameter</param>
        /// <param name="param3">Specification for how to fill out the delegate's third parameter</param>
        /// <param name="param4">Specification for how to fill out the delegate's fourth parameter</param>
        /// <param name="invoke">The delegate</param>
        /// <returns>this. For chained method calls.</returns>
        public TThis SpecifyDelegate<T1, T2, T3, T4>(Param<T1> param1, Param<T2> param2, Param<T3> param3, Param<T4> param4, Action<T1, T2, T3, T4> invoke)
            => SpecifyDelegate(invoke, typeof(void), param1, param2, param3, param4);

        /// <summary>
        /// Specifies the method to invoke a given delegate
        /// </summary>
        /// <typeparam name="T1">Type of the delegate's first parameter</typeparam>
        /// <typeparam name="T2">Type of the delegate's second parameter</typeparam>
        /// <typeparam name="T3">Type of the delegate's third parameter</typeparam>
        /// <typeparam name="T4">Type of the delegate's fourth parameter</typeparam>
        /// <typeparam name="T5">Type of the delegate's fifth parameter</typeparam>
        /// <param name="param1">Specification for how to fill out the delegate's first parameter</param>
        /// <param name="param2">Specification for how to fill out the delegate's second parameter</param>
        /// <param name="param3">Specification for how to fill out the delegate's third parameter</param>
        /// <param name="param4">Specification for how to fill out the delegate's fourth parameter</param>
        /// <param name="param5">Specification for how to fill out the delegate's fifth parameter</param>
        /// <param name="invoke">The delegate</param>
        /// <returns>this. For chained method calls.</returns>
        public TThis SpecifyDelegate<T1, T2, T3, T4, T5>(Param<T1> param1, Param<T2> param2, Param<T3> param3, Param<T4> param4, Param<T5> param5, Action<T1, T2, T3, T4, T5> invoke)
            => SpecifyDelegate(invoke, typeof(void), param1, param2, param3, param4, param5);

        /// <summary>
        /// Specifies the method to invoke a given delegate
        /// </summary>
        /// <typeparam name="T1">Type of the delegate's first parameter</typeparam>
        /// <typeparam name="T2">Type of the delegate's second parameter</typeparam>
        /// <typeparam name="T3">Type of the delegate's third parameter</typeparam>
        /// <typeparam name="T4">Type of the delegate's fourth parameter</typeparam>
        /// <typeparam name="T5">Type of the delegate's fifth parameter</typeparam>
        /// <typeparam name="T6">Type of the delegate's sixth parameter</typeparam>
        /// <param name="param1">Specification for how to fill out the delegate's first parameter</param>
        /// <param name="param2">Specification for how to fill out the delegate's second parameter</param>
        /// <param name="param3">Specification for how to fill out the delegate's third parameter</param>
        /// <param name="param4">Specification for how to fill out the delegate's fourth parameter</param>
        /// <param name="param5">Specification for how to fill out the delegate's fifth parameter</param>
        /// <param name="param6">Specification for how to fill out the delegate's sixth parameter</param>
        /// <param name="invoke">The delegate</param>
        /// <returns>this. For chained method calls.</returns>
        public TThis SpecifyDelegate<T1, T2, T3, T4, T5, T6>(Param<T1> param1, Param<T2> param2, Param<T3> param3, Param<T4> param4, Param<T5> param5, Param<T6> param6, Action<T1, T2, T3, T4, T5, T6> invoke)
            => SpecifyDelegate(invoke, typeof(void), param1, param2, param3, param4, param5, param6);

        /// <summary>
        /// Specifies the method to invoke a given delegate
        /// </summary>
        /// <typeparam name="TReturn">The type to be returned by the method</typeparam>
        /// <param name="invoke">The delegate</param>
        /// <returns>this. For chained method calls</returns>
        public TThis SpecifyDelegate<TReturn>(Func<TReturn> invoke)
            => SpecifyDelegate(invoke, typeof(TReturn));

        /// <summary>
        /// Specifies the method to invoke a given delegate
        /// </summary>
        /// <typeparam name="T1">Type of the delegate's first parameter</typeparam>
        /// <typeparam name="TReturn">The type to be returned by the method</typeparam>
        /// <param name="param1">Specification for how to fill out the delegate's first parameter</param>
        /// <param name="invoke">The delegate</param>
        /// <returns>this. For chained method calls.</returns>
        public TThis SpecifyDelegate<T1, TReturn>(Param<T1> param1, Func<T1, TReturn> invoke)
            => SpecifyDelegate(invoke, typeof(TReturn), param1);

        /// <summary>
        /// Specifies the method to invoke a given delegate
        /// </summary>
        /// <typeparam name="T1">Type of the delegate's first parameter</typeparam>
        /// <typeparam name="T2">Type of the delegate's second parameter</typeparam>
        /// <typeparam name="TReturn">The type to be returned by the method</typeparam>
        /// <param name="param1">Specification for how to fill out the delegate's first parameter</param>
        /// <param name="param2">Specification for how to fill out the delgate's second parameter</param>
        /// <param name="invoke">The delegate</param>
        /// <returns>this. For chained method calls.</returns>
        public TThis SpecifyDelegate<T1, T2, TReturn>(Param<T1> param1, Param<T2> param2, Func<T1, T2, TReturn> invoke)
            => SpecifyDelegate(invoke, typeof(TReturn), param1, param2);

        /// <summary>
        /// Specifies the method to invoke a given delegate
        /// </summary>
        /// <typeparam name="T1">Type of the delegate's first parameter</typeparam>
        /// <typeparam name="T2">Type of the delegate's second parameter</typeparam>
        /// <typeparam name="T3">Type of the delegate's third parameter</typeparam>
        /// <typeparam name="TReturn">The type to be returned by the method</typeparam>
        /// <param name="param1">Specification for how to fill out the delegate's first parameter</param>
        /// <param name="param2">Specification for how to fill out the delegate's second parameter</param>
        /// <param name="param3">Specification for how to fill out the delegate's third parameter</param>
        /// <param name="invoke">The delegate</param>
        /// <returns>this. For chained method calls.</returns>
        public TThis SpecifyDelegate<T1, T2, T3, TReturn>(Param<T1> param1, Param<T2> param2, Param<T3> param3, Func<T1, T2, T3, TReturn> invoke)
            => SpecifyDelegate(invoke, typeof(TReturn), param1, param2, param3);

        /// <summary>
        /// Specifies the method to invoke a given delegate
        /// </summary>
        /// <typeparam name="T1">Type of the delegate's first parameter</typeparam>
        /// <typeparam name="T2">Type of the delegate's second parameter</typeparam>
        /// <typeparam name="T3">Type of the delegate's third parameter</typeparam>
        /// <typeparam name="T4">Type of the delegate's fourth parameter</typeparam>
        /// <typeparam name="TReturn">The type to be returned by the method</typeparam>
        /// <param name="param1">Specification for how to fill out the delegate's first parameter</param>
        /// <param name="param2">Specification for how to fill out the delegate's second parameter</param>
        /// <param name="param3">Specification for how to fill out the delegate's third parameter</param>
        /// <param name="param4">Specification for how to fill out the delegate's fourth parameter</param>
        /// <param name="invoke">The delegate</param>
        /// <returns>this. For chained method calls.</returns>
        public TThis SpecifyDelegate<T1, T2, T3, T4, TReturn>(Param<T1> param1, Param<T2> param2, Param<T3> param3, Param<T4> param4, Func<T1, T2, T3, T4, TReturn> invoke)
            => SpecifyDelegate(invoke, typeof(TReturn), param1, param2, param3, param4);

        /// <summary>
        /// Specifies the method to invoke a given delegate
        /// </summary>
        /// <typeparam name="T1">Type of the delegate's first parameter</typeparam>
        /// <typeparam name="T2">Type of the delegate's second parameter</typeparam>
        /// <typeparam name="T3">Type of the delegate's third parameter</typeparam>
        /// <typeparam name="T4">Type of the delegate's fourth parameter</typeparam>
        /// <typeparam name="T5">Type of the delegate's fifth parameter</typeparam>
        /// <typeparam name="TReturn">The type to be returned by the method</typeparam>
        /// <param name="param1">Specification for how to fill out the delegate's first parameter</param>
        /// <param name="param2">Specification for how to fill out the delegate's second parameter</param>
        /// <param name="param3">Specification for how to fill out the delegate's third parameter</param>
        /// <param name="param4">Specification for how to fill out the delegate's fourth parameter</param>
        /// <param name="param5">Specification for how to fill out the delegate's fifth parameter</param>
        /// <param name="invoke">The delegate</param>
        /// <returns>this. For chained method calls.</returns>
        public TThis SpecifyDelegate<T1, T2, T3, T4, T5, TReturn>(Param<T1> param1, Param<T2> param2, Param<T3> param3, Param<T4> param4, Param<T5> param5, Func<T1, T2, T3, T4, T5, TReturn> invoke)
            => SpecifyDelegate(invoke, typeof(TReturn), param1, param2, param3, param4, param5);

        /// <summary>
        /// Specifies the method to invoke a given delegate
        /// </summary>
        /// <typeparam name="T1">Type of the delegate's first parameter</typeparam>
        /// <typeparam name="T2">Type of the delegate's second parameter</typeparam>
        /// <typeparam name="T3">Type of the delegate's third parameter</typeparam>
        /// <typeparam name="T4">Type of the delegate's fourth parameter</typeparam>
        /// <typeparam name="T5">Type of the delegate's fifth parameter</typeparam>
        /// <typeparam name="T6">Type of the delegate's sixth parameter</typeparam>
        /// <typeparam name="TReturn">The type to be returned by the method</typeparam>
        /// <param name="param1">Specification for how to fill out the delegate's first parameter</param>
        /// <param name="param2">Specification for how to fill out the delegate's second parameter</param>
        /// <param name="param3">Specification for how to fill out the delegate's third parameter</param>
        /// <param name="param4">Specification for how to fill out the delegate's fourth parameter</param>
        /// <param name="param5">Specification for how to fill out the delegate's fifth parameter</param>
        /// <param name="param6">Specification for how to fill out the delegate's sixth parameter</param>
        /// <param name="invoke">The delegate</param>
        /// <returns>this. For chained method calls.</returns>
        public TThis SpecifyDelegate<T1, T2, T3, T4, T5, T6, TReturn>(Param<T1> param1, Param<T2> param2, Param<T3> param3, Param<T4> param4, Param<T5> param5, Param<T6> param6, Func<T1, T2, T3, T4, T5, T6, TReturn> invoke)
            => SpecifyDelegate(invoke, typeof(TReturn), param1, param2, param3, param4, param5, param6);

        /// <summary>
        /// Specifies the method to be based on a method from the component
        /// </summary>
        /// <param name="method">The component's method</param>
        /// <returns>this. For chained method calls.</returns>
        public void SpecifyMethod(MethodInfo method)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            _decoration.IsVirtual = method.IsVirtual;
            _returnType = method.ReturnType;
            BackingMethodInfo = method;
            _behavior = new MethodPassToComponentMethodBehavior(method, method.Name, typeof(TComponent));
        }

        /// <summary>
        /// Specifies a linq expression to use for the method's logic.
        /// </summary>
        /// <param name="expr">Linq expression</param>
        /// <returns>this. For chained method calls.</returns>
        public TThis SpecifyLinq(LambdaExpression expr)
        {
            if (expr == null)
                throw new ArgumentNullException(nameof(expr));
            if (expr.Parameters.Count != 1 || expr.Parameters[0].Type != typeof(TComponent))
                throw new ArgumentException("Invalid parameters on lambda.", nameof(expr));

            var visitor = new LinqExpressionConverter<TComponent>();
            _returnType = expr.ReturnType;
            var result = visitor.Visit(expr) as LambdaExpression;
            List<Param> paramTypes = new List<Param>();
            List<ParameterExpression> paramExprs = new List<ParameterExpression>();
            paramExprs.Add(expr.Parameters.Single());
            paramTypes.Add(new SourceParam<TComponent>());
            foreach (var p in visitor.GetParameters())
            {
                paramTypes.Add(p.Parameter);
                p.Parameter.Validate(_paramSettings);
                paramExprs.Add(p.Expression);
            }
            BackingMethodInfo = null;
            var lambda = Expression.Lambda(result.Body, paramExprs);
            _behavior = new MethodBehaviorFromExpression<TComponent>(lambda, _decoration.PublicName, paramTypes.ToArray());
            return (TThis)this;
        }
        
        /// <summary>
        /// A type of delegate to which this method can be assigned
        /// </summary>
        public Type DelegateType
        {
            get
            {
                if (_returnType == typeof(void) || _returnType == null)
                    return GetParameterTypes().GetActionWithGenerics();
                else
                    return GetParameterTypes().GetFuncWithGenerics(ReturnType);
            }
        }

        /// <summary>
        /// The System.Reflection.MethodInfo built by this behavior
        /// </summary>
        public MethodInfo BuiltMethodInfo { get; private set; }

        /// <summary>
        /// Gets an array of the types expected by the parameters of the method
        /// </summary>
        /// <returns>Type array</returns>
        internal Type[] GetParameterTypes() => _paramSettings.Select(p => p.Type).ToArray();

        /// <summary>
        /// Sets the parameter settings
        /// </summary>
        /// <param name="settings">Settings</param>
        internal void SetParamSettings(ParamSettingsContainer[] settings)
        {
            if (_paramSettings != null && _paramSettings.Length == settings.Length)
            {
                var parameters = BackingMethodInfo?.GetParameters() ?? new ParameterInfo[settings.Length];

                for (int i = 0; i < settings.Length; ++i)
                    if (settings[i].Settings != null)
                        _paramSettings[i].Settings.LoadFrom(settings[i].Settings, parameters[i]);
            }
            else
                _paramSettings = settings;
        }
        
        void IMethodBehaviorInternal<TComponent>.Validate(MethodGroupValidationResult group)
        {
            var validation = group.ValidateOverload(ReturnType);
            if (!ValidateBackingMethod(out string warning))
                validation.AddWarning(warning);

            bool defaultSoFar = true;
            for (int i = _paramSettings.Length - 1; i >= 0; --i)
            {
                var paramSetting = _paramSettings[i];
                if (paramSetting.Settings != null)
                {
                    var paramValidation = validation.ValidateParameter(paramSetting.Settings.Name, paramSetting.Settings.Default != null, paramSetting.Settings.Default?.Value, paramSetting.Type);
                    if (paramSetting.Settings?.Default != null)
                    {
                        if (!defaultSoFar)
                            paramValidation.AddError($"Method {_decoration.PublicName} Parameter {paramSetting.Settings.Name} has default value but is not at the end.");
                    }
                    else
                        defaultSoFar = false;

                    validation.ValidateIdentifier(paramSetting.Settings.Name, false);
                }
                else
                {
                    validation.ValidateParameter($"_{i}", false, null, paramSetting.Type);
                    defaultSoFar = false;
                }
                
            }

            _behavior.Validate(validation);
        }

        void IMethodBehaviorInternal<TComponent>.InvokeOn(ISpecification<TComponent> specification)
            => ProtectedInvokeOn(specification);

        protected abstract void ProtectedInvokeOn(ISpecification<TComponent> specification);

        IEnumerable<Action> IMethodBehaviorInternal<TComponent>.DeclareMethod(TypeSpecifier tb, FieldBuilder sourceField, IAttributeConverter attributeConverter)
        {
            List<Action> adders = new List<Action>();
            if (!_decoration.IsHidden && ValidateBackingMethod(out _))
            {
                var methodAttributes = _decoration.AccessModifier.Convert() | MethodAttributes.HideBySig;
                if (_decoration.IsVirtual)
                    methodAttributes |= MethodAttributes.Virtual;

                MethodBuilder builder = tb.TypeBuilder.DefineMethod(_decoration.PublicName, methodAttributes, _returnType, GetParameterTypes());

                IAttributeConverter aggregateConverter = new AttributeConverterAggregate(_decoration.Attributes, attributeConverter);
                aggregateConverter.HandleAttributes(AttributeTargets.Method, builder.SetCustomAttribute,
                    BackingMethodInfo != null ? CustomAttributeData.GetCustomAttributes(BackingMethodInfo).ToArray() : new CustomAttributeData[0]);

                int unusedParamName = 0;

                for (int i = 0; i < _paramSettings.Length; ++i)
                    if (_paramSettings[i].Settings != null)
                        _paramSettings[i].Settings.BuildParameter(builder, i, aggregateConverter, ref unusedParamName);

                BuiltMethodInfo = builder;

                adders.Add(() =>
                {
                    ILGenerator il = builder.GetILGenerator();
                    _behavior.Emit(il, sourceField, tb);
                });
            }
            return adders;
        }

        bool ValidateBackingMethod(out string warning)
        {
            if (BackingMethodInfo == null)
            {
                warning = null;
                return true;
            }

            StringBuilder sb = new StringBuilder();

            if (BackingMethodInfo.ContainsGenericParameters)
                sb.AppendLine($"Method {BackingMethodInfo.Name} has generic parameter and will be skipped. That feature will be added in {Roadmap.Features.GenericMethods.Version()}.");

            var referenceParameter = BackingMethodInfo.GetParameters().FirstOrDefault(p => p.ParameterType.IsByRef);

            if (referenceParameter != null)
            {
                if (referenceParameter.IsOut)
                    sb.AppendLine($"Method {BackingMethodInfo.Name} has out parameter and will be skipped. That feature will be added in {Roadmap.Features.OutParams.Version()}.");
                else
                    sb.AppendLine($"Method {BackingMethodInfo.Name} has ref parameter and will be skipped. That feature will be added in {Roadmap.Features.RefParams.Version()}.");
            }

            if (BackingMethodInfo.IsStatic)
                sb.AppendLine($"Method {BackingMethodInfo.Name} is static and will be skipped. That feature will be added in {Roadmap.Features.StaticMethods.Version()}.");

            warning = sb.ToString();
            return string.IsNullOrEmpty(warning);
        }

        TThis SpecifyDelegate(Delegate invoke, Type returnType, params Param[] paramTypes)
        {
            if (invoke == null)
                throw new ArgumentNullException(nameof(invoke));

            foreach (var p in paramTypes)
                p.Validate(_paramSettings);
            _behavior = new MethodFromDelegateBehavior(invoke, _decoration.PublicName, paramTypes);
            _returnType = returnType;
            BackingMethodInfo = null;
            return (TThis)this;
        }
    }

    /// <summary>
    /// A method behavior
    /// </summary>
    internal abstract class MethodBehavior
    {
        /// <summary>
        /// Validates this method
        /// </summary>
        /// <param name="validation">Validation</param>
        internal abstract void Validate(MethodValidationResult validation);

        /// <summary>
        /// Emits the IL for the method
        /// </summary>
        /// <param name="il">The ILGenerator</param>
        /// <param name="sourceField">The field in which the source component is stored</param>
        /// <param name="typeBeingBuilt">The type under construction</param>
        internal abstract void Emit(ILGenerator il, FieldBuilder sourceField, TypeSpecifier typeBeingBuilt);
    }

    public class MethodFunctionBehavior<TComponent, TReturn> : MethodBehaviorBase<TComponent, MethodFunctionBehavior<TComponent, TReturn>>
        where TComponent : class
    {
        internal MethodFunctionBehavior(string name)
            :base(name, typeof(TReturn), new ParamSettingsContainer[0]) { }

        protected override void ProtectedInvokeOn(ISpecification<TComponent> specification) => specification.OnMethod(this);
    }

    public class MethodFunctionBehavior<TComponent, T1, TReturn> : MethodBehaviorBase<TComponent, MethodFunctionBehavior<TComponent, T1, TReturn>>
        where TComponent : class
    {
        internal MethodFunctionBehavior(string name, ParamSettingsContainer p)
            : base(name, typeof(TReturn), new[] { p }) { }

        protected override void ProtectedInvokeOn(ISpecification<TComponent> specification) => specification.OnMethod(this);
    }

    public class MethodFunctionBehavior<TComponent, T1, T2, TReturn> : MethodBehaviorBase<TComponent, MethodFunctionBehavior<TComponent, T1, T2, TReturn>>
        where TComponent : class
    {
        internal MethodFunctionBehavior(string name, ParamSettingsContainer p1, ParamSettingsContainer p2)
            : base(name, typeof(TReturn), new[] { p1, p2}) { }

        protected override void ProtectedInvokeOn(ISpecification<TComponent> specification) => specification.OnMethod(this);
    }

    public class MethodFunctionBehavior<TComponent, T1, T2, T3, TReturn> : MethodBehaviorBase<TComponent, MethodFunctionBehavior<TComponent, T1, T2, T3, TReturn>>
        where TComponent : class
    {
        internal MethodFunctionBehavior(string name, ParamSettingsContainer p1, ParamSettingsContainer p2, ParamSettingsContainer p3)
            : base(name, typeof(TReturn), new[] { p1, p2, p3 }) { }

        protected override void ProtectedInvokeOn(ISpecification<TComponent> specification) => specification.OnMethod(this);
    }

    public class MethodFunctionBehavior<TComponent, T1, T2, T3, T4, TReturn> : MethodBehaviorBase<TComponent, MethodFunctionBehavior<TComponent, T1, T2, T3, T4, TReturn>>
        where TComponent : class
    {
        internal MethodFunctionBehavior(string name, ParamSettingsContainer p1, ParamSettingsContainer p2, ParamSettingsContainer p3, ParamSettingsContainer p4)
            : base(name, typeof(TReturn), new[] { p1, p2, p3, p4 }) { }

        protected override void ProtectedInvokeOn(ISpecification<TComponent> specification) => specification.OnMethod(this);
    }

    public class MethodFunctionBehavior<TComponent, T1, T2, T3, T4, T5, TReturn> : MethodBehaviorBase<TComponent, MethodFunctionBehavior<TComponent, T1, T2, T3, T4, T5, TReturn>>
        where TComponent : class
    {
        internal MethodFunctionBehavior(string name, ParamSettingsContainer p1, ParamSettingsContainer p2, ParamSettingsContainer p3, ParamSettingsContainer p4, ParamSettingsContainer p5)
            : base(name, typeof(TReturn), new[] { p1, p2, p3, p4, p5 }) { }

        protected override void ProtectedInvokeOn(ISpecification<TComponent> specification) => specification.OnMethod(this);
    }

    public class MethodFunctionBehavior<TComponent, T1, T2, T3, T4, T5, T6, TReturn> : MethodBehaviorBase<TComponent, MethodFunctionBehavior<TComponent, T1, T2, T3, T4, T5, T6, TReturn>>
        where TComponent : class
    {
        internal MethodFunctionBehavior(string name, ParamSettingsContainer p1, ParamSettingsContainer p2, ParamSettingsContainer p3, ParamSettingsContainer p4, ParamSettingsContainer p5, ParamSettingsContainer p6)
            : base(name, typeof(TReturn), new[] { p1, p2, p3, p4, p5, p6 }) { }

        protected override void ProtectedInvokeOn(ISpecification<TComponent> specification) => specification.OnMethod(this);
    }

    public class MethodActionBehavior<TComponent> : MethodBehaviorBase<TComponent, MethodActionBehavior<TComponent>>
        where TComponent : class
    {
        internal MethodActionBehavior(string name)
            : base(name, typeof(void), new ParamSettingsContainer[0]) { }

        protected override void ProtectedInvokeOn(ISpecification<TComponent> specification) => specification.OnMethod(this);
    }

    public class MethodActionBehavior<TComponent, T1> : MethodBehaviorBase<TComponent, MethodActionBehavior<TComponent, T1>>
        where TComponent : class
    {
        internal MethodActionBehavior(string name, ParamSettingsContainer p1)
            : base(name, typeof(void), new[] { p1 }) { }

        protected override void ProtectedInvokeOn(ISpecification<TComponent> specification) => specification.OnMethod(this);
    }

    public class MethodActionBehavior<TComponent, T1, T2> : MethodBehaviorBase<TComponent, MethodActionBehavior<TComponent, T1, T2>>
        where TComponent : class
    {
        internal MethodActionBehavior(string name, ParamSettingsContainer p1, ParamSettingsContainer p2)
            : base(name, typeof(void), new[] { p1, p2 }) { }

        protected override void ProtectedInvokeOn(ISpecification<TComponent> specification) => specification.OnMethod(this);
    }

    public class MethodActionBehavior<TComponent, T1, T2, T3> : MethodBehaviorBase<TComponent, MethodActionBehavior<TComponent, T1, T2, T3>>
        where TComponent : class
    {
        internal MethodActionBehavior(string name, ParamSettingsContainer p1, ParamSettingsContainer p2, ParamSettingsContainer p3)
            : base(name, typeof(void), new[] { p1, p2, p3 }) { }

        protected override void ProtectedInvokeOn(ISpecification<TComponent> specification) => specification.OnMethod(this);
    }

    public class MethodActionBehavior<TComponent, T1, T2, T3, T4> : MethodBehaviorBase<TComponent, MethodActionBehavior<TComponent, T1, T2, T3, T4>>
        where TComponent : class
    {
        internal MethodActionBehavior(string name, ParamSettingsContainer p1, ParamSettingsContainer p2, ParamSettingsContainer p3, ParamSettingsContainer p4)
            : base(name, typeof(void), new[] { p1, p2, p3, p4 }) { }

        protected override void ProtectedInvokeOn(ISpecification<TComponent> specification) => specification.OnMethod(this);
    }

    public class MethodActionBehavior<TComponent, T1, T2, T3, T4, T5> : MethodBehaviorBase<TComponent, MethodActionBehavior<TComponent, T1, T2, T3, T4, T5>>
        where TComponent : class
    {
        internal MethodActionBehavior(string name, ParamSettingsContainer p1, ParamSettingsContainer p2, ParamSettingsContainer p3, ParamSettingsContainer p4, ParamSettingsContainer p5)
            : base(name, typeof(void), new[] { p1, p2, p3, p4, p5 }) { }

        protected override void ProtectedInvokeOn(ISpecification<TComponent> specification) => specification.OnMethod(this);
    }

    public class MethodActionBehavior<TComponent, T1, T2, T3, T4, T5, T6> : MethodBehaviorBase<TComponent, MethodActionBehavior<TComponent, T1, T2, T3, T4, T5, T6>>
        where TComponent : class
    {
        internal MethodActionBehavior(string name, ParamSettingsContainer p1, ParamSettingsContainer p2, ParamSettingsContainer p3, ParamSettingsContainer p4, ParamSettingsContainer p5, ParamSettingsContainer p6)
            : base(name, typeof(void), new[] { p1, p2, p3, p4, p5, p6 }) { }

        protected override void ProtectedInvokeOn(ISpecification<TComponent> specification) => specification.OnMethod(this);
    }
}
