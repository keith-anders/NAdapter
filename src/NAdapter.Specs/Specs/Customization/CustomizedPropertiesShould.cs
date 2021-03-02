using Moq;
using Shouldly;
using Xunit;
using System;
using System.Linq;
using System.Reflection;
using Xunit.Sdk;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.CSharp.RuntimeBinder;
using System.Linq.Expressions;
using System.IO;

namespace NAdapter.Test
{
    public class CustomizedPropertyShould1Attribute : Attribute { }
    public class CustomizedPropertyShould2Attribute : Attribute { }

    public class CustomizedPropertyShouldModel
    {
        [CustomizedPropertyShould1]
        public string P1 { get; set; }

        public int P2 { get; set; }

        public List<string> P3 { get; set; }
    }

    public abstract class PropertyTestAttribute : DataAttribute
    {
        protected IEnumerable<object[]> Values()
        {
            return new[]
            {
                new object[] { new PropertyAdded() },
                new object[] { new PropertyExistingByExpression() },
                new object[] { new PropertyExistingByName() },
                new object[] { new PropertyExistingByISpecification() }
            };
        }
    }

    public class PropertyBehaviorTestAttribute : PropertyTestAttribute
    {
        public override IEnumerable<object[]> GetData(MethodInfo testMethod) => Values();
    }

    public class PropertyExistingBehaviorTestAttribute : PropertyTestAttribute
    {
        public override IEnumerable<object[]> GetData(MethodInfo testMethod) => Values().Where(o => o[0] is PropertyExisting);
    }

    public abstract class PropertyBehaviorManager
    {
        public abstract void Behavior(Action<PropertyBehavior<CustomizedPropertyShouldModel>> behavior);
        public IAdapter<CustomizedPropertyShouldModel> GetAdapter(CustomizedPropertyShouldModel model = null)
        {
            Adapter = Finish(model);
            return Adapter;
        }
        public abstract dynamic Value { get; set; }
        public PropertyInfo TypePropertyInfo { get { return Adapter.GetType().GetProperty(PropertyName); } }

        protected abstract string PropertyName { get; }
        protected virtual IAdapter<CustomizedPropertyShouldModel> Finish(CustomizedPropertyShouldModel model) => Spec.Finish().Create(model);
        public IAdapter<CustomizedPropertyShouldModel> Adapter { get; private set; }
        public dynamic DynamicAdapter { get { return Adapter; } }
        public Specification<CustomizedPropertyShouldModel> Spec { get; private set; } = Specification.New<CustomizedPropertyShouldModel>();
    }

    public class PropertyAdded : PropertyBehaviorManager
    {
        public override void Behavior(Action<PropertyBehavior<CustomizedPropertyShouldModel>> action)
        {
            action(Spec.SpecifyProperty(PropertyName).SpecifyAutoImplemented<string>());
        }
        public override dynamic Value { get { return DynamicAdapter.NewProperty; } set { DynamicAdapter.NewProperty = value; } }
        protected override string PropertyName => "NewProperty";
    }

    public abstract class PropertyExisting : PropertyBehaviorManager
    {
        public override dynamic Value { get { return DynamicAdapter.P1; } set { DynamicAdapter.P1 = value; } }
        protected override string PropertyName => nameof(DynamicAdapter.P1);
    }

    public class PropertyExistingByExpression : PropertyExisting
    {
        public override void Behavior(Action<PropertyBehavior<CustomizedPropertyShouldModel>> action)
        {
            action(Spec.SpecifyProperty(x => x.P1));
        }
    }

    public class PropertyExistingByName : PropertyExisting
    {
        public override void Behavior(Action<PropertyBehavior<CustomizedPropertyShouldModel>> action)
        {
            action(Spec.SpecifyProperty(PropertyName));
        }
    }

    public class PropertyExistingByISpecification : PropertyExisting
    {
        AdapterFactory<CustomizedPropertyShouldModel> _result;

