using System;
using System.Reflection.Emit;

namespace NAdapter
{
    /// <summary>
    /// Parameters for a delegate
    /// </summary>
    public abstract class Param
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        internal Param() { }

        /// <summary>
        /// Type to be returned by the param
        /// </summary>
        internal abstract Type Type { get; }

        /// <summary>
        /// Emits the IL for this parameter
        /// </summary>
        /// <param name="il">IL generator</param>
        /// <param name="sourceField">The field containing the source component</param>
        /// <param name="typeBeingBuilt">The type being built</param>
        internal abstract void Emit(ILGenerator il, FieldBuilder sourceField, Type typeBeingBuilt);

        /// <summary>
        /// Validates the parameter. Does nothing by default.
        /// </summary>
        /// <param name="paramSettings">Param settings containers to validate against</param>
        internal virtual void Validate(ParamSettingsContainer[] paramSettings) { }

        /// <summary>
        /// Validates this parameter
        /// </summary>
        /// <param name="validation">Validation</param>
        internal abstract void Validate(SubparameterValidationResult validation);


        /// <summary>
        /// Get the method's argument from a given 1-based index.
        /// </summary>
        /// <typeparam name="T">The type of argument</typeparam>
        /// <param name="index">The 1-based index of the argument to return</param>
        /// <returns>Parameter</returns>
        public static Param<T> Arg<T>(int index) => new ParamDecorator<T>(new ArgumentParam(index, typeof(T)));

        /// <summary>
        /// Get the method's argument of a given name.
        /// </summary>
        /// <typeparam name="T">The type of argument</typeparam>
        /// <param name="name">The name of the argument to return</param>
        /// <returns>Parameter</returns>
        public static Param<T> Arg<T>(string name) => new ParamDecorator<T>(new ArgumentParam(name, typeof(T)));

        /// <summary>
        /// Get the source component
        /// </summary>
        /// <typeparam name="T">Type of component being decorated</typeparam>
        /// <param name="spec">The specification</param>
        /// <returns>Parameter</returns>
        public static Param<TComponent> Source<TComponent>(Specification<TComponent> spec = null) where TComponent : class
            => new SourceParam<TComponent>();

        /// <summary>
        /// Declares a constant value for a parameter
        /// </summary>
        /// <param name="value">The value of the parameter</param>
        /// <returns>Parameter</returns>
        public static Param<string> Declare(string value) => new DeclareStringParam(value);

        /// <summary>
        /// Declares a constant value for a parameter
        /// </summary>
        /// <param name="value">The value of the parameter</param>
        /// <returns>Parameter</returns>
        public static Param<bool> Declare(bool value) => new DeclareBoolParam(value);

        /// <summary>
        /// Declares a constant value for a parameter
        /// </summary>
        /// <param name="value">The value of the parameter</param>
        /// <returns>Parameter</returns>
        public static Param<byte> Declare(byte value) => new DeclareByteParam(value);

        /// <summary>
        /// Declares a constant value for a parameter
        /// </summary>
        /// <param name="value">The value of the parameter</param>
        /// <returns>Parameter</returns>
        public static Param<sbyte> Declare(sbyte value) => new DeclareSByteParam(value);

        /// <summary>
        /// Declares a constant value for a parameter
        /// </summary>
        /// <param name="value">The value of the parameter</param>
        /// <returns>Parameter</returns>
        public static Param<short> Declare(short value) => new DeclareShortParam(value);

        /// <summary>
        /// Declares a constant value for a parameter
        /// </summary>
        /// <param name="value">The value of the parameter</param>
        /// <returns>Parameter</returns>
        public static Param<ushort> Declare(ushort value) => new DeclareUShortParam(value);

        /// <summary>
        /// Declares a constant value for a parameter
        /// </summary>
        /// <param name="value">The value of the parameter</param>
        /// <returns>Parameter</returns>
        public static Param<int> Declare(int value) => new DeclareIntParam(value);

