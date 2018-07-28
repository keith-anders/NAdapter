using System;
using System.Reflection.Emit;

namespace NAdapter
{
    /// <summary>
    /// Parameter that declares its value as a constant
    /// </summary>
    /// <typeparam name="T">Type of parameter</typeparam>
    internal abstract class DeclareParam<T> : Param<T>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="value">The constant value</param>
        internal DeclareParam(T value) => Value = value;
        
        public override string ToString() => $"Param: Declared {Value}";
        
        /// <summary>
        /// No-op
        /// </summary>
        /// <param name="validation">Validation</param>
        internal override void Validate(SubparameterValidationResult validation) { }

        /// <summary>
        /// Constant Value of the parameter
        /// </summary>
        protected T Value { get; private set; }
    }

    /// <summary>
    /// Parameter that declares a <see cref="System.Int32"/> for its constant value
    /// </summary>
    internal class DeclareIntParam : DeclareParam<int>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="value">Value</param>
        internal DeclareIntParam(int value) : base(value) { }

        /// <summary>
        /// Emits IL to load this constant onto the evaluation stack
        /// </summary>
        /// <param name="il">ILGenerator</param>
        /// <param name="sourceField">Field containing the source component</param>
        /// <param name="typeBeingBuilt">Type being built</param>
        internal override void Emit(ILGenerator il, FieldBuilder sourceField, Type typeBeingBuilt)
        {
            il.Emit(OpCodes.Ldc_I4, Value);
        }
    }

    /// <summary>
    /// Parameter that declares a <see cref="System.String"/> for its constant value
    /// </summary>
    internal class DeclareStringParam : DeclareParam<string>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="value">Value</param>
        internal DeclareStringParam(string value) : base(value) { }

        /// <summary>
        /// Emits IL to load this constant onto the evaluation stack
        /// </summary>
        /// <param name="il">ILGenerator</param>
        /// <param name="sourceField">Field containing the source component</param>
        /// <param name="typeBeingBuilt">Type being built</param>
        internal override void Emit(ILGenerator il, FieldBuilder sourceField, Type typeBeingBuilt)
        {
            if (Value != null)
                il.Emit(OpCodes.Ldstr, Value);
            else
                il.Emit(OpCodes.Ldnull);
        }
    }

    /// <summary>
    /// Parameter that declares a <see cref="System.Double"/> for its constant value
    /// </summary>
    internal class DeclareDoubleParam : DeclareParam<double>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="value">Value</param>
        internal DeclareDoubleParam(double value) : base(value) { }
        
        /// <summary>
        /// Emits IL to load this constant onto the evaluation stack
        /// </summary>
        /// <param name="il">ILGenerator</param>
        /// <param name="sourceField">Field containing the source component</param>
        /// <param name="typeBeingBuilt">Type being built</param>
        internal override void Emit(ILGenerator il, FieldBuilder sourceField, Type typeBeingBuilt)
        {
            il.Emit(OpCodes.Ldc_R8, Value);
        }
    }

    /// <summary>
    /// Parameter that declares a <see cref="System.Single"/> for its constant value
    /// </summary>
    internal class DeclareSingleParam : DeclareParam<float>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="value">Value</param>
        internal DeclareSingleParam(float value) : base(value) { }

        /// <summary>
        /// Emits IL to load this constant onto the evaluation stack
        /// </summary>
        /// <param name="il">ILGenerator</param>
        /// <param name="sourceField">Field containing the source component</param>
        /// <param name="typeBeingBuilt">Type being built</param>
        internal override void Emit(ILGenerator il, FieldBuilder sourceField, Type typeBeingBuilt)
        {
            il.Emit(OpCodes.Ldc_R4, Value);
        }
    }

    /// <summary>
    /// Parameter that declares a <see cref="System.Boolean"/> for its constant value
    /// </summary>
    internal class DeclareBoolParam : DeclareParam<bool>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="value">Value</param>
        internal DeclareBoolParam(bool value) : base(value) { }

        /// <summary>
        /// Emits IL to load this constant onto the evaluation stack
        /// </summary>
        /// <param name="il">ILGenerator</param>
        /// <param name="sourceField">Field containing the source component</param>
        /// <param name="typeBeingBuilt">Type being built</param>
        internal override void Emit(ILGenerator il, FieldBuilder sourceField, Type typeBeingBuilt)
        {
            il.Emit(Value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
        }
    }

    /// <summary>
    /// Parameter that declares a <see cref="System.Char"/> for its constant value
    /// </summary>
    internal class DeclareCharParam : DeclareParam<char>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="value">Value</param>
        internal DeclareCharParam(char value) : base(value) { }

        /// <summary>
        /// Emits IL to load this constant onto the evaluation stack
        /// </summary>
        /// <param name="il">ILGenerator</param>
        /// <param name="sourceField">Field containing the source component</param>
        /// <param name="typeBeingBuilt">Type being built</param>
        internal override void Emit(ILGenerator il, FieldBuilder sourceField, Type typeBeingBuilt)
        {
            il.Emit(OpCodes.Ldc_I4, Value);
        }
    }

    /// <summary>
    /// Parameter that declares a <see cref="System.Byte"/> for its constant value
    /// </summary>
    internal class DeclareByteParam : DeclareParam<byte>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="value">Value</param>
        internal DeclareByteParam(byte value) : base(value) { }

        /// <summary>
        /// Emits IL to load this constant onto the evaluation stack
        /// </summary>
        /// <param name="il">ILGenerator</param>
        /// <param name="sourceField">Field containing the source component</param>
        /// <param name="typeBeingBuilt">Type being built</param>
        internal override void Emit(ILGenerator il, FieldBuilder sourceField, Type typeBeingBuilt)
        {
            il.Emit(OpCodes.Ldc_I4, Value);
        }
    }

    /// <summary>
    /// Parameter that declares a <see cref="System.SByte"/> for its constant value
    /// </summary>
    internal class DeclareSByteParam : DeclareParam<sbyte>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="value">Value</param>
        internal DeclareSByteParam(sbyte value) : base(value) { }

        /// <summary>
        /// Emits IL to load this constant onto the evaluation stack
        /// </summary>
        /// <param name="il">ILGenerator</param>
        /// <param name="sourceField">Field containing the source component</param>
        /// <param name="typeBeingBuilt">Type being built</param>
        internal override void Emit(ILGenerator il, FieldBuilder sourceField, Type typeBeingBuilt)
        {
            il.Emit(OpCodes.Ldc_I4, Value);
        }
    }

    /// <summary>
    /// Parameter that declares a <see cref="System.Int16"/> for its constant value
    /// </summary>
    internal class DeclareShortParam : DeclareParam<short>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="value">Value</param>
        internal DeclareShortParam(short value) : base(value) { }

        /// <summary>
        /// Emits IL to load this constant onto the evaluation stack
        /// </summary>
        /// <param name="il">ILGenerator</param>
        /// <param name="sourceField">Field containing the source component</param>
        /// <param name="typeBeingBuilt">Type being built</param>
        internal override void Emit(ILGenerator il, FieldBuilder sourceField, Type typeBeingBuilt)
        {
            il.Emit(OpCodes.Ldc_I4, Value);
        }
    }

    /// <summary>
    /// Parameter that declares a <see cref="System.UInt16"/> for its constant value
    /// </summary>
    internal class DeclareUShortParam : DeclareParam<ushort>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="value">Value</param>
        internal DeclareUShortParam(ushort value) : base(value) { }

        /// <summary>
        /// Emits IL to load this constant onto the evaluation stack
        /// </summary>
        /// <param name="il">ILGenerator</param>
        /// <param name="sourceField">Field containing the source component</param>
        /// <param name="typeBeingBuilt">Type being built</param>
        internal override void Emit(ILGenerator il, FieldBuilder sourceField, Type typeBeingBuilt)
        {
            il.Emit(OpCodes.Ldc_I4, Value);
        }
    }

    /// <summary>
    /// Parameter that declares a <see cref="System.UInt32"/> for its constant value
    /// </summary>
    internal class DeclareUIntParam : DeclareParam<uint>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="value">Value</param>
        internal DeclareUIntParam(uint value) : base(value) { }

        /// <summary>
        /// Emits IL to load this constant onto the evaluation stack
        /// </summary>
        /// <param name="il">ILGenerator</param>
        /// <param name="sourceField">Field containing the source component</param>
        /// <param name="typeBeingBuilt">Type being built</param>
        internal override void Emit(ILGenerator il, FieldBuilder sourceField, Type typeBeingBuilt)
        {
            il.Emit(OpCodes.Ldc_I4, Value);
        }
    }

    /// <summary>
    /// Parameter that declares a <see cref="System.Int64"/> for its constant value
    /// </summary>
    internal class DeclareLongParam : DeclareParam<long>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="value">Value</param>
        internal DeclareLongParam(long value) : base(value) { }

        /// <summary>
        /// Emits IL to load this constant onto the evaluation stack
        /// </summary>
        /// <param name="il">ILGenerator</param>
        /// <param name="sourceField">Field containing the source component</param>
        /// <param name="typeBeingBuilt">Type being built</param>
        internal override void Emit(ILGenerator il, FieldBuilder sourceField, Type typeBeingBuilt)
        {
            il.Emit(OpCodes.Ldc_I8, Value);
        }
    }

    /// <summary>
    /// Parameter that declares a <see cref="System.UInt64"/> for its constant value
    /// </summary>
    internal class DeclareULongParam : DeclareParam<ulong>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="value">Value</param>
        internal DeclareULongParam(ulong value) : base(value) { }

        /// <summary>
        /// Emits IL to load this constant onto the evaluation stack
        /// </summary>
        /// <param name="il">ILGenerator</param>
        /// <param name="sourceField">Field containing the source component</param>
        /// <param name="typeBeingBuilt">Type being built</param>
        internal override void Emit(ILGenerator il, FieldBuilder sourceField, Type typeBeingBuilt)
        {
            il.Emit(OpCodes.Ldc_I8, Value);
        }
    }

    /// <summary>
    /// Parameter that declares null for its constant value
    /// </summary>
    internal class DeclareNullParam<T> : DeclareParam<T> where T : class
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        internal DeclareNullParam() : base(null) { }

        /// <summary>
        /// Emits IL to load this constant onto the evaluation stack
        /// </summary>
        /// <param name="il">ILGenerator</param>
        /// <param name="sourceField">Field containing the source component</param>
        /// <param name="typeBeingBuilt">Type being built</param>
        internal override void Emit(ILGenerator il, FieldBuilder sourceField, Type typeBeingBuilt)
        {
            il.Emit(OpCodes.Ldnull);
        }
    }

}