        public override void Behavior(Action<PropertyBehavior<CustomizedPropertyShouldModel>> action)
        {
            var mock = new Mock<ISpecification<CustomizedPropertyShouldModel>>();
            Spec.Specify(mock.Object);
            mock.Setup(x => x.OnProperty(It.IsAny<PropertyBehavior<CustomizedPropertyShouldModel, string>>())).Callback<PropertyBehavior<CustomizedPropertyShouldModel, string>>(p =>
            {
                if (p.Name == nameof(CustomizedPropertyShouldModel.P1))
                    action(p);
            });
            _result = Spec.Finish();
        }
        protected override IAdapter<CustomizedPropertyShouldModel> Finish(CustomizedPropertyShouldModel model)
        {
            return _result.Create(model);
        }
    }

    public class CustomizedPropertyShould
    {
        [Theory, PropertyBehaviorTest]
        public void WhenPropertyIsSpecifiedAsAutoImplemented_ThenPropertyBehavesAsAutoImplemented(PropertyBehaviorManager manager)
        {
            manager.Behavior(b => b.SpecifyAutoImplemented<string>());
            
            var adapter = manager.GetAdapter();

            manager.Value = "42";
            string d = manager.Value;
            d.ShouldBe("42");
        }

        [Theory, PropertyBehaviorTest]
        public void WhenPropertyIsSpecifiedAsComponentPropertyInfo_ThenComponentPropertyInfoIsExposed(PropertyBehaviorManager manager)
        {
            manager.Behavior(b => b.SpecifyBackingComponentProperty(typeof(CustomizedPropertyShouldModel).GetProperty(nameof(CustomizedPropertyShouldModel.P1)))
                    .BackingPropertyInfo
                    .Name
                    .ShouldBe(nameof(CustomizedPropertyShouldModel.P1)));
        }

        [Theory, PropertyBehaviorTest]
        public void WhenPropertyIsSpecifiedAsComponentPropertyInfo_ThenPropertySetterCallsComponentPropertySetter(PropertyBehaviorManager manager)
        {
            manager.Behavior(b => b.SpecifyBackingComponentProperty(typeof(CustomizedPropertyShouldModel).GetProperty(nameof(CustomizedPropertyShouldModel.P1))));

            var model = new CustomizedPropertyShouldModel();
            var adapter = manager.GetAdapter(model);

            manager.Value = "test";
            model.P1.ShouldBe("test");
        }

        [Theory, PropertyBehaviorTest]
        public void WhenPropertyIsSpecifiedAsComponentPropertyInfo_ThenPropertyGetterReturnsComponentProperty(PropertyBehaviorManager manager)
        {
            manager.Behavior(b => b.SpecifyBackingComponentProperty(typeof(CustomizedPropertyShouldModel).GetProperty(nameof(CustomizedPropertyShouldModel.P1))));

            var model = new CustomizedPropertyShouldModel();
            manager.GetAdapter(model);

            model.P1 = "test";
            string value = manager.Value;
            value.ShouldBe("test");
        }

        [Theory, PropertyBehaviorTest]
        public void WhenPropertyIsSpecifiedAsComponentProperty_ThenComponentPropertyInfoIsExposed(PropertyBehaviorManager manager)
        {
            manager.Behavior(b => b.SpecifyBackingComponentProperty(m => m.P1)
                    .BackingPropertyInfo
                    .Name
                    .ShouldBe(nameof(CustomizedPropertyShouldModel.P1)));
        }

        [Theory, PropertyBehaviorTest]
        public void WhenPropertyIsSpecifiedAsComponentProperty_ThenPropertySetterCallsComponentPropertySetter(PropertyBehaviorManager manager)
        {
            manager.Behavior(b => b.SpecifyBackingComponentProperty(m => m.P1));

            var model = new CustomizedPropertyShouldModel();
            manager.GetAdapter(model);

            manager.Value = "test";
            model.P1.ShouldBe("test");
        }