        /// <summary>
        /// Declares a constant value for a parameter
        /// </summary>
        /// <param name="value">The value of the parameter</param>
        /// <returns>Parameter</returns>
        public static Param<uint> Declare(uint value) => new DeclareUIntParam(value);

        /// <summary>
        /// Declares a constant value for a parameter
        /// </summary>
        /// <param name="value">The value of the parameter</param>
        /// <returns>Parameter</returns>
        public static Param<char> Declare(char value) => new DeclareCharParam(value);

        /// <summary>
        /// Declares a constant value for a parameter
        /// </summary>
        /// <param name="value">The value of the parameter</param>
        /// <returns>Parameter</returns>
        public static Param<double> Declare(double value) => new DeclareDoubleParam(value);

        /// <summary>
        /// Declares a constant value for a parameter
        /// </summary>
        /// <param name="value">The value of the parameter</param>
        /// <returns>Parameter</returns>
        public static Param<float> Declare(float value) => new DeclareSingleParam(value);

        /// <summary>
        /// Declares a constant value for a parameter
        /// </summary>
        /// <param name="value">The value of the parameter</param>
        /// <returns>Parameter</returns>
        public static Param<long> Declare(long value) => new DeclareLongParam(value);

        /// <summary>
        /// Declares a constant value for a parameter
        /// </summary>
        /// <param name="value">The value of the parameter</param>
        /// <returns>Parameter</returns>
        public static Param<ulong> Declare(ulong value) => new DeclareULongParam(value);

        /// <summary>
        /// Declares a constant value of null for a parameter
        /// </summary>
        /// <returns>Parameter</returns>
        public static Param<T> DeclareNull<T>() where T : class => new DeclareNullParam<T>();

        /// <summary>
        /// Declares a property getter for a parameter
        /// </summary>
        /// <typeparam name="T">Type of property</typeparam>
        /// <param name="property">Property to get</param>
        /// <returns>Parameter</returns>
        public static Param<Func<T>> Getter<TComponent, T>(PropertyBehavior<TComponent, T> property) where TComponent : class
            => new ParamDecorator<Func<T>>(new PropertyGetterParam<TComponent>(property, typeof(T)));

        /// <summary>
        /// Declares a property setter for a parameter
        /// </summary>
        /// <typeparam name="T">Type of property</typeparam>
        /// <param name="property">Property to set</param>
        /// <returns>Parameter</returns>
        public static Param<Action<T>> Setter<TComponent, T>(PropertyBehavior<TComponent, T> property) where TComponent : class
            => new ParamDecorator<Action<T>>(new PropertySetterParam<TComponent>(property, typeof(T)));

        public static Param<Action> Method<TComponent>(MethodActionBehavior<TComponent> method) where TComponent : class
            => new ParamDecorator<Action>(new MethodParam<TComponent>(method, typeof(Action)));

        public static Param<Action<T1>> Method<TComponent, T1>(MethodActionBehavior<TComponent, T1> method) where TComponent : class
            => new ParamDecorator<Action<T1>>(new MethodParam<TComponent>(method, typeof(Action<T1>)));

        public static Param<Action<T1, T2>> Method<TComponent, T1, T2>(MethodActionBehavior<TComponent, T1, T2> method) where TComponent : class
            => new ParamDecorator<Action<T1, T2>>(new MethodParam<TComponent>(method, typeof(Action<T1, T2>)));

        public static Param<Action<T1, T2, T3>> Method<TComponent, T1, T2, T3>(MethodActionBehavior<TComponent, T1, T2, T3> method) where TComponent : class
            => new ParamDecorator<Action<T1, T2, T3>>(new MethodParam<TComponent>(method, typeof(Action<T1, T2, T3>)));

        public static Param<Action<T1, T2, T3, T4>> Method<TComponent, T1, T2, T3, T4>(MethodActionBehavior<TComponent, T1, T2, T3, T4> method) where TComponent : class
            => new ParamDecorator<Action<T1, T2, T3, T4>>(new MethodParam<TComponent>(method, typeof(Action<T1, T2, T3, T4>)));

