using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xunit;

namespace NAdapter.Test
{
    public class QuickStartModel
    {
        public string String { get; set; }

        public List<string> ListString { get; set; }

        public int Multiply(int x, int y) => x * y;
    }
    
    public class QuickStart
    {
        public QuickStart() => Spec = Specification.New<QuickStartModel>();

        Specification<QuickStartModel> Spec { get; set; }

        dynamic GetDynamic(QuickStartModel component = null) => Spec.Finish().Create(component);

        IAdapter<QuickStartModel> GetAdapter(QuickStartModel component = default(QuickStartModel)) => Spec.Finish().Create(component);
        
        [Fact]
        public void ChangePropertyName()
        {
            Spec.SpecifyProperty(x => x.String)
                .Decoration
                .PublicName = "ChangedString";

            var model = new QuickStartModel();
            var dynamicModel = GetDynamic(model);

            model.String = "test";
            string result = dynamicModel.ChangedString;

            result.ShouldBe("test");
        }

        [Fact]
        public void AddAutoImplementedProperty()
        {
            Spec.SpecifyProperty("NewProperty")
                .SpecifyAutoImplemented<string>();

            dynamic d = GetDynamic();
            d.NewProperty = "test";
            string result = d.NewProperty;

            result.ShouldBe("test");
        }

        [Fact]
        public void AddPropertyBackedByComponentProperty()
        {
            Spec.SpecifyProperty("NewProperty")
                .SpecifyBackingComponentProperty(m => m.String);

            var model = new QuickStartModel();
            dynamic d = GetDynamic(model);

            model.String = "test";
            string result = d.NewProperty;
            result.ShouldBe("test");
            d.NewProperty = "test2";
            model.String.ShouldBe("test2");
        }

        [Fact]
        public void AddPropertyBackedByDelegates()
        {
            string backingField = null;
            Spec.SpecifyProperty("NewProperty")
                .SpecifyDelegates(
                    m => backingField,
                    (m, f) => backingField = f);

            dynamic d = GetDynamic();
            d.NewProperty = "test";
            backingField.ShouldBe("test");
            backingField = "test2";
            string result = d.NewProperty;
            result.ShouldBe("test2");
        }

        [Fact]
        public void AddPropertyFilter()
        {
            Spec.SpecifyProperty(x => x.String)
                .AddGetterFilter(s => s + "tested");

            var model = new QuickStartModel();
            dynamic d = GetDynamic(model);
            model.String = "This is ";
            string result = d.String;
            result.ShouldBe("This is tested");
        }

        [Fact]
        public void FilterApplication()
        {
            // This makes the property getter fill out the list with a non-null
            // value and return the new value if appropriate. Such a thing would
            // perhaps be coded like this:
            //      get
            //      {
            //          var list = Source.P3;
            //          if (list == null)
            //          {
            //              var newList = new List<string>();
            //              Source.P3 = newList;
            //              return newList;
            //          }
            //          return list;
            //      }
            var list = Expression.Parameter(typeof(List<string>), "list");
            var comp = Expression.Parameter(typeof(QuickStartModel), "comp");
            var setter = Expression.Parameter(typeof(Action<List<string>>), "setter");
            var newList = Expression.Parameter(typeof(List<string>), "newList");
            LambdaExpression lambda = Expression.Lambda(
                Expression.Condition(
                    Expression.Equal(list, Expression.Constant(null)),
                    Expression.Block(new[] { newList },
                        Expression.Assign(newList, Expression.New(typeof(List<string>))),
                        Expression.Invoke(setter, newList),
                        newList), list)
                , list, comp, setter);
            Spec.SpecifyProperty(m => m.ListString)
                .AddGetterFilter(lambda);

            var model = new QuickStartModel();
            dynamic adapter = GetAdapter(model);

            model.ListString.ShouldBeNull();

            List<string> result = adapter.ListString;
            result.ShouldNotBeNull();
            result.ShouldBeSameAs(model.ListString);
        }

