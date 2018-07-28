using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;

namespace NAdapter
{
    public interface IPropertyBehavior<TComponent>
        where TComponent : class
    {
        PropertyDecoration Decoration { get; }
        Type PropertyType { get; }
        string Name { get; }
        PropertyInfo BackingPropertyInfo { get; }
    }

    internal interface IPropertyBehaviorInternal<TComponent>: IPropertyBehavior<TComponent>
        where TComponent : class
    {
        PropertyInfo ResultPropertyInfo { get; }
        IPropertyBehavior<TComponent> SpecifyBackingComponentProperty(PropertyInfo info);
        void Validate(PropertyValidationResult validation);
        void InvokeOn(ISpecification<TComponent> specification);
        IPropertyBehaviorInternal<TComponent> GetFinalDescendant();
        IEnumerable<Action> DeclareProperty(TypeSpecifier tb, FieldBuilder sourceField, IAttributeConverter attributeConverter, PropertyInfo propertyToOverride);
    }

    internal delegate void OnPropertyBehaviorReplaced<TComponent>(IPropertyBehaviorInternal<TComponent> old, IPropertyBehaviorInternal<TComponent> newer)
        where TComponent : class;
    
    public class PropertyBehavior<TComponent, TValue>: PropertyBehavior<TComponent> where TComponent : class
    {
        public override Type PropertyType => typeof(TValue);

        protected override void ProtectedInvokeOn(ISpecification<TComponent> specification)
        {
            specification.OnProperty(this);
        }

        internal PropertyBehavior(string name, Access access):
            base(name, access)
        { }

        public PropertyBehavior<TComponent, TValue> AddGetterFilter(Expression<Func<TValue, TValue>> expr)
        {
            Behavior.AddGetterFilter<TComponent>(expr);
            return this;
        }

        public PropertyBehavior<TComponent, TValue> AddGetterFilter(Expression<Func<TValue, TComponent, TValue>> expr)
        {
            Behavior.AddGetterFilter<TComponent>(expr);
            return this;
        }

        public PropertyBehavior<TComponent, TValue> AddGetterFilter(Expression<Func<TValue, TComponent, Action<TValue>, TValue>> expr)
        {
            Behavior.AddGetterFilter<TComponent>(expr);
            return this;
        }

        public PropertyBehavior<TComponent, TValue> AddGetterFilter(LambdaExpression expr)
        {
            Behavior.AddGetterFilter<TComponent>(expr);
            return this;
        }

        public PropertyBehavior<TComponent, TValue> AddSetterFilter(Expression<Func<TValue, TValue>> expr)
        {
            Behavior.AddSetterFilter<TComponent>(expr);
            return this;
        }

        public PropertyBehavior<TComponent, TValue> AddSetterFilter(Expression<Func<TValue, TComponent, TValue>> expr)
        {
            Behavior.AddSetterFilter<TComponent>(expr);
            return this;
        }

        public PropertyBehavior<TComponent, TValue> AddSetterFilter(Expression<Func<TValue, TComponent, Func<TValue>, TValue>> expr)
        {
            Behavior.AddSetterFilter<TComponent>(expr);
            return this;
        }

        public PropertyBehavior<TComponent, TValue> AddSetterFilter(LambdaExpression expr)
        {
            Behavior.AddSetterFilter<TComponent>(expr);
            return this;
        }
    }

    /// <summary>
    /// Behavior of a property
    /// </summary>
    /// <typeparam name="TComponent">Type of component being adapted</typeparam>
    public class PropertyBehavior<TComponent>: IPropertyBehaviorInternal<TComponent> where TComponent : class
    {
        PropertyDecoration _decoration = new PropertyDecoration();
        PropertyBehavior _behavior;
        
        internal PropertyBehavior Behavior
        {
            get { return _behavior; }
            private set
            {
                _behavior = value;
                _decoration.Behavior = value;
            }
        }

        PropertiesSpecification<TComponent> _parent;
        IPropertyBehaviorInternal<TComponent> _child;

        IPropertyBehaviorInternal<TComponent> IPropertyBehaviorInternal<TComponent>.GetFinalDescendant()
            => _child?.GetFinalDescendant() ?? this;

