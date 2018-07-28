using Moq;
using Shouldly;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xunit;
using Xunit.Sdk;

namespace NAdapter.Test
{
    public class CustomizedMethodShould1Attribute : Attribute { }
    public class CustomizedMethodShould2Attribute : Attribute { }

    public class CustomizedMethodShouldModel
    {
        [CustomizedMethodShould1]
        public string Run(string input) => new string(input.Reverse().ToArray());
    }

    public abstract class MethodTestAttribute : DataAttribute
    {
        protected IEnumerable<object[]> Values()
        {
            return new[]
            {
                new object[] { new MethodAdded() },
                new object[] { new MethodExistingByExpression() },
                new object[] { new MethodExistingBySignature() },
                new object[] { new MethodExistingByISpecification() }
            };
        }
    }

    public class MethodBehaviorTestAttribute : MethodTestAttribute
    {
        public override IEnumerable<object[]> GetData(MethodInfo testMethod) => Values();
    }

    public class MethodExistingBehaviorTestAttribute : MethodTestAttribute
    {
        public override IEnumerable<object[]> GetData(MethodInfo testMethod) => Values().Where(o => o[0] is MethodExisting);
    }

    public abstract class MethodBehaviorManager
    {
        public abstract void Behavior(Action<MethodFunctionBehavior<CustomizedMethodShouldModel, string, string>> behavior);
        public IAdapter<CustomizedMethodShouldModel> GetAdapter(CustomizedMethodShouldModel model = null)
        {
            Adapter = Finish(model);
            return Adapter;
        }
        protected virtual IAdapter<CustomizedMethodShouldModel> Finish(CustomizedMethodShouldModel model) => Spec.Finish().Create(model);
        public IAdapter<CustomizedMethodShouldModel> Adapter { get; private set; }
        public dynamic DynamicAdapter { get { return Adapter; } }
        public Specification<CustomizedMethodShouldModel> Spec { get; private set; } = Specification.New<CustomizedMethodShouldModel>();
        public abstract MethodInfo MethodInfo { get; }
        public abstract string Call(string input);
        public Type AdapterType { get => Adapter.GetType(); }
    }

    public class MethodAdded : MethodBehaviorManager
    {
        public override void Behavior(Action<MethodFunctionBehavior<CustomizedMethodShouldModel, string, string>> action)
        {
            action(Spec
                .SpecifyMethod()
                .WithFunctionSignature<string, string>("NewMethod")
                .SpecifyLinq(m => Spec.Linq.Arg<string>(1) + "added"));
        }
        public override MethodInfo MethodInfo => Adapter.GetType().GetMethod("NewMethod", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        public override string Call(string input) => DynamicAdapter.NewMethod(input);
    }

    public abstract class MethodExisting : MethodBehaviorManager
    {
        public override string Call(string input) => DynamicAdapter.Run(input);
        public override MethodInfo MethodInfo => Adapter.GetType().GetMethod(nameof(CustomizedMethodShouldModel.Run), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
    }

    public class MethodExistingByExpression : MethodExisting
    {
        public override void Behavior(Action<MethodFunctionBehavior<CustomizedMethodShouldModel, string, string>> action)
        {
            action(Spec
                .SpecifyMethod()
                .WithFunctionSignature<string, string>(x => x.Run(null)));
        }
    }

    public class MethodExistingBySignature : MethodExisting
    {
        public override void Behavior(Action<MethodFunctionBehavior<CustomizedMethodShouldModel, string, string>> action)
        {
            action(Spec.SpecifyMethod().WithFunctionSignature<string, string>(nameof(CustomizedMethodShouldModel.Run)));
        }
    }

    public class MethodExistingByISpecification : MethodExisting
    {
        AdapterFactory<CustomizedMethodShouldModel> _result;

        public override void Behavior(Action<MethodFunctionBehavior<CustomizedMethodShouldModel, string, string>> action)
        {
            var mock = new Mock<ISpecification<CustomizedMethodShouldModel>>();
            Spec.Specify(mock.Object);
            mock.Setup(x => x.OnMethod(
                It.IsAny<MethodFunctionBehavior<CustomizedMethodShouldModel, string, string>>()))
                .Callback<MethodFunctionBehavior<CustomizedMethodShouldModel, string, string>>(p =>
            {
                if (p.Name == nameof(CustomizedMethodShouldModel.Run))
                    action(p);
            });
            _result = Spec.Finish();
        }
        protected override IAdapter<CustomizedMethodShouldModel> Finish(CustomizedMethodShouldModel model)
        {
            return _result.Create(model);
        }
    }
    