        [Theory, PropertyBehaviorTest]
        public void WhenPropertyIsSpecifiedAsComponentProperty_ThenPropertyGetterReturnsComponentProperty(PropertyBehaviorManager manager)
        {
            manager.Behavior(b => b.SpecifyBackingComponentProperty(m => m.P1));

            var model = new CustomizedPropertyShouldModel();
            manager.GetAdapter(model);

            model.P1 = "test";
            string value = manager.Value;
            value.ShouldBe("test");
        }

        [Theory, PropertyBehaviorTest]
        public void WhenPropertyIsSpecifiedAsDelegates_ThenGetterCallsGetterFunc(PropertyBehaviorManager manager)
        {
            string backingField = null;
            manager.Behavior(b => b.SpecifyDelegates(
                    component => $"{component.P1}.{backingField}",
                    null));

            manager.GetAdapter(new CustomizedPropertyShouldModel() { P1 = "test0" });

            backingField = "test1";
            string value = manager.Value;
            value.ShouldBe("test0.test1");
        }

        [Theory, PropertyBehaviorTest]
        public void WhenPropertyIsSpecifiedAsDelegates_ThenSetterCallsSetterFunc(PropertyBehaviorManager manager)
        {
            string backingField = null;
            manager.Behavior(b => b.SpecifyDelegates<string>(
                    null,
                    (component, value) => backingField = value));

            manager.GetAdapter(new CustomizedPropertyShouldModel() { P1 = "test0" });
            
            manager.Value = "test2";
            backingField.ShouldBe("test2");
        }

        [Theory, PropertyBehaviorTest]
        public void WhenPropertyIsSpecifiedWithNullGetter_ThenMakePublicGetterThrows(PropertyBehaviorManager manager)
        {
            Should.Throw<InvalidPropertySpecificationException>(() =>
                {
                    string backingField = null;
                    manager.Behavior(b => b.SpecifyDelegates<string>(
                                null,
                                (component, value) => backingField = value)
                                .Decoration
                                .SpecifyReadWrite());
                    manager.GetAdapter();
                });
        }

        [Theory, PropertyBehaviorTest]
        public void WhenPropertyIsSpecifiedWithNullSetter_ThenMakePublicSetterThrows(PropertyBehaviorManager manager)
        {
            Should.Throw<InvalidPropertySpecificationException>(() =>
            {
                manager.Behavior(b => b.SpecifyDelegates(
                            component => component.P1,
                            null)
                            .Decoration
                            .SpecifyPublicSetter());
                manager.GetAdapter();
            });
        }

        [Theory, PropertyBehaviorTest]
        public void WhenPropertyIsSpecifiedWithBothDelegatesNull_ThenSpecificationThrows(PropertyBehaviorManager manager)
        {
            Should.Throw<InvalidPropertySpecificationException>(() =>
            {
                manager.Behavior(b => b.SpecifyDelegates<string>(null, null));
                manager.GetAdapter();
            });
        }

        [Theory, PropertyExistingBehaviorTest]
        public void WhenPropertyNameIsChanged_ThenAdapterHasPropertyWithNewName(PropertyBehaviorManager manager)
        {
            manager.Behavior(b => b.SpecifyBackingComponentProperty(m => m.P1)
                     .Decoration
                     .PublicName = "FauxString");

            var model = new CustomizedPropertyShouldModel();
            manager.GetAdapter(model);

            model.P1 = "test";
            string result = manager.DynamicAdapter.FauxString;
            result.ShouldBe("test");
        }

        [Theory, PropertyBehaviorTest]
        public void WhenPropertyIsHidden_ThenAdapterDoesNotHaveProperty(PropertyBehaviorManager manager)
        {
            manager.Behavior(b => b.Decoration.IsHidden = true);

            manager.GetAdapter();

            manager.TypePropertyInfo.ShouldBeNull();
        }