        internal void SetParent(PropertiesSpecification<TComponent> specification) => _parent = specification;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Property name</param>
        /// <param name="access">Property's least restrictive access modifier</param>
        internal PropertyBehavior(string name, Access access)
        {
            _decoration.PublicName = name;
            _decoration.AccessModifier = access;
        }
        
        /// <summary>
        /// Property name
        /// </summary>
        public string Name { get { return _decoration.PublicName; } }

        /// <summary>
        /// Property type
        /// </summary>
        public virtual Type PropertyType { get { return null; } }

        protected virtual void ProtectedInvokeOn(ISpecification<TComponent> specification) { throw new InvalidOperationException("Property with no type."); }

        /// <summary>
        /// PropertyInfo of the component property which this property forwards.
        /// Null if the property's behavior is defined otherwise
        /// </summary>
        public PropertyInfo BackingPropertyInfo { get; private set; }

        /// <summary>
        /// Decoration
        /// </summary>
        /// <returns>Decoration</returns>
        public PropertyDecoration Decoration { get { return _decoration; } }

        void ThisReplacedWith(PropertyBehavior<TComponent> newer)
        {
            if (_parent != null)
                _parent.Replaced(this, newer);
            _child = _child ?? newer;
        }
        
        public override string ToString() => $"{PropertyType} {Name}";

        /// <summary>
        /// Configures the property to be auto-implemented, a-la { get; set; }
        /// </summary>
        /// <typeparam name="TProperty">Type of property to implement</typeparam>
        /// <returns>this. For chained method calls.</returns>
        public PropertyBehavior<TComponent, TProperty> SpecifyAutoImplemented<TProperty>()
        {
            var result = new PropertyBehavior<TComponent, TProperty>(Name, _decoration.AccessModifier)
            {
                BackingPropertyInfo = null,
                Behavior = new PropertyWithBackingFieldBehavior<TProperty>()
            };
            
            result.Decoration.SpecifyPublicGetter();
            result.Decoration.SpecifyPublicSetter();
            ThisReplacedWith(result);
            return result;
        }

        /// <summary>
        /// Configures the property to forward a given component property
        /// </summary>
        /// <typeparam name="TProperty">Type of property</typeparam>
        /// <param name="expr">Expression specifying the component property</param>
        /// <returns>this. For chained method calls.</returns>
        public PropertyBehavior<TComponent, TProperty> SpecifyBackingComponentProperty<TProperty>(Expression<Func<TComponent, TProperty>> expr)
            => SpecifyBackingComponentProperty<TProperty>(expr.GetProperty());

        void IPropertyBehaviorInternal<TComponent>.InvokeOn(ISpecification<TComponent> specification)
            => ProtectedInvokeOn(specification);

        /// <summary>
        /// Configures the property to forward a given component property
        /// </summary>
        /// <param name="info"><see cref="System.Reflection.PropertyInfo"/> of the component property
        /// to be forwarded</param>
        /// <returns>this. For chained method calls.</returns>
        public PropertyBehavior<TComponent, T> SpecifyBackingComponentProperty<T>(PropertyInfo info)
        {
            var result = new PropertyBehavior<TComponent, T>(Name, _decoration.AccessModifier)
            {
                Behavior = new PropertyWithBackingComponentProperty(info),
                BackingPropertyInfo = info
            };
            
            if (info.GetGetMethod(true) != null)
            {
                if (info.GetGetMethod(true).IsPublic)
                    result.Decoration.SpecifyPublicGetter();
                else
                    result.Decoration.SpecifyPrivateGetter();
            }
            if (info.GetSetMethod(true) != null)
            {
                if (info.GetSetMethod(true).IsPublic)
                    result.Decoration.SpecifyPublicSetter();
                else
                    result.Decoration.SpecifyPrivateSetter();
            }
            ThisReplacedWith(result);
            return result;
        }