    public class CustomizedMethodShould
    {
        [Theory, MethodExistingBehaviorTest]
        public void WhenMethodAttributeIsHidden_ThenAdapterMethodDoesNotHaveAttribute(MethodBehaviorManager manager)
        {
            manager.Behavior(b => b
                .Decoration
                .Attributes
                .HideAttributesOfType<CustomizedMethodShould1Attribute>());

            manager.GetAdapter();

            manager.MethodInfo.GetCustomAttributes(true).ShouldBeEmpty();
        }

        [Theory, MethodExistingBehaviorTest]
        public void WhenMethodAttributeIsConverted_ThenAdapterMethodHasConvertedAttribute(MethodBehaviorManager manager)
        {
            manager.Behavior(b => b
                     .Decoration
                     .Attributes
                     .RegisterAttributeConversion<CustomizedMethodShould1Attribute>(a => new CustomizedMethodShould2Attribute()));

            manager.GetAdapter();

            manager.MethodInfo.GetCustomAttributes(true)
                      .Cast<CustomizedMethodShould2Attribute>()
                      .ShouldHaveSingleItem()
                      .ShouldNotBeNull();
        }

        [Theory, MethodExistingBehaviorTest]
        public void WhenMethodAttributeIsConverted_ThenAdapterMethodDoesNotHaveOriginalAttribute(MethodBehaviorManager manager)
        {
            manager.Behavior(b => b
                     .Decoration
                     .Attributes
                     .RegisterAttributeConversion<CustomizedMethodShould1Attribute>(a => new CustomizedMethodShould2Attribute()));

            manager.GetAdapter();

            manager.MethodInfo.GetCustomAttributes(true)
                      .OfType<CustomizedMethodShould1Attribute>()
                      .ShouldBeEmpty();
        }

        [Theory, MethodBehaviorTest]
        public void WhenDecorationIsMadePrivate_AdapterMethodIsPrivate(MethodBehaviorManager manager)
        {
            manager.Behavior(b => b.Decoration.AccessModifier = Access.Private);

            manager.GetAdapter();

            manager.MethodInfo.IsPrivate.ShouldBeTrue();
        }

        [Theory, MethodBehaviorTest]
        public void WhenMethodAttributeIsAdded_ThenAdapterMethodHasAttribute(MethodBehaviorManager manager)
        {
            manager.Behavior(b => b.Decoration
                     .Attributes
                     .AddAttribute(() => new CustomizedMethodShould2Attribute()));

            manager.GetAdapter();

            manager.MethodInfo.GetCustomAttributes(true)
                      .OfType<CustomizedMethodShould2Attribute>()
                      .ShouldHaveSingleItem()
                      .ShouldNotBeNull();
        }

        [Theory, MethodBehaviorTest]
        public void WhenMethodIsHidden_ThenAdapterDoesNotHaveMethod(MethodBehaviorManager manager)
        {
            manager.Behavior(b => b.Decoration.IsHidden = true);

            manager.GetAdapter();

            manager.MethodInfo.ShouldBeNull();
        }

        [Theory, MethodBehaviorTest]
        public void WhenMethodIsVirtual_ThenAdapterMethodIsVirtual(MethodBehaviorManager manager)
        {
            manager.Behavior(b => b.Decoration.IsVirtual = true);

            manager.GetAdapter();

            manager.MethodInfo.IsVirtual.ShouldBeTrue();
        }