        [Theory, PropertyBehaviorTest]
        public void WhenPropertyIsSpecifiedReadOnly_ThenAdapterPropertySetterThrowsMissingMemberException(PropertyBehaviorManager manager)
        {
            manager.Behavior(b => b
                .SpecifyBackingComponentProperty(m => m.P1)
                .Decoration
                .SpecifyReadOnly());

            var model = new CustomizedPropertyShouldModel() { P1 = "test" };
            manager.GetAdapter(model);

            string value = manager.Value;
            value.ShouldBe("test");

            var ex = Assert.Throws<RuntimeBinderException>(() =>
            {
                manager.Value = "test2";
            });
            ex.Message.ShouldContain("cannot be");
        }

        [Theory, PropertyBehaviorTest]
        public void WhenPropertyIsSpecifiedPrivateGetter_ThenAdapterPropertyGetterIsPrivate(PropertyBehaviorManager manager)
        {
            manager.Behavior(b => b
                .Decoration
                .SpecifyPrivateGetter());

            var model = new CustomizedPropertyShouldModel() { P1 = "test" };
            manager.GetAdapter(model);

            var ex = Assert.Throws<RuntimeBinderException>(() =>
            {
                object value = manager.Value;
            });
            ex.Message.ShouldMatch("(protection level|inaccessible)");
        }

        [Theory, PropertyBehaviorTest]
        public void WhenPropertyIsSpecifiedPrivateGetter_ThenAdapterPropertyGetterWorks(PropertyBehaviorManager manager)
        {
            manager.Behavior(b => b
                .SpecifyBackingComponentProperty(x => x.P1)
                .Decoration
                .SpecifyPrivateGetter());

            var model = new CustomizedPropertyShouldModel() { P1 = "test" };
            manager.GetAdapter(model);

            string result = manager.TypePropertyInfo.GetGetMethod(true).Invoke(manager.Adapter, new object[0]) as string;
            result.ShouldBe("test");
        }

        [Theory, PropertyBehaviorTest]
        public void WhenPropertyIsSpecifiedWriteOnly_ThenAdapterPropertySetterThrowsMissingMember(PropertyBehaviorManager manager)
        {
            manager.Behavior(b => b
                .SpecifyBackingComponentProperty(m => m.P1)
                .Decoration
                .SpecifyWriteOnly());

            var model = new CustomizedPropertyShouldModel();
            manager.GetAdapter(model);

            manager.Value = "test";
            model.P1.ShouldBe("test");

            var ex = Assert.Throws<RuntimeBinderException>(() =>
            {
                object value = manager.Value;
            });
            ex.Message.ShouldContain("cannot be");
        }

        [Theory, PropertyBehaviorTest]
        public void WhenPropertyIsSpecifiedPrivateSetter_ThenAdapterPropertySetterIsPrivate(PropertyBehaviorManager manager)
        {
            manager.Behavior(b => b
                .Decoration
                .SpecifyPrivateSetter());

            var model = new CustomizedPropertyShouldModel();
            manager.GetAdapter(model);

            var ex = Assert.Throws<RuntimeBinderException>(() =>
            {
                manager.Value = "Test";
            });
            ex.Message.ShouldMatch("(protection level|inaccessible)");
        }

        [Theory, PropertyBehaviorTest]
        public void WhenPropertyIsSpecifiedPrivateSetter_ThenAdapterPropertySetterWorks(PropertyBehaviorManager manager)
        {
            manager.Behavior(b => b
                .SpecifyBackingComponentProperty(x => x.P1)
                .Decoration
                .SpecifyPrivateSetter());

            var model = new CustomizedPropertyShouldModel();
            manager.GetAdapter(model);

            manager.TypePropertyInfo.GetSetMethod(true).Invoke(manager.Adapter, new object[] { "test" });
            model.P1.ShouldBe("test");
        }

        [Theory, PropertyBehaviorTest]
        public void WhenPropertyIsSpecifiedVirtual_ThenAdapterPropertyGetterIsVirtual(PropertyBehaviorManager manager)
        {
            manager.Behavior(b => b
                .Decoration
                .IsVirtual = true);

            manager.GetAdapter();

            manager.TypePropertyInfo.GetGetMethod().IsVirtual.ShouldBeTrue();
        }