        public IPropertyBehavior<TComponent> SpecifyBackingComponentProperty(PropertyInfo info)
        {
            return (IPropertyBehavior<TComponent>)GetType()
                .GetMethods()
                .FirstOrDefault(m =>
                                m.Name == nameof(SpecifyBackingComponentProperty) &&
                                m.IsGenericMethod &&
                                m.GetParameters()[0].ParameterType == typeof(PropertyInfo))
                .MakeGenericMethod(info.PropertyType)
                .Invoke(this, new object[] { info });
        }

        /// <summary>
        /// Configures the property to run a given delegate for its getter and setter
        /// </summary>
        /// <typeparam name="TValue">Type of property</typeparam>
        /// <param name="getter">Getter function. Set to null for a writeonly property</param>
        /// <param name="setter">Setter function. Set to null for a readonly property</param>
        /// <returns>this. For chained method calls.</returns>
        public PropertyBehavior<TComponent, T> SpecifyDelegates<T>(Func<TComponent, T> getter, Action<TComponent, T> setter)
        {
            var result = new PropertyBehavior<TComponent, T>(Name, _decoration.AccessModifier)
            {
                Behavior = new PropertyWithBackingDelegates<TComponent, T>(_decoration.PublicName, getter, setter)
            };
            ThisReplacedWith(result);
            return result;
        }

        /// <summary>
        /// The <see cref="System.Reflection.PropertyInfo"/> of the property created when this behavior ran.
        /// </summary>
        public PropertyInfo ResultPropertyInfo { get; private set; }

        /// <summary>
        /// The backing field created when this behavior ran, if any.
        /// </summary>
        internal FieldBuilder ResultBackingField { get { return _behavior.BackingField; } }

        /// <summary>
        /// Validates this property specification
        /// </summary>
        /// <param name="validation">Validation</param>
        void IPropertyBehaviorInternal<TComponent>.Validate(PropertyValidationResult validation)
        {
            if (PropertyType == null)
                validation.AddError($"Property {Name} has no property type.");
            if (!Validate(out string warning))
                validation.AddWarning(warning);
            if (!_decoration.HasSetter && !_decoration.HasGetter)
                validation.AddError($"Property {Name} has neither getter nor setter.");
            Behavior.Validate(validation);
        }

        /// <summary>
        /// Configures the property to be the property which returns the adapted component
        /// </summary>
        /// <returns>this. For chained method calls.</returns>
        internal void SpecifyComponentBackingField<T>()
        {
            Behavior = new PropertyWithBackingFieldBehavior<TComponent>();
            _decoration.SpecifyPublicGetter();
            _decoration.SpecifyPublicSetter();
        }

        IEnumerable<Action> IPropertyBehaviorInternal<TComponent>.DeclareProperty(TypeSpecifier tb, FieldBuilder sourceField, IAttributeConverter attributeConverter, PropertyInfo propertyToOverride)
        {
            List<Action> a = new List<Action>();
            if (!_decoration.IsHidden)
            {
                PropertyBuilder prop = tb.TypeBuilder.DefineProperty(Name,
                    propertyToOverride != null ? propertyToOverride.Attributes : PropertyAttributes.None,
                    PropertyType, Type.EmptyTypes);

                MethodAttributes meth = MethodAttributes.SpecialName | MethodAttributes.HideBySig;
                if (_decoration.IsVirtual)
                    meth |= MethodAttributes.Virtual;

                IAttributeConverter attributes = new AttributeConverterAggregate(_decoration.Attributes, attributeConverter);
                attributes.HandleAttributes(AttributeTargets.Property, prop.SetCustomAttribute, BackingPropertyInfo == null ? new CustomAttributeData[0] : CustomAttributeData.GetCustomAttributes(BackingPropertyInfo).ToArray());

                Lazy<TypeSpecifier> nestedType = new Lazy<TypeSpecifier>(() => tb.CreateDisplayClass());

                MethodBuilder propGetter = null;
                if (_decoration.HasGetter)
                {
                    propGetter = tb.TypeBuilder.DefineMethod($"get_{_decoration.PublicName}", meth | _decoration.GetterAccess.Value, PropertyType, Type.EmptyTypes);
                    prop.SetGetMethod(propGetter);
                    if (propertyToOverride?.CanRead ?? false)
                        tb.TypeBuilder.DefineMethodOverride(propGetter, propertyToOverride.GetGetMethod());
                }

                MethodBuilder propSetter = null;
                if (_decoration.HasSetter)
                {
                    propSetter = tb.TypeBuilder.DefineMethod($"set_{_decoration.PublicName}", meth | _decoration.SetterAccess.Value, null, new Type[] { PropertyType });
                    prop.SetSetMethod(propSetter);
                    if (propertyToOverride?.CanWrite ?? false)
                        tb.TypeBuilder.DefineMethodOverride(propSetter, propertyToOverride.GetSetMethod());
                }

                if (_decoration.HasGetter)
                    a.Add(() =>
                    {
                        ILGenerator getterIL = propGetter.GetILGenerator();
                        _behavior.EmitGetter(getterIL, sourceField, nestedType, tb, Name, propSetter);
                    });

                if (_decoration.HasSetter)
                    a.Add(() =>
                    {
                        ILGenerator setterIL = propSetter.GetILGenerator();
                        _behavior.EmitSetter(setterIL, sourceField, nestedType, tb, Name, propGetter);
                    });

                ResultPropertyInfo = prop;
            }
            return a;
        }
        
