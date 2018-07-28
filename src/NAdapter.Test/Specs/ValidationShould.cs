using Shouldly;
using Xunit;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace NAdapter.Test
{
    public class ValidationWithIndexShouldModel
    {
        [IndexerName("TestFoo")]
        public string this[string accessor]
        {
            get { return null; }
            set { }
        }
    }

    public class ValidationWithStaticPropertyModel
    {
        public static string TestFoo { get { return null; } }
    }

    public class ValidationWithGenericMethodModel
    {
        public void TestFoo<T>() { }
    }

    public class ValidationWithRefParamModel
    {
        public void TestFoo(ref int i) { }
    }

    public class ValidationWithOutParamModel
    {
        public void TestFoo(out int i) { i = 0; }
    }

    public class ValidationWithStaticMethodModel
    {
        public static void TestFoo() { }
    }

    public delegate void DelegateTester();

    public class ValidationWithEventModel
    {
#pragma warning disable 0067
        public event DelegateTester TestFoo;
#pragma warning restore 0067
    }
    
    public class ValidationWithLongMethodModel
    {
        public void Do(int a, double b, long c, string d, bool e, object f, float g) { }
    }

    public class ValidationShouldModel { }
    
    public class ValidationShould: TestBase<ValidationShouldModel>
    {
        [Fact]
        public void GivenComponentWithIndexProperty_WhenSpecIsValidated_ThenWarningIsReturned()
        {
            var spec = Specification.New<ValidationWithIndexShouldModel>();
            var warning = spec.Validate().Warnings.ShouldHaveSingleItem();

            warning.ShouldSatisfyAllConditions(
                () => warning.ShouldContain("TestFoo"),
                () => warning.ShouldContain("index"),
                () => warning.ShouldContain("0.1.1")
                );
        }
        
        [Fact]
        public void GivenComponentWithStaticProperty_WhenSpecIsValidated_ThenWarningIsReturned()
        {
            var spec = Specification.New<ValidationWithStaticPropertyModel>();
            var warning = spec.Validate().Warnings.ShouldHaveSingleItem();

            warning.ShouldSatisfyAllConditions(
                () => warning.ShouldContain("TestFoo"),
                () => warning.ShouldContain("static"),
                () => warning.ShouldContain("0.1.3")
                );
        }

        [Fact]
        public void GivenComponentWithPropertyWithNoGetterOrSetter_WhenSpecIsValidated_ThenErrorIsReturned()
        {
            // The only way to get a property with neither a getter or a setter is to base it on
            // a component property that is the same way. But c# doesn't allow it, so I have to build
            // a malformed type with raw MSIL in order to test this.

            var assemblyName = new AssemblyName("NAdapter.Test.Dynamic");
            var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
            var module = assembly.DefineDynamicModule(assemblyName.Name, assemblyName.Name + "Module.dll");

            var tb = module.DefineType("InvalidPropGetterSetter", TypeAttributes.Public, typeof(Object));

            var ctorBuilder = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
            var ctorIL = ctorBuilder.GetILGenerator();
            ctorIL.Emit(OpCodes.Ldarg_0);
            ctorIL.Emit(OpCodes.Callvirt, typeof(Object).GetConstructor(Type.EmptyTypes));
            ctorIL.Emit(OpCodes.Ret);

            tb.DefineProperty("Test", PropertyAttributes.None, typeof(String), Type.EmptyTypes);
            // Property has no getter or setter defined

            var malformedType = tb.CreateType();

            var specification = typeof(Specification).GetMethod(nameof(Specification.New)).MakeGenericMethod(malformedType).Invoke(null, new object[0]);
            var specType = typeof(Specification<>).MakeGenericType(malformedType);
            var validator = specType.GetMethod(nameof(Specification<string>.Validate));
            var result = validator.Invoke(specification, new object[0]) as TypeValidationResult;

            result.Errors.ShouldNotBeEmpty();
        }

        [Fact]
        public void GivenComponentWithGenericMethod_WhenSpecIsValidated_ThenWarningIsReturned()
        {
            var spec = Specification.New<ValidationWithGenericMethodModel>();
            var warning = spec.Validate().Warnings.ShouldHaveSingleItem();

            warning.ShouldSatisfyAllConditions(
                () => warning.ShouldContain("TestFoo"),
                () => warning.ShouldContain("generic"),
                () => warning.ShouldContain("0.1.1")
                );
        }

        [Fact]
        public void GivenComponentWithMethodContainingRefParam_WhenSpecIsValidated_ThenWarningIsReturned()
        {
            var spec = Specification.New<ValidationWithRefParamModel>();
            var warning = spec.Validate().Warnings.ShouldHaveSingleItem();

            warning.ShouldSatisfyAllConditions(
                () => warning.ShouldContain("TestFoo"),
                () => warning.ShouldContain("ref"),
                () => warning.ShouldContain("0.1.1")
                );
        }

        [Fact]
        public void GivenComponentWithMethodContainingOutParam_WhenSpecIsValidated_ThenWarningIsReturned()
        {
            var spec = Specification.New<ValidationWithOutParamModel>();
            var warning = spec.Validate().Warnings.ShouldHaveSingleItem();

            warning.ShouldSatisfyAllConditions(
                () => warning.ShouldContain("TestFoo"),
                () => warning.ShouldContain("out"),
                () => warning.ShouldContain("0.1.1")
                );
        }

        [Fact]
        public void GivenComponentWithStaticMethod_WhenSpecIsValidated_ThenWarningIsReturned()
        {
            var spec = Specification.New<ValidationWithStaticMethodModel>();
            var warning = spec.Validate().Warnings.ShouldHaveSingleItem();

            warning.ShouldSatisfyAllConditions(
                () => warning.ShouldContain("TestFoo"),
                () => warning.ShouldContain("static"),
                () => warning.ShouldContain("0.1.3")
                );
        }

        [Fact]
        public void GivenComponentWithEvent_WhenSpecIsValidated_ThenWarningIsReturned()
        {
            var spec = Specification.New<ValidationWithEventModel>();
            var warning = spec.Validate().Warnings.ShouldHaveSingleItem();

            warning.ShouldSatisfyAllConditions(
                () => warning.ShouldContain("TestFoo"),
                () => warning.ShouldContain("event", Case.Insensitive),
                () => warning.ShouldContain("0.1.3")
                );
        }

        [Fact]
        public void GivenDefaultParameterNotAtEnd_WhenSpecIsValidated_ThenErrorIsReturned()
        {
            Spec.SpecifyMethod()
                .WithFunctionSignature<int, int, int>("Sum",
                    param1: ParamSettings.WithName("x").WithDefault(3))
                .SpecifyLinq(x => Spec.Linq.Arg<int>(1) + Spec.Linq.Arg<int>(2));

            var validation = Spec.Validate().Errors.ShouldHaveSingleItem();

            validation.ShouldContain("default");
        }

        [Fact]
        public void GivenIdentifierWithInvalidCharacter_ThenIdentifierIsInvalid()
        {
            Spec.SpecifyMethod()
                .WithFunctionSignature<int>("$String")
                .SpecifyLinq(x => 0);
            
            var errors = Spec.Validate().Errors.ShouldHaveSingleItem();

            errors.ShouldContain("$String");
        }

        [Fact]
        public void GivenKeywordIdentifier_ThenIdentifierIsInvalid()
        {
            Spec.SpecifyMethod()
                .WithFunctionSignature<int>("this")
                .SpecifyLinq(x => 0);

            var validation = Spec.Validate().Errors.ShouldHaveSingleItem();

            validation.ShouldContain("this");
        }

        [Fact]
        public void GivenChangedProperty_ThenCrossReferenceIsInvalid()
        {
            var four = Spec.SpecifyProperty("Four")
                .SpecifyDelegates(x => 4, null);

            Spec.SpecifyMethod()
                .WithFunctionSignature<int>("GetFour")
                .SpecifyLinq(x => Spec.Linq.Getter<int>(four));

            four.SpecifyDelegates(x => "4", null);

            Spec.Validate().Errors.ShouldHaveSingleItem().ShouldNotBeNull();
        }
        
        [Fact]
        public void GivenChangedMethod_ThenCrossReferenceIsInvalid()
        {
            var four = Spec.SpecifyMethod()
                .WithFunctionSignature<int>("Four")
                .SpecifyLinq(x => 4);

            Spec.SpecifyMethod()
                .WithFunctionSignature<int, int>("FourPlus")
                .SpecifyLinq(x => Spec.Linq.Arg<int>(1) + Spec.Linq.Function<int>(four));

            four.SpecifyLinq(x => "4");
            
            Spec.Validate().Errors.ShouldHaveSingleItem().ShouldNotBeNull();
        }

        [Fact]
        public void GivenDuplicateNames_WhenTypeIsValidated_ThenErrorIsReturned()
        {
            Spec.SpecifyMethod()
                .WithActionSignature("Test")
                .SpecifyLinq(x => 4);

            Spec.SpecifyProperty("Test")
                .SpecifyAutoImplemented<int>();
            
            Spec.Validate().Errors.ShouldHaveSingleItem().ShouldContain("Test");
        }

        [Fact]
        public void GivenMethodOverloads_WhenTypeIsValidated_ThenOverloadsHaveCorrectParameterTypesInCorrectOrder()
        {
            Spec.SpecifyMethod()
                .WithActionSignature<string, int>("Tester")
                .SpecifyDelegate(() => { });

            Spec.SpecifyMethod()
                .WithFunctionSignature<string, double, int>("Tester")
                .SpecifyDelegate(() => 4);

            var overloads = Spec.Validate().MethodGroups.ShouldHaveSingleItem().MethodOverloads.ToArray();

            overloads.ShouldSatisfyAllConditions(
                () => overloads[0].ShouldSatisfyAllConditions(
                    () => overloads[0].ReturnType.ShouldBe(typeof(void)),
                    () => overloads[0].Parameters.Select(p => p.ParameterType).SequenceEqual(
                        new[] { typeof(String), typeof(Int32) }).ShouldBeTrue()
                    ),
                () => overloads[1].ShouldSatisfyAllConditions(
                    () => overloads[1].ReturnType.ShouldBe(typeof(Int32)),
                    () => overloads[1].Parameters.Select(p => p.ParameterType).SequenceEqual(
                        new[] { typeof(String), typeof(Double) }).ShouldBeTrue()
                    )
                );
        }

        [Fact]
        public void GivenMethodWithTooManyParameters_WhenTypeIsValidated_ThenWarningIsAdded()
        {
            var spec = Specification.New<ValidationWithLongMethodModel>();
            spec.Validate()
                .Warnings
                .ShouldHaveSingleItem()
                .ShouldContain("too many", Case.Insensitive);
        }
    }
}