        [Theory, PropertyBehaviorTest]
        public void WhenPropertyIsSpecifiedVirtual_ThenAdapterPropertySetterIsVirtual(PropertyBehaviorManager manager)
        {
            manager.Behavior(b => b
                .Decoration
                .IsVirtual = true);

            manager.GetAdapter();

            manager.TypePropertyInfo.GetSetMethod().IsVirtual.ShouldBeTrue();
        }

        [Theory, PropertyBehaviorTest]
        public void WhenPropertyAttributeIsAdded_ThenAdapterPropertyHasAttribute(PropertyBehaviorManager manager)
        {
            manager.Behavior(b => b
                .Decoration
                .Attributes
                .AddAttribute(() => new CustomizedPropertyShould2Attribute()));

            manager.GetAdapter();

            manager.TypePropertyInfo.GetCustomAttributes(true)
                .OfType<CustomizedPropertyShould2Attribute>()
                .ShouldHaveSingleItem()
                .ShouldNotBeNull();
        }

        [Theory, PropertyExistingBehaviorTest]
        public void WhenPropertyAttributeIsHidden_ThenAdapterPropertyDoesNotHaveAttribute(PropertyBehaviorManager manager)
        {
            manager.Behavior(b => b
                .Decoration
                .Attributes
                .HideAttributesOfType<CustomizedPropertyShould1Attribute>());

            manager.GetAdapter();

            manager.TypePropertyInfo.GetCustomAttributes(true).ShouldBeEmpty();
        }

        [Theory, PropertyExistingBehaviorTest]
        public void WhenPropertyAttributeIsConverted_ThenAdapterPropertyHasConvertedAttribute(PropertyBehaviorManager manager)
        {
            manager.Behavior(b => b
                .Decoration
                .Attributes
                .RegisterAttributeConversion<CustomizedPropertyShould1Attribute>(a => new CustomizedPropertyShould2Attribute()));

            manager.GetAdapter();

            manager.TypePropertyInfo.GetCustomAttributes(true)
                .Cast<CustomizedPropertyShould2Attribute>()
                .ShouldHaveSingleItem()
                .ShouldNotBeNull();
        }

        [Theory, PropertyExistingBehaviorTest]
        public void WhenPropertyAttributeIsConverted_ThenAdapterPropertyDoesNotHaveOriginalAttribute(PropertyBehaviorManager manager)
        {
            manager.Behavior(b => b
                .Decoration
                .Attributes
                .RegisterAttributeConversion<CustomizedPropertyShould1Attribute>(a => new CustomizedPropertyShould2Attribute()));

            manager.GetAdapter();

            manager.TypePropertyInfo.GetCustomAttributes(true)
                .OfType<CustomizedPropertyShould1Attribute>()
                .ShouldBeEmpty();
        }

        [Theory, PropertyBehaviorTest]
        public void WhenPropertyGetterFilterIsAdded_ThenFilterIsApplied(PropertyBehaviorManager manager)
        {
            manager.Behavior(b => b
                .SpecifyBackingComponentProperty(m => m.P2)
                .AddGetterFilter(i => i + 1)
                .AddGetterFilter(i => i + 2));

            manager.GetAdapter(new CustomizedPropertyShouldModel()
            {
                P2 = 10
            });

            int value = manager.Value;
            value.ShouldBe(13);
        }

        [Theory, PropertyBehaviorTest]
        public void WhenPropertyGetterFilterIsAddedWithComponent_ThenFilterIsApplied(PropertyBehaviorManager manager)
        {
            manager.Behavior(b => b
                .SpecifyBackingComponentProperty(m => m.P2)
                .AddGetterFilter((i, c) => c.P1 == null ? i + 1 : i));

            var model = new CustomizedPropertyShouldModel() { P2 = 5 };
            manager.GetAdapter(model);

            int result = manager.Value;
            // P1 is null, so P2 should return (actual P2 = 5) + 1 = 6
            result.ShouldBe(6);
            model.P1 = "test";
            // P1 is not null, so P2 should return actual P2 = 5;
            result = manager.Value;
            result.ShouldBe(5);
        }