        bool Validate(out string warning)
        {
            if (BackingPropertyInfo == null)
            {
                warning = null;
                return true;
            }

            StringBuilder sb = new StringBuilder();
            if (BackingPropertyInfo.GetIndexParameters().Length != 0)
                sb.AppendLine($"Property {BackingPropertyInfo.Name} has index parameter and will be skipped. That feature will be added in {Roadmap.Features.IndexProperties.Version()}.");
            if (BackingPropertyInfo.GetAccessors(true).Any(s => s.IsStatic))
                sb.AppendLine($"Property {BackingPropertyInfo.Name} is static and will be skipped. That feature will be added in {Roadmap.Features.StaticProperties.Version()}.");

            warning = sb.ToString();
            return string.IsNullOrEmpty(warning);
        }
    }

    /// <summary>
    /// Behavior of a property
    /// </summary>
    internal abstract class PropertyBehavior
    {
        List<LambdaExpression> _getterFilters = new List<LambdaExpression>();
        List<LambdaExpression> _setterFilters = new List<LambdaExpression>();
        
        protected abstract Type PropertyType { get; }

        /// <summary>
        /// Indicates whether this property can be read
        /// </summary>
        internal abstract bool CanRead { get; }

        /// <summary>
        /// Indicates whether this property can be written
        /// </summary>
        internal abstract bool CanWrite { get; }

        /// <summary>
        /// Emits the IL of the getter
        /// </summary>
        /// <param name="getterIL">The ILGenerator</param>
        /// <param name="sourceField">The field of the source component</param>
        protected abstract void ProtectedEmitGetter(ILGenerator getterIL, FieldBuilder sourceField, Lazy<TypeSpecifier> nestedType, TypeSpecifier thisType, string name);

        /// <summary>
        /// Emits the IL of the setter
        /// </summary>
        /// <param name="setterIL">The ILGenerator</param>
        /// <param name="sourceField">The field of the source component</param>
        protected abstract void ProtectedEmitSetter(ILGenerator setterIL, FieldBuilder sourceField, Lazy<TypeSpecifier> nestedType, TypeSpecifier thisType, string name);