        [Fact]
        public void FilterSetterApplication()
        {
            // This makes the property setter call AddRange to add the new values to
            // the list rather than replacing the list reference. It would perhaps
            // be coded like this:
            // set
            // {
            //      var existingList = Source.P3;
            //      if (existingList == null)
            //          Source.P3 = value;
            //      else
            //          existingList.AddRange(value == null ? new List<string>() : value);
            // }
            var list = Expression.Parameter(typeof(List<string>), "value");
            var comp = Expression.Parameter(typeof(QuickStartModel), "comp");
            var getter = Expression.Parameter(typeof(Func<List<string>>), "getter");
            var newList = Expression.Parameter(typeof(List<string>), "newList");
            var existingList = Expression.Parameter(typeof(List<string>), "existingList");
            LambdaExpression lambda = Expression.Lambda(
                Expression.Block(new[] { newList, existingList },
                    Expression.Assign(existingList, Expression.Invoke(getter)),
                    Expression.Condition(
                       Expression.Equal(existingList, Expression.Constant(null)),
                       list,
                       Expression.Block(
                           Expression.Call(
                               existingList,
                               typeof(List<string>).GetMethod("AddRange"),
                               Expression.Condition(
                                   Expression.Equal(list, Expression.Constant(null)),
                                   Expression.New(typeof(List<string>)),
                                   list)),
                           existingList))
                ), list, comp, getter);
            Spec.SpecifyProperty(m => m.ListString)
                .AddSetterFilter(lambda);

            var model = new QuickStartModel();
            dynamic adapter = GetAdapter(model);

            model.ListString.ShouldBeNull();
            adapter.ListString = new List<string>() { "test1" };
            model.ListString.ShouldHaveSingleItem().ShouldBe("test1");

            adapter.ListString = new List<string>() { "test2" };
            model.ListString[0].ShouldBe("test1");
            model.ListString[1].ShouldBe("test2");
        }

        [Fact]
        public void AddMethodWithSpecifyDelegate()
        {
            var sumMethod = Spec.SpecifyMethod()
                .WithFunctionSignature<int, int, int>("Sum")
                .SpecifyDelegate(
                    Param.Arg<int>(1),
                    Param.Arg<int>(2),
                    (x, y) => x + y);

            dynamic adapter = GetAdapter();
            
            int sum = adapter.Sum(3, 7);
            sum.ShouldBe(10);
        }

        [Fact]
        public void AddMethodWithSpecifyDelegateUsingSourceParam()
        {
            int backingField = 0;
            var storeIntPlus = Spec.SpecifyMethod()
                            .WithActionSignature<int>("StoreIntPlus", ParamSettings.WithName("value"))
                            .SpecifyDelegate(
                                Param.Source(Spec),
                                Param.Arg<int>("value"),
                                (c, i) => backingField = int.Parse(c.String) + i);
            
            dynamic adapter = GetAdapter(new QuickStartModel() { String = "5" });
            adapter.StoreIntPlus(3);
            backingField.ShouldBe(8);
        }

        [Fact]
        public void AddMethodWithSpecifyDelegateUsingArgByName()
        {
            Spec.SpecifyMethod()
                    .WithFunctionSignature<int, int, int>("Sum",
                        ParamSettings.WithName("x"),
                        ParamSettings.WithName("y"))
                    .SpecifyDelegate(
                        Param.Arg<int>("x"),
                        Param.Arg<int>("y"),
                        (x, y) => x + y);

            dynamic adapter = GetAdapter();

            int sum = adapter.Sum(5, -2);
            sum.ShouldBe(3);
        }

        [Fact]
        public void AddMethodWithSpecifyDelegateThatCallsAnotherMethod()
        {
            var sum = Spec.SpecifyMethod()
                .WithFunctionSignature<int, int, int>("Sum")
                .SpecifyDelegate(
                    Param.Arg<int>(1), Param.Arg<int>(2),
                    (x, y) => x + y);

            int result = 0;
            Spec.SpecifyMethod()
                    .WithActionSignature<object, int, string>(
                        "SetInteger",
                        ParamSettings.WithName("o"),
                        ParamSettings.WithName("x"),
                        ParamSettings.WithName("y").WithDefault("6"))
                    .SpecifyDelegate(
                        Param.Source(Spec),
                        Param.Arg<int>(2),
                        Param.Arg<string>("y"),
                        Param.Method(sum),
                        (component, x, y, sumDelegate) =>
                        {
                            var parse = int.Parse(y);
                            var call = sumDelegate(x, parse);
                            result = component.Multiply(call, 7);
                        });

            dynamic adapter = GetAdapter(new QuickStartModel());
            adapter.SetInteger(null, 3/*, default: 6*/);

            result.ShouldBe((3 + 6) * 7);
        }

