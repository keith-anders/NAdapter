using Shouldly;
using Xunit;
using System;
using System.Reflection;

namespace NAdapter.Test
{
    public class ByDefaultPropertiesShouldAttribute : Attribute { }

    public class ByDefaultPropertiesShouldModel
    {
        public ByDefaultPropertiesShouldModel(string value, string privateSet)
        {
            ReadOnlyString = value;
            StringWithPrivateSetter = privateSet;
        }

        [ByDefaultPropertiesShould]
        public string String { get; set; }

        public string WriteOnlyString { set { WriteOnlyStringSet?.Invoke(value); } }

        public string ReadOnlyString { get; }

        public string StringWithPrivateGetter { private get; set; }

        public string StringWithPrivateSetter { get; private set; }

        public delegate void StringWritten(string value);

        public event StringWritten WriteOnlyStringSet;
    }
    
    public class ByDefaultPropertiesShould: TestBase<ByDefaultPropertiesShouldModel>
    {
        IAdapter<ByDefaultPropertiesShouldModel> Adapter;
        dynamic Dynamic { get { return Adapter; } }
        ByDefaultPropertiesShouldModel Component;
        PropertyInfo GetProperty(string name) => Adapter.GetType().GetProperty(name);
        string LastSetWriteOnlyString;
        const string ReadOnlyStringValue = "Test";
        const string PrivateSetStringValue = "private";
        
        public ByDefaultPropertiesShould()
        {
            Component = new ByDefaultPropertiesShouldModel(ReadOnlyStringValue, PrivateSetStringValue);
            Component.WriteOnlyStringSet += s => LastSetWriteOnlyString = s;
            Adapter = GetAdapter(Component);
        }

        [Fact]
        public void GivenNoSetUp_WhenComponentHasProperty_ThenAdapterPropertyGetterReturnsComponentProperty()
        {
            Component.String = "test";
            string result = Dynamic.String;

            result.ShouldBe("test");
        }

        [Fact]
        public void GivenNoSetUp_WhenComponentHasProperty_ThenAdapterPropertySetterCallsComponentProperty()
        {
            Dynamic.String = "test";

            Component.String.ShouldBe("test");
        }

        [Fact]
        public void GivenNoSetUp_WhenComponentHasProperty_PropertyTypeEqualsComponentPropertyType()
        {
            Component.String = "test";
            object result = Dynamic.String;

            result.ShouldBeOfType<String>();
        }

        [Fact]
        public void GivenNoSetUp_WhenComponentPropertyHasNoGetter_AdapterPropertyHasNoGetter()
        {
            var prop = GetProperty(nameof(ByDefaultPropertiesShouldModel.WriteOnlyString));

            prop.ShouldSatisfyAllConditions(
                () => prop.CanRead.ShouldBeFalse(),
                () => prop.GetGetMethod().ShouldBeNull()
                );
        }

        [Fact]
        public void GivenNoSetUp_WhenComponentPropertyHasNoGetter_AdapterPropertyCallsComponentSetter()
        {
            Dynamic.WriteOnlyString = "test";

            LastSetWriteOnlyString.ShouldBe("test");
        }

        [Fact]
        public void GivenNoSetUp_WhenComponentPropertyHasNoSetter_AdapterPropertyHasNoSetter()
        {
            var prop = GetProperty(nameof(ByDefaultPropertiesShouldModel.ReadOnlyString));
            prop.ShouldSatisfyAllConditions(
                () => prop.CanWrite.ShouldBeFalse(),
                () => prop.GetSetMethod().ShouldBeNull()
                );
        }

        [Fact]
        public void GivenNoSetUp_WhenComponentPropertyHasNoSetter_AdapterPropertyCallsComponentGetter()
        {
            string result = Dynamic.ReadOnlyString;
            result.ShouldBe(ReadOnlyStringValue);
        }

        [Fact]
        public void GivenNoSetUp_WhenComponentPropertyHasPrivateGetter_AdapterPropertyHasPrivateGetter()
        {
            GetProperty(nameof(ByDefaultPropertiesShouldModel.StringWithPrivateGetter))
                .GetGetMethod(true)
                .IsPrivate
                .ShouldBeTrue();
        }

        [Fact]
        public void GivenNoSetUp_WhenComponentPropertyHasPrivateGetter_AdapterPropertyCallsPrivateGetter()
        {
            Component.StringWithPrivateGetter = "test";
            var prop = GetProperty(nameof(ByDefaultPropertiesShouldModel.StringWithPrivateGetter));
            var privateGetter = prop.GetGetMethod(true);

            privateGetter.Invoke(Adapter, new object[0]).ShouldBe("test");
        }

        [Fact]
        public void GivenNoSetUp_WhenComponentPropertyHasPrivateGetter_AdapterPropertyCallsSetter()
        {
            Dynamic.StringWithPrivateGetter = "test";

            typeof(ByDefaultPropertiesShouldModel)
                .GetProperty(nameof(ByDefaultPropertiesShouldModel.StringWithPrivateGetter))
                .GetGetMethod(true)
                .Invoke(Component, new object[0])
                .ShouldBe("test");
        }

        [Fact]
        public void GivenNoSetUp_WhenComponentPropertyHasPrivateSetter_AdapterPropertyHasPrivateSetter()
        {
            GetProperty(nameof(ByDefaultPropertiesShouldModel.StringWithPrivateSetter))
                .GetSetMethod(true)
                .IsPrivate
                .ShouldBeTrue();
        }

        [Fact]
        public void GivenNoSetUp_WhenComponentPropertyHasPrivateSetter_AdapterPropertyCallsPrivateSetter()
        {
            GetProperty(nameof(ByDefaultPropertiesShouldModel.StringWithPrivateSetter))
                .GetSetMethod(true)
                .Invoke(Adapter, new object[1] { "new string" });
            
            Component.StringWithPrivateSetter.ShouldBe("new string");
        }

        [Fact]
        public void GivenNoSetUp_WhenComponentPropertyHasPrivateSetter_AdapterPropertyCallsGetter()
        {
            string result = Dynamic.StringWithPrivateSetter;
            result.ShouldBe(PrivateSetStringValue);
        }

        [Fact]
        public void GivenNoSetUp_WhenComponentPropertyHasAttribute_AttributeIsCopiedToAdapter()
        {
            GetProperty(nameof(ByDefaultPropertiesShouldModel.String))
                .GetCustomAttributes()
                .ShouldHaveSingleItem()
                .ShouldBeOfType<ByDefaultPropertiesShouldAttribute>();
        }
    }
}