        private void EmitFilters(IEnumerable<LambdaExpression> lambdas, Lazy<TypeSpecifier> nestedType, ILGenerator il, FieldBuilder sourceField, string name, MethodBuilder otherMethod, Type otherDelegateType)
        {
            foreach (var lambda in lambdas)
            {
                Type[] parameters = lambda.Parameters.Select(p => p.Type).ToArray();
                var staticMethod = nestedType.Value.TypeBuilder.DefineMethod(
                    nestedType.Value.GetMethodName($"get_{name}"),
                    MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig,
                    CallingConventions.Standard, PropertyType, parameters);
                
                // We'd really like to compile this straight into our method,
                // but `LambdaExpression.CompileToMethod` only takes static methods,
                // so the next best thing is to use this attribute to tell the JITter
                // that it should be inlined.
                staticMethod.SetCustomAttribute(new CustomAttributeBuilder(
                    typeof(MethodImplAttribute).GetConstructor(new Type[] { typeof(MethodImplOptions) }),
                    new object[1] { MethodImplOptions.AggressiveInlining }));
                
                lambda.CompileToMethod(staticMethod);

                if (parameters.Length > 1)
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, sourceField);
                }
                if (parameters.Length > 2)
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldftn, otherMethod);
                    il.Emit(OpCodes.Newobj, otherDelegateType.GetConstructors().Single());
                }

                il.Emit(OpCodes.Call, staticMethod);
            }
        }

        internal void EmitGetter(ILGenerator getterIL, FieldBuilder sourceField, Lazy<TypeSpecifier> nestedType, TypeSpecifier thisType, string name, MethodBuilder setter)
        {
            ProtectedEmitGetter(getterIL, sourceField, nestedType, thisType, name);
            EmitFilters(_getterFilters, nestedType, getterIL, sourceField, name, setter, typeof(Action<>).MakeGenericType(PropertyType));
            getterIL.Emit(OpCodes.Ret);
        }

        internal void EmitSetter(ILGenerator setterIL, FieldBuilder sourceField, Lazy<TypeSpecifier> nestedType, TypeSpecifier thisType, string name, MethodBuilder getter)
        {
            setterIL.DeclareLocal(PropertyType);
            setterIL.Emit(OpCodes.Ldarg_1);
            EmitFilters(_setterFilters, nestedType, setterIL, sourceField, name, getter, typeof(Func<>).MakeGenericType(PropertyType));
            setterIL.Emit(OpCodes.Stloc_0);
            ProtectedEmitSetter(setterIL, sourceField, nestedType, thisType, name);
            setterIL.Emit(OpCodes.Ret);
        }
        
        internal void AddGetterFilter<TComponent>(LambdaExpression expr)
            where TComponent : class
        {
            Validate<TComponent>(expr, typeof(Action<>).MakeGenericType(PropertyType));
            _getterFilters.Add(expr);
        }

        void Validate<TComponent>(LambdaExpression expr, Type validThirdParam)
        {
            if (expr.ReturnType != PropertyType)
                throw new ArgumentException($"Cannot add filter returning type {expr.ReturnType} to property of type {PropertyType}.");
            switch (expr.Parameters.Count)
            {
                case 3:
                    if (expr.Parameters[2].Type != validThirdParam)
                        throw new ArgumentException($"Filter parameter type mismatch: filter third parameter must be of type {validThirdParam}");
                    goto case 2;
                case 2:
                    if (expr.Parameters[1].Type != typeof(TComponent))
                        throw new ArgumentException($"Filter parameter type mismatch: filter second parameter must be the source component.");
                    goto case 1;
                case 1:
                    if (expr.Parameters[0].Type != PropertyType)
                        throw new ArgumentException($"Filter parameter type mismatch: filter must take property type first.");
                    break;
                case 0: throw new ArgumentException("Cannot add filter taking no parameters. Re-writing the property behavior may be more appropriate.");
                default: throw new ArgumentException($"Filter parameter count error: filter must take no more than three parameters.");
            }
        }
        
        internal void AddSetterFilter<TComponent>(LambdaExpression expr)
            where TComponent : class
        {
            Validate<TComponent>(expr, typeof(Func<>).MakeGenericType(PropertyType));
            _setterFilters.Add(expr);
        }

        /// <summary>
        /// The backing field created for the property, if any
        /// </summary>
        protected internal FieldBuilder BackingField { get; protected set; }

        internal void Validate(PropertyValidationResult validation)
        {
            if (_getterFilters.Any(g => g.Parameters.Count > 2) && !CanWrite)
                validation.AddError("Getter filter is registered to require access to setter, but property is readonly.");
            if (_setterFilters.Any(g => g.Parameters.Count > 2) && !CanRead)
                validation.AddError("Setter filter is registered to require access to getter, but property is writeonly.");
        }
    }
}