        [Fact]
        public void AddMethodWithSpecifyDelegateThatReferencesAProperty()
        {
            var newProp = Spec.SpecifyProperty("NewProperty")
                .SpecifyBackingComponentProperty(m => m.String);

            Spec.SpecifyMethod()
                .WithFunctionSignature<string>("GetNewProperty")
                .SpecifyDelegate(Param.Getter(newProp), p => p());

            dynamic adapter = GetAdapter(new QuickStartModel() { String = "test" });

            string methodReturn = adapter.GetNewProperty();
            methodReturn.ShouldBe("test");
        }

        [Fact]
        public void AddMethodWithSpecifyLinqThatUsesArg()
        {
            Spec.SpecifyMethod()
                .WithFunctionSignature<int, int, int>("Sum")
                .SpecifyLinq(c => (Spec.Linq.Arg<int>(1) + Spec.Linq.Arg<int>(2)));

            dynamic adapter = GetAdapter();
            int sum = adapter.Sum(3, 7);

            sum.ShouldBe(10);
        }

        [Fact]
        public void AddMethodWithSpecifyLinqThatCallsAnotherMethod()
        {
            var sum = Spec.SpecifyMethod()
                .WithFunctionSignature<int, int, int>("Sum")
                .SpecifyLinq(c => Spec.Linq.Arg<int>(1) + Spec.Linq.Arg<int>(2));

            Spec.SpecifyMethod()
                .WithFunctionSignature<int>("Sum2")
                .SpecifyLinq(c => Spec.Linq.Function(sum, 2, 3));

            dynamic adapter = GetAdapter();
            int sum2 = adapter.Sum2();

            sum2.ShouldBe(5);
        }

        [Fact]
        public void AddMethodWithSpecifyLinqWithComplexExpression()
        {
            var sum = Spec.SpecifyMethod()
                .WithFunctionSignature<int, int, int>("Sum")
                .SpecifyLinq(c => Spec.Linq.Arg<int>(1) + Spec.Linq.Arg<int>(2));

            var setInt = Spec.SpecifyMethod()
                            .WithFunctionSignature<object, int, string, int>(
                                "SetInteger", param3: ParamSettings.WithName("y").WithDefault("6"))
                            .SpecifyLinq(
                m => m.Multiply(7, Spec.Linq.Function(sum, Spec.Linq.Arg<int>(2), int.Parse(Spec.Linq.Arg<string>(3)))));

            dynamic adapter = GetAdapter(new QuickStartModel());

            int result = (int)adapter.SetInteger(null, 3);
            result.ShouldBe((3 + 6) * 7);
        }

        [Fact]
        public void AddMethodWithSpecifyLinqThatReferencesAProperty()
        {
            var prop = Spec.SpecifyProperty(x => x.String);

            Spec.SpecifyMethod()
                .WithFunctionSignature<int, string>("Concat")
                .SpecifyLinq(m => Spec.Linq.Getter(prop) + Spec.Linq.Arg<int>(1).ToString());

            dynamic adapter = GetAdapter(new QuickStartModel() { String = "test" });
            string result = adapter.Concat(5);
            result.ShouldBe("test5");
        }

        [Fact]
        public void AddMethodWithSpecifyLinqBindsLocalsEarly()
        {
            int randomNumber = 4;
            Spec.SpecifyMethod()
                .WithFunctionSignature<int>("Return4")
                .SpecifyLinq(m => randomNumber);
            randomNumber = 6;

            dynamic adapter = GetAdapter(new QuickStartModel());
            int four = adapter.Return4();

            four.ShouldBe(4);
        }

        [Fact]
        public void AddMethodWithSpecifyLinqReferencingNonConstableLocalsThrows()
        {
            object o = new object();
            var spec = Spec.SpecifyMethod().WithFunctionSignature<object>("MakeObject");

            var ex = Should.Throw<ArgumentException>(() => spec.SpecifyLinq(m => o));
            
            ex.ParamName.ShouldBe("expr");
        }

        [Fact(Skip ="Feature not implemented")]
        public void AddPropertyWithTypeConvertingFilter()
        {
            var factory = Spec.Finish();

            var spec = Specification.New<QuickStartModel2>();
            spec.SpecifyProperty(x => x.CentralModel)
                //.SetBidirectionalFilter(factory.Type,
                //    x => factory.Create(x),
                //    x => x.Source)
                    ;
        }
    }

    public class QuickStartModel2
    {
        public QuickStartModel CentralModel { get; set; }
    }
}
