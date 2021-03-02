using Shouldly;
using Xunit;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace NAdapter.Test
{
    public class ByDefaultMethodsShouldAttribute: Attribute { }

    public class ByDefaultMethodsShouldModel
    {
        [ByDefaultMethodsShould]
        public string Convert([ByDefaultMethodsShould]int i = 3)
        {
            return i.ToString();
        }
    }
    
    public class ByDefaultMethodsShould: TestBase<ByDefaultMethodsShouldModel>
    {
        IAdapter<ByDefaultMethodsShouldModel> Adapter;
        dynamic Dynamic { get { return Adapter; } }
        ByDefaultMethodsShouldModel Component;
        PropertyInfo GetProperty(string name) => Adapter.GetType().GetProperty(name);
        MethodInfo GetMethod(string name) => Adapter.GetType().GetMethod(name);
        ParameterInfo GetParameter(string methodName, int paramIndex) => GetMethod(methodName).GetParameters()[paramIndex];
        
        public ByDefaultMethodsShould()
        {
            Component = new ByDefaultMethodsShouldModel();
            Adapter = GetAdapter(Component);
        }

        [Fact]
        public void GivenNoSetUp_WhenComponentHasMethod_ThenAdapterMethodCallsComponentMethod()
        {
            string seventeen = Dynamic.Convert(17);
            seventeen.ShouldBe("17");
        }

        [Fact]
        public void GivenNoSetUp_WhenComponentHasMethod_ThenParameterNameEqualsComponentParameter()
        {
            var parameter = GetParameter(nameof(ByDefaultMethodsShouldModel.Convert), 0);
            parameter.Name.ShouldBe("i");
        }

        [Fact]
        public void GivenNoSetUp_WhenComponentHasMethod_ThenParameterDefaultValueEqualsComponentParameterDefault()
        {
            string defaultValue = Dynamic.Convert();
            defaultValue.ShouldBe("3");
        }

        [Fact]
        public void GivenNoSetUp_WhenComponentHasMethodWithAttribute_ThenAdapterMethodHasSameAttribute()
        {
            GetMethod(nameof(ByDefaultMethodsShouldModel.Convert))
                .GetCustomAttributes()
                .ShouldHaveSingleItem()
                .ShouldBeOfType<ByDefaultMethodsShouldAttribute>();
        }

        [Fact]
        public void GivenNoSetUp_WhenComponentMethodHasParameterWithAttribute_ThenAdapterParamHasSameAttribute()
        {
            GetParameter(nameof(ByDefaultMethodsShouldModel.Convert), 0)
                .GetCustomAttributes()
                .Where(a => !(a is OptionalAttribute))
                .ShouldHaveSingleItem()
                .ShouldBeOfType<ByDefaultMethodsShouldAttribute>();
        }
    }
}
