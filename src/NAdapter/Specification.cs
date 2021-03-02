//#define DEBUGIL   // Intended for debugging this assembly only. Allows saving off the generated type so that it can be inspected with PEVerify.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace NAdapter
{
    /// <summary>
    /// Entry point for new specifications
    /// </summary>
    public static class Specification
    {
        const string kAssembly = "NAdapter.Dynamic";

        static object s_typeLock = new object();
        static Dictionary<string, int> _reservedNames = new Dictionary<string, int>();
        static ModuleBuilder _module;
        static AssemblyBuilder _assembly;
        
        static Specification()
        {
            var assemblyName = new AssemblyName(kAssembly);
            _assembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            _module = _assembly.DefineDynamicModule(assemblyName.Name);
        }

        static bool TypeAlreadyExists(string name)
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(a => a.GetType(name))
                .FirstOrDefault(t => t != null)
                != null;
        }

        /// <summary>
        /// Makes a new adapter specification
        /// </summary>
        /// <typeparam name="TComponent">Type of component to adapt</typeparam>
        /// <returns>New specification</returns>
        public static Specification<TComponent> New<TComponent>()
            where TComponent : class
        {
            lock (s_typeLock)
            {
                string typeName = $"{kAssembly}.{typeof(TComponent).Name}Adapter";
                string proposedName = typeName;
                bool foundValue = _reservedNames.TryGetValue(typeName, out int value);
                if (!foundValue && !TypeAlreadyExists(proposedName))
                    _reservedNames[typeName] = 1;
                else
                {
                    if (!foundValue)
                        value = 1;

                    proposedName = $"{typeName}_{value++}";
                    for (; TypeAlreadyExists(proposedName); ++value)
                        proposedName = $"{typeName}_{value}";
                    _reservedNames[typeName] = value;
                }
                
                return new Specification<TComponent>(proposedName,
                    new Lazy<TypeBuilder>(() =>
                    {
                        return _module.DefineType(proposedName, TypeAttributes.Public, typeof(Object));
                    }));
            }
        }

#if DEBUGIL
        public static void Save(string path) => _assembly.Save(path);
#endif
    }

    /// <summary>
    /// Specification for an adapter type.
    /// </summary>
    /// <typeparam name="TComponent">Type of component to adapt</typeparam>
    public class Specification<TComponent> where TComponent : class
    {
        const Access DefaultAccess = Access.Public;
        const Behavior DefaultBehavior = Behavior.AddOrGet;

        string _typeName;
        bool _hasBuilt = false;
        object _buildLock = new object();
        PropertiesSpecification<TComponent> _properties = new PropertiesSpecification<TComponent>();
        MethodsSpecification<TComponent> _methods = new MethodsSpecification<TComponent>();
        ISpecification<TComponent> _userSpecification;
        Lazy<TypeBuilder> _tb;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tb">TypeBuilder initializer</param>
        /// <param name="attributeConverter">Attributes as setup on the Adapters collection</param>
        internal Specification(string typeName, Lazy<TypeBuilder> tb)
        {
            _tb = tb;
            _typeName = typeName;
        }

        /// <summary>
        /// Attributes to be applied to the adapter type
        /// </summary>
        public AttributeSpecification Attributes { get; private set; } = new AttributeSpecification(AttributeTargets.Class);

        /// <summary>
        /// Linq parameter for specifying method behaviors
        /// </summary>
        public LinqParam<TComponent> Linq { get; }
        
        /// <summary>
        /// Builds the adapter factory
        /// </summary>
        /// <returns>Adapter Factory</returns>
        public AdapterFactory<TComponent> Finish()
        {
            Type adapterType;
            lock (_buildLock)
            {
                if (_hasBuilt)
                    throw new SpecificationAlreadyFinishedException(_tb.Value.FullName);
                adapterType = CreateAdapterType();
                _hasBuilt = true;
            }

            var factoryType = typeof(AdapterFactory<,>).MakeGenericType(typeof(TComponent), adapterType);
            return (AdapterFactory<TComponent>)Activator.CreateInstance(factoryType);
        }
        
        /// <summary>
        /// Specifies a property
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <param name="access">The least restrictive access modifier for the property</param>
        /// <param name="behavior">The behavior to employ when returning the property</param>
        /// <returns>The property's behavior</returns>
        public PropertyBehavior<TComponent> SpecifyProperty(string name, Access access = DefaultAccess, Behavior behavior = DefaultBehavior)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            behavior.Parse(out bool canGet, out bool canAdd, out bool throwIfNull);

            var result = _properties.Property(canGet, canAdd, name, access);
            if (throwIfNull && result == null)
                throw new UnexpectedMemberFindBehaviorException(string.Format("Could not {0} property {1}", canGet ? "get" : "add", name));
            return result;
        }

        /// <summary>
        /// Specifies a property
        /// </summary>
        /// <typeparam name="T">An expression describing a component property with the
        /// same name as the property to specify</typeparam>
        /// <param name="expr">Expression</param>
        /// <param name="access">Least restrictive access modifier to apply to the property</param>
        /// <param name="behavior">The behavior to employ when returning the property</param>
        /// <returns>The property's behavior</returns>
        public PropertyBehavior<TComponent, T> SpecifyProperty<T>(Expression<Func<TComponent, T>> expr, Access access = DefaultAccess, Behavior behavior = DefaultBehavior)
        {
            if (expr == null)
                throw new ArgumentNullException(nameof(expr));
            var propertyInfo = expr.GetProperty();
            var result = SpecifyProperty(propertyInfo.Name, access, behavior);
            if (result != null && result.BackingPropertyInfo != null)
                return result.SpecifyBackingComponentProperty<T>(propertyInfo);
            return result as PropertyBehavior<TComponent, T>;
        }

        public PropertyBehavior<TComponent> SpecifyPropertyWeak<T>(Expression<Func<TComponent, T>> expr, Access access = DefaultAccess, Behavior behavior = DefaultBehavior)
        {
            if (expr == null)
                throw new ArgumentNullException(nameof(expr));

            var propertyInfo = expr.GetProperty();
            return SpecifyProperty(propertyInfo.Name, access, behavior);
        }

        /// <summary>
        /// Specifies a method
        /// </summary>
        /// <param name="access">The least restrictive access modifier to apply to the method</param>
        /// <param name="behavior">The behavior to employ when getting the property</param>
        /// <returns>The method's behavior</returns>
        public MethodBehaviorFinder<TComponent> SpecifyMethod(Access access = DefaultAccess, Behavior behavior = DefaultBehavior)
        {
            return new MethodBehaviorFinder<TComponent>(_methods, behavior, access);
        }

        /// <summary>
        /// Adds a user-defined specification
        /// </summary>
        /// <param name="specification">The specification</param>
        public void Specify(ISpecification<TComponent> specification)
            => _userSpecification = specification;
        
        /// <summary>
        /// Validates the specification. Any errors that will cause the type not to be
        /// created will be reported here. It need not be called, but it can be useful
        /// for debugging.
        /// </summary>
        /// <returns>Validation</returns>
        public TypeValidationResult Validate()
        {
            var result = new TypeValidationResult(_typeName);

            result.ValidateIdentifier(_typeName, true);

            foreach (var prop in _properties)
                prop.Validate(result.ValidateProperty(prop.Name, prop.PropertyType, prop.Decoration.GetterAccess.Convert(), prop.Decoration.SetterAccess.Convert()));

            foreach (var method in _methods)
                method.Validate(result.ValidateMethod(method.Name));

            foreach (var methodValidator in result.MethodGroups)
                methodValidator.Finish();

            foreach (var ev in typeof(TComponent).GetEvents())
                new EventBehavior<TComponent>(ev).Validate(result);

            return result;
        }

        /// <summary>
        /// Generated Code attribute to be applied to generated members.
        /// </summary>
        internal static readonly CustomAttributeBuilder GeneratedCode = new CustomAttributeBuilder(
                typeof(GeneratedCodeAttribute).GetConstructors().Single(),
                new object[]
                {
                    Assembly.GetAssembly(typeof(TypeSpecifier)).GetName().Name,
                    Assembly.GetAssembly(typeof(TypeSpecifier)).GetName().Version.ToString()
                });
        
        Type CreateAdapterType()
        {
            var componentType = typeof(TComponent);

            if (_userSpecification != null)
            {
                foreach (var prop in _properties.ToList())
                    prop.InvokeOn(_userSpecification);
                foreach (var method in _methods.Where(m => m.IsValid).ToList())
                    method.InvokeOn(_userSpecification);
                _userSpecification.OnMembersFinished(this);
            }

            var validation = Validate().Errors;
            if (validation.Length != 0)
                throw new InvalidTypeSpecificationException(validation);

            IAttributeConverter attributeConverter = Attributes;
            attributeConverter.HandleAttributes(AttributeTargets.Class, _tb.Value.SetCustomAttribute, CustomAttributeData.GetCustomAttributes(componentType).ToArray());

            _tb.Value.AddInterfaceImplementation(typeof(IAdapter<TComponent>));

            var iSourceProperty = typeof(IAdapter<TComponent>).GetProperty("Source");
            var sourcePropertyBehavior = new PropertyBehavior<TComponent, TComponent>("Source", Access.Public);
            sourcePropertyBehavior.SpecifyComponentBackingField<TComponent>();
            sourcePropertyBehavior.Decoration.IsVirtual = true;
            
            ConstructorBuilder ctorBuilder = _tb.Value.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
            ILGenerator ctorIL = ctorBuilder.GetILGenerator();
            ctorIL.Emit(OpCodes.Ldarg_0);
            ctorIL.Emit(OpCodes.Callvirt, typeof(Object).GetConstructor(Type.EmptyTypes));
            ctorIL.Emit(OpCodes.Ret);

            TypeSpecifier ts = new TypeSpecifier(_tb.Value);

            foreach (var action in ((IPropertyBehaviorInternal<TComponent>)sourcePropertyBehavior).DeclareProperty(ts, null, attributeConverter, iSourceProperty))
                action();
            var sourceField = sourcePropertyBehavior.ResultBackingField;

            List<Action> behaviorAdders = new List<Action>();

            foreach (var prop in _properties)
                behaviorAdders.AddRange(prop.DeclareProperty(ts, sourceField, attributeConverter, null));
            
            foreach (var method in _methods.Where(m => m.IsValid))
                behaviorAdders.AddRange(method.DeclareMethod(ts, sourceField, attributeConverter));

            foreach (var behaviorAdder in behaviorAdders)
                behaviorAdder();

            var resultType = _tb.Value.CreateType();
            ts.Finish();
            return resultType;
        }
    }
}