        [Theory, MethodBehaviorTest]
        public void WhenMethodNameIsChanged_ThenAdapterHasMethodWithNewName(MethodBehaviorManager manager)
        {
            manager.Behavior(b => b.Decoration.PublicName = "NewName");

            manager.GetAdapter();

            manager.AdapterType.GetMethod("NewName", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                .ShouldNotBeNull();
        }

        [Theory, MethodExistingBehaviorTest]
        public void WhenParamIsSpecifiedByIndex_ThenDelegateReceivesCorrectParam(MethodBehaviorManager manager)
        {
            string backingField = null;
            manager.Behavior(b => b
                .SpecifyDelegate(Param.Arg<string>(1),
                a =>
                {
                    backingField = a;
                    return "blocked";
                }));

            manager.GetAdapter();

            string result = manager.DynamicAdapter.Run("test");
            result.ShouldBe("blocked");
            backingField.ShouldBe("test");
        }

        [Fact]
        public void WhenParamIsSpecifiedByName_ThenDelegateReceivesCorrectParam()
        {
            string backingField = null;
            var spec = Specification.New<CustomizedMethodShouldModel>();
            spec
                .SpecifyMethod()
                .WithFunctionSignature<string, string>(nameof(CustomizedMethodShouldModel.Run), ParamSettings.WithName("x"))
                .SpecifyDelegate(Param.Arg<string>("x"),
                a =>
                {
                    backingField = a;
                    return "blocked";
                });

            dynamic adapter = spec.Finish().Create();

            string result = adapter.Run("test");
            result.ShouldBe("blocked");
            backingField.ShouldBe("test");
        }

        [Theory, MethodExistingBehaviorTest]
        public void WhenParamIsSpecifiedAsDeclared_ThenDelegateReceivesDeclaredValue(MethodBehaviorManager manager)
        {
            string value = "test1";
            manager.Behavior(b => b
                .SpecifyDelegate(Param.Declare(value), x => x));

            manager.GetAdapter();
            value = "test2";    // To ensure that the value is bound early.

            string result = manager.DynamicAdapter.Run("test3");
            result.ShouldBe("test1");
        }

        [Theory, MethodExistingBehaviorTest]
        public void WhenParamIsSpecifiedAsDeclaredNull_ThenDelegateReceivesNull(MethodBehaviorManager manager)
        {
            manager.Behavior(b => b
                .SpecifyDelegate(Param.DeclareNull<string>(), x =>
                {
                    x.ShouldBeNull();
                    return "pass";
                }));

            manager.GetAdapter();

            string result = manager.DynamicAdapter.Run("test");
            result.ShouldBe("pass");
        }

        [Fact]
        public void WhenParamIsPropertyGetter_ThenGetterIsInvokedAtRuntime()
        {
            var spec = Specification.New<CustomizedMethodShouldModel>();
            var prop = spec.SpecifyProperty("Is4")
                .SpecifyDelegates(c => 4, null);

            spec.SpecifyMethod()
                .WithFunctionSignature<int>("CallsIs4")
                .SpecifyDelegate(Param.Getter(prop), g => g());

            dynamic adapter = spec.Finish().Create();

            int result = adapter.CallsIs4();
            result.ShouldBe(4);
        }

        [Fact]
        public void WhenParamIsPropertySetter_ThenSetterIsInvokedAtRuntime()
        {
            int backingField = 0;
            var spec = Specification.New<CustomizedMethodShouldModel>();
            var prop = spec.SpecifyProperty("PassThrough")
                .SpecifyDelegates<int>(null, (c, v) => backingField = v);

            spec.SpecifyMethod()
                .WithFunctionSignature<int, int>("CallsPassThrough")
                .SpecifyDelegate(
                    Param.Arg<int>(1),
                    Param.Setter(prop),
                    (i, s) => s(i));

            dynamic adapter = spec.Finish().Create();
            
            adapter.CallsPassThrough(5);

            backingField.ShouldBe(5);
        }

        [Fact]
        public void WhenParamIsMethod_ThenMethodIsInvokedAtRuntime()
        {
            var spec = Specification.New<CustomizedMethodShouldModel>();
            var m1 = spec.SpecifyMethod()
                .WithFunctionSignature<int>("Return4")
                .SpecifyDelegate(() => 4);

            spec.SpecifyMethod()
                .WithFunctionSignature<int, int>("SumWith4")
                .SpecifyDelegate(
                    Param.Arg<int>(1),
                    Param.Method(m1),
                    (i, m) => i + m());

            dynamic adapter = spec.Finish().Create();

            int result = adapter.SumWith4(5);
            result.ShouldBe(9);
        }

        [Fact]
        public void WhenParamIsComponent_ThenComponentIsReturned()
        {
            CustomizedMethodShouldModel model = new CustomizedMethodShouldModel();
            bool ran = false;
            var spec = Specification.New<CustomizedMethodShouldModel>();
            spec.SpecifyMethod()
                .WithActionSignature("Verify")
                .SpecifyDelegate(
                    Param.Source(spec),
                    c =>
                    {
                        model.ShouldBeSameAs(c);
                        ran = true;
                    });

            dynamic adapter = spec.Finish().Create(model);

            adapter.Verify();

            ran.ShouldBeTrue();
        }

        [Theory, MethodExistingBehaviorTest]
        public void WhenLinqParamIsSpecifiedByIndex_ThenLinqReceivesCorrectParam(MethodBehaviorManager manager)
        {
            manager.Behavior(b => b
                .SpecifyLinq(c => manager.Spec.Linq.Arg<string>(1)));

            manager.GetAdapter();

            string result = manager.DynamicAdapter.Run("test");
            result.ShouldBe("test");
        }

        [Fact]
        public void WhenLinqParamIsSpecifiedByName_ThenLinqReceivesCorrectParam()
        {
            var spec = Specification.New<CustomizedMethodShouldModel>();

            spec.SpecifyMethod()
                .WithFunctionSignature<string, string>(nameof(CustomizedMethodShouldModel.Run), ParamSettings.WithName("x"))
                .SpecifyLinq(a => spec.Linq.Arg<string>("x"));

            dynamic adapter = spec.Finish().Create();

            string result = adapter.Run("test");
            result.ShouldBe("test");
        }

        [Fact]
        public void WhenRecursiveMethodIsSpecified_ThenOperationSucceeds()
        {
            var spec = Specification.New<CustomizedMethodShouldModel>();

            var fibonacci = spec
                .SpecifyMethod()
                .WithFunctionSignature<int, int>("Fibonacci");

            fibonacci.SpecifyLinq(x =>
                spec.Linq.Arg<int>(1) == 0 ? 1 :
                    spec.Linq.Arg<int>(1) == 1 ? 1 :
                        (spec.Linq.Function<int, int>(fibonacci, spec.Linq.Arg<int>(1) - 1) +
                            spec.Linq.Function<int, int>(fibonacci, spec.Linq.Arg<int>(1) - 2)));

            dynamic adapter = spec.Finish().Create();

            int result = adapter.Fibonacci(6);
            result.ShouldBe(13);
        }

        [Fact]
        public void WhenRecursiveMethodWithBlockLambdaIsSpecified_ThenOperationSucceeds()
        {
            var spec = Specification.New<CustomizedMethodShouldModel>();

            var fibonacci = spec
                .SpecifyMethod()
                .WithFunctionSignature<int, int>("Fibonacci");

            // \todo: invent a syntax for building code that doesn't suck quite this much to work with.
            var arg = Expression.Parameter(typeof(Int32), "arg");
            var linqType = typeof(LinqParam<CustomizedMethodShouldModel>);
            var linq = Expression.New(linqType);
            var endLabel = Expression.Label();
            var argMethod = linqType.GetMethod("Arg", new Type[] { typeof(Int32) }).MakeGenericMethod(typeof(Int32));
            var funcMethod = linqType
                .GetMethods()
                .Where(m => m.Name == "Function")
                .FirstOrDefault(m => m.GetGenericArguments().Length == 2)
                .MakeGenericMethod(typeof(Int32), typeof(Int32));
            var result = Expression.Parameter(typeof(Int32), "result");
            var fibMinusOne = Expression.Parameter(typeof(Int32), "fib1");
            var fibMinusTwo = Expression.Parameter(typeof(Int32), "fib2");

            LambdaExpression lambda = Expression.Lambda(
                Expression.Block(
                    new ParameterExpression[] { arg, fibMinusOne, fibMinusTwo, result },  // int arg, fibMinusOne, fibMinusTwo, result;
                    
                    // arg = Spec.Linq.Arg<int>(1);
                    Expression.Assign(arg,
                        Expression.Call(linq, argMethod, Expression.Constant(1))),
                    
                    // if (arg == 0) result = 1;
                    Expression.IfThenElse(
                        Expression.Equal(arg, Expression.Constant(0)),
                        Expression.Assign(result, Expression.Constant(1)),

                        // else if (arg == 1) result = 1;
                        Expression.IfThenElse(
                            Expression.Equal(arg, Expression.Constant(1)),
                            Expression.Assign(result, Expression.Constant(1)),
                            Expression.Block(

                                // else {
                                //      fibMinusOne = fibonacci(arg - 1);
                                Expression.Assign(fibMinusOne, Expression.Call(linq, funcMethod,
                                    Expression.Constant(fibonacci),
                                       Expression.Subtract(arg, Expression.Constant(1)))),

                                //      fibMinusTwo = fibonacci(arg - 2);
                                Expression.Assign(fibMinusTwo, Expression.Call(linq, funcMethod,
                                    Expression.Constant(fibonacci),
                                       Expression.Subtract(arg, Expression.Constant(2)))),

                                //      result = fibMinusOne + fibMinusTwo;
                                // }
                                Expression.Assign(result, Expression.Add(fibMinusOne, fibMinusTwo))))),

                    // return result;
                    result),
                    Expression.Parameter(typeof(CustomizedMethodShouldModel)));

            fibonacci.SpecifyLinq(lambda);

            dynamic adapter = spec.Finish().Create();

            int fibonacciResult = adapter.Fibonacci(7);
            fibonacciResult.ShouldBe(21);
        }

        [Theory, MethodExistingBehaviorTest]
        public void WhenLinqParamIsSpecifiedAsConstableLocal_ThenLinqReceivesDeclaredValue(MethodBehaviorManager manager)
        {
            string value = "test1";
            manager.Behavior(b => b
                .SpecifyLinq(x => value));
            
            dynamic adapter = manager.GetAdapter();
            value = "test2";    // To ensure that the value is bound early.

            string result = adapter.Run("test3");
            result.ShouldBe("test1");
        }

        [Fact]
        public void WhenLinqParamIsPropertyGetter_ThenGetterIsInvokedAtRuntime()
        {
            var spec = Specification.New<CustomizedMethodShouldModel>();

            var prop = spec.SpecifyProperty("Is4")
                .SpecifyDelegates(c => 4, null);

            spec.SpecifyMethod()
                .WithFunctionSignature<int>("CallsIs4")
                .SpecifyLinq(c => spec.Linq.Getter<int>(prop));

            dynamic adapter = spec.Finish().Create();

            int result = adapter.CallsIs4();
            result.ShouldBe(4);
        }

        [Fact]
        public void WhenLinqParamIsPropertySetter_ThenSetterIsInvokedAtRuntime()
        {
            int backingField = 0;
            var spec = Specification.New<CustomizedMethodShouldModel>();

            var prop = spec.SpecifyProperty("PassThrough")
                .SpecifyDelegates<int>(null, (c, v) => backingField = v);

            spec.SpecifyMethod()
                .WithFunctionSignature<int, int>("CallsPassThrough")
                .SpecifyLinq(c => spec.Linq.Setter(prop, spec.Linq.Arg<int>(1)));

            dynamic adapter = spec.Finish().Create();

            adapter.CallsPassThrough(5);

            backingField.ShouldBe(5);
        }

        [Fact]
        public void WhenLinqParamIsMethod_ThenMethodIsInvokedAtRuntime()
        {
            var spec = Specification.New<CustomizedMethodShouldModel>();

            var m1 = spec.SpecifyMethod()
                .WithFunctionSignature<int>("Return4")
                .SpecifyDelegate(() => 4);

            spec.SpecifyMethod()
                .WithFunctionSignature<int, int>("SumWith4")
                .SpecifyLinq(c => spec.Linq.Arg<int>(1) + spec.Linq.Function<int>(m1));

            dynamic adapter = spec.Finish().Create();

            int result = adapter.SumWith4(5);
            result.ShouldBe(9);
        }

        [Fact]
        public void WhenLinqMethodUsesMagic0_ThenMethodStillRuns()
        {
            var spec = Specification.New<CustomizedMethodShouldModel>();

            spec.SpecifyMethod()
                .WithFunctionSignature<int, int>("ReturnArg")
                .SpecifyLinq(_0 => spec.Linq.Arg<int>(1));

            dynamic adapter = spec.Finish().Create();

            int result = adapter.ReturnArg(9);
            result.ShouldBe(9);
        }

        [Fact]
        public void WhenLinqMethodUsesMagic1_ThenMethodStillRuns()
        {
            var spec = Specification.New<CustomizedMethodShouldModel>();

            spec.SpecifyMethod()
                .WithFunctionSignature<int, int>("ReturnArg")
                .SpecifyLinq(_1 => spec.Linq.Arg<int>(1));

            dynamic adapter = spec.Finish().Create();

            int result = adapter.ReturnArg(12);
            result.ShouldBe(12);
        }

        [Fact]
        public void WhenLinqReturnsValueButFunctionDoesNot_ThenMethodRuns()
        {
            var spec = Specification.New<CustomizedMethodShouldModel>();

            spec.SpecifyMethod()
                .WithActionSignature("Call")
                .SpecifyLinq(x => 1);

            dynamic adapter = spec.Finish().Create();

            Should.NotThrow(() => adapter.Call());
        }

        [Fact]
        public void WhenLinqExpressionHasWrongParameters_ThenExceptionIsThrownImmediately()
        {
            Should.Throw<ArgumentException>(() =>
                Specification.New<CustomizedMethodShouldModel>().SpecifyMethod()
                    .WithFunctionSignature<int, string>(e => e.Run(null)));
        }
    }
}