        public static Param<Action<T1, T2, T3, T4, T5>> Method<TComponent, T1, T2, T3, T4, T5>(MethodActionBehavior<TComponent, T1, T2, T3, T4, T5> method) where TComponent : class
            => new ParamDecorator<Action<T1, T2, T3, T4, T5>>(new MethodParam<TComponent>(method, typeof(Action<T1, T2, T3, T4, T5>)));

        public static Param<Action<T1, T2, T3, T4, T5, T6>> Method<TComponent, T1, T2, T3, T4, T5, T6>(MethodActionBehavior<TComponent, T1, T2, T3, T4, T5, T6> method) where TComponent : class
            => new ParamDecorator<Action<T1, T2, T3, T4, T5, T6>>(new MethodParam<TComponent>(method, typeof(Action<T1, T2, T3, T4, T5, T6>)));

        public static Param<Func<TReturn>> Method<TComponent, TReturn>(MethodFunctionBehavior<TComponent, TReturn> method) where TComponent : class
            => new ParamDecorator<Func<TReturn>>(new MethodParam<TComponent>(method, typeof(Func<TReturn>)));

        public static Param<Func<T1, TReturn>> Method<TComponent, T1, TReturn>(MethodFunctionBehavior<TComponent, T1, TReturn> method) where TComponent : class
            => new ParamDecorator<Func<T1, TReturn>>(new MethodParam<TComponent>(method, typeof(Func<T1, TReturn>)));

        public static Param<Func<T1, T2, TReturn>> Method<TComponent, T1, T2, TReturn>(MethodFunctionBehavior<TComponent, T1, T2, TReturn> method) where TComponent : class
            => new ParamDecorator<Func<T1, T2, TReturn>>(new MethodParam<TComponent>(method, typeof(Func<T1, T2, TReturn>)));

        public static Param<Func<T1, T2, T3, TReturn>> Method<TComponent, T1, T2, T3, TReturn>(MethodFunctionBehavior<TComponent, T1, T2, T3, TReturn> method) where TComponent : class
            => new ParamDecorator<Func<T1, T2, T3, TReturn>>(new MethodParam<TComponent>(method, typeof(Func<T1, T2, T3, TReturn>)));

        public static Param<Func<T1, T2, T3, T4, TReturn>> Method<TComponent, T1, T2, T3, T4, TReturn>(MethodFunctionBehavior<TComponent, T1, T2, T3, T4, TReturn> method) where TComponent : class
            => new ParamDecorator<Func<T1, T2, T3, T4, TReturn>>(new MethodParam<TComponent>(method, typeof(Func<T1, T2, T3, T4, TReturn>)));

        public static Param<Func<T1, T2, T3, T4, T5, TReturn>> Method<TComponent, T1, T2, T3, T4, T5, TReturn>(MethodFunctionBehavior<TComponent, T1, T2, T3, T4, T5, TReturn> method) where TComponent : class
            => new ParamDecorator<Func<T1, T2, T3, T4, T5, TReturn>>(new MethodParam<TComponent>(method, typeof(Func<T1, T2, T3, T4, T5, TReturn>)));

        public static Param<Func<T1, T2, T3, T4, T5, T6, TReturn>> Method<TComponent, T1, T2, T3, T4, T5, T6, TReturn>(MethodFunctionBehavior<TComponent, T1, T2, T3, T4, T5, T6, TReturn> method) where TComponent : class
            => new ParamDecorator<Func<T1, T2, T3, T4, T5, T6, TReturn>>(new MethodParam<TComponent>(method, typeof(Func<T1, T2, T3, T4, T5, T6, TReturn>)));
    }

    /// <summary>
    /// Parameter for a delegate
    /// </summary>
    /// <typeparam name="T">Type of parameter being fed to the delegate</typeparam>
    public abstract class Param<T> : Param
    {
        /// <summary>
        /// Type of parameter
        /// </summary>
        internal override Type Type => typeof(T);
    }
}