        // This filters the getter of the P3 (List<string>) property
        // to return a new list if the current value is null, and also
        // sets the value to be the returned list if that substitution
        // had to be performed.
        [Theory, PropertyBehaviorTest]
        public void WhenPropertyGetterFilterIsAddedWithComponentAndSetter_ThenFilterIsApplied(PropertyBehaviorManager manager)
        {
            var list = Expression.Parameter(typeof(List<string>), "list");
            var comp = Expression.Parameter(typeof(CustomizedPropertyShouldModel), "comp");
            var setter = Expression.Parameter(typeof(Action<List<string>>), "setter");
            var newList = Expression.Parameter(typeof(List<string>), "newList");
            LambdaExpression lambda = Expression.Lambda(
                Expression.Condition(
                    Expression.Equal(list, Expression.Constant(null)),
                    Expression.Block(new [] { newList },
                        Expression.Assign(newList, Expression.New(typeof(List<string>))),
                        Expression.Invoke(setter, newList),
                        newList) , list)
                , list, comp, setter);
            manager.Behavior(b => b
                .SpecifyBackingComponentProperty(m => m.P3)
                .AddGetterFilter(lambda));

            var model = new CustomizedPropertyShouldModel();
            manager.GetAdapter(model);

            model.P3.ShouldBeNull();

            List<string> result = manager.Value;
            result.ShouldNotBeNull();
            result.ShouldBeSameAs(model.P3);
        }

        [Theory, PropertyBehaviorTest]
        public void WhenPropertySetterFilterIsAdded_ThenFilterIsApplied(PropertyBehaviorManager manager)
        {
            manager.Behavior(b => b
                .SpecifyBackingComponentProperty(m => m.P2)
                .AddSetterFilter(i => i + 1)
                .AddSetterFilter(i => i + 2));

            var model = new CustomizedPropertyShouldModel();
            manager.GetAdapter(model);

            manager.Value = 10;
            model.P2.ShouldBe(13);
        }

        [Theory, PropertyBehaviorTest]
        public void WhenPropertySetterFilterIsAddedWithComponent_ThenFilterIsApplied(PropertyBehaviorManager manager)
        {
            manager.Behavior(b => b
                .SpecifyBackingComponentProperty(m => m.P2)
                .AddSetterFilter((i, c) => c.P1 == null ? i + 1 : i));

            var model = new CustomizedPropertyShouldModel();
            manager.GetAdapter(model);

            manager.Value = 5;
            // P1 is null, so P2 should be set to 5 + 1 = 6
            model.P2.ShouldBe(6);

            model.P1 = "test";
            manager.Value = 10;
            // P1 is not null, so P2 should bet set to 10
            model.P2.ShouldBe(10);
        }

        // This filters the setter of the P3 (List<string>) property
        // to merely AddRange the entries to the existing value, instead
        // of overwriting the list reference, and set the value to a new list
        // if it is null.
        [Theory, PropertyBehaviorTest]
        public void WhenPropertySetterFilterIsAddedWithComponentAndGetter_ThenFilterIsApplied(PropertyBehaviorManager manager)
        {
            var list = Expression.Parameter(typeof(List<string>), "value");
            var comp = Expression.Parameter(typeof(CustomizedPropertyShouldModel), "comp");
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
            manager.Behavior(b => b
                .SpecifyBackingComponentProperty(m => m.P3)
                .AddSetterFilter(lambda));

            var model = new CustomizedPropertyShouldModel();
            manager.GetAdapter(model);

            model.P3.ShouldBeNull();
            manager.Value = new List<string>() { "test1" };
            model.P3.ShouldHaveSingleItem()
                .ShouldBe("test1");

            manager.Value = new List<string>() { "test2" };
            model.P3[0].ShouldBe("test1");
            model.P3[1].ShouldBe("test2");
        }
    }
}
