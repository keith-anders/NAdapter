using Shouldly;
using Xunit;
using System.Reflection;

namespace NAdapter.Test
{
    public class CustomizedParameterShouldModel
    {
        public int Sum([AdapterTest("test")]int x, int y) { return x + y; }
    }
    
    public class CustomizedParameterShould: TestBase<CustomizedParameterShouldModel>
    {
        [Fact]
        public void WhenGivingParameterName_ThenAdapterParameterHasName()
        {
            Spec.SpecifyMethod()
                .WithFunctionSignature<int, int, int>(nameof(CustomizedParameterShouldModel.Sum),
                    ParamSettings.WithName("first"));

            var parameterInfo = GetAdapter().GetType().GetMethod(nameof(CustomizedParameterShouldModel.Sum)).GetParameters()[0];

            parameterInfo.Name.ShouldBe("first");
        }

        [Fact]
        public void WhenGivingParameterDefault_ThenAdapterParameterHasDefault()
        {
            Spec.SpecifyMethod()
                .WithFunctionSignature<int, int, int>(nameof(CustomizedParameterShouldModel.Sum),
                    param2: ParamSettings.WithName("second").WithDefault(5));

            dynamic adapter = GetAdapter(new CustomizedParameterShouldModel());

            int result = adapter.Sum(3);
            result.ShouldBe(8);
        }

        [Fact]
        public void WhenGivingParameterName_ThenOtherParameterNamesStayTheSame()
        {
            Spec.SpecifyMethod()
                .WithFunctionSignature<int, int, int>(nameof(CustomizedParameterShouldModel.Sum),
                    ParamSettings.WithName("first"));

            var parameterInfo = GetAdapter().GetType().GetMethod(nameof(CustomizedParameterShouldModel.Sum)).GetParameters()[1];
            
            parameterInfo.Name.ShouldBe("y");
        }
        
        [Fact]
        public void WhenParameterAttributeIsAdded_ThenAdapterParameterHasAttribute()
        {
            var p = ParamSettings.WithName("x");
            var pAtts = p.Attributes;
            Spec.SpecifyMethod()
                .WithFunctionSignature<int, int, int>(nameof(CustomizedParameterShouldModel.Sum), p);
            IAdapter<CustomizedParameterShouldModel> adapter = null;
            var attributeTester = new AttributeTester<ParameterInfo>(
                () => { },
                () => pAtts,
                () => adapter = Spec.Finish().Create(null),
                () => adapter.GetType().GetMethod(nameof(CustomizedParameterShouldModel.Sum)).GetParameters()[0],
                pi => pi.GetCustomAttributes());

            attributeTester.TestNewAttribute();
        }

        [Fact]
        public void WhenParameterAttributeIsConverted_ThenAdapterParameterHasConvertedAttribute()
        {
            var p = ParamSettings.WithName("x");
            var pAtts = p.Attributes;
            Spec.SpecifyMethod()
                .WithFunctionSignature<int, int, int>(nameof(CustomizedParameterShouldModel.Sum), p);
            IAdapter<CustomizedParameterShouldModel> adapter = null;
            var attributeTester = new AttributeTester<ParameterInfo>(
                () => { },
                () => pAtts,
                () => adapter = Spec.Finish().Create(null),
                () => adapter.GetType().GetMethod(nameof(CustomizedParameterShouldModel.Sum)).GetParameters()[0],
                pi => pi.GetCustomAttributes());

            attributeTester.TestConvertingAttribute();
        }

        [Fact]
        public void WhenParameterAttributeIsHidden_ThenAdapterParameterDoesNotHaveHiddenAttribute()
        {
            var p = ParamSettings.WithName("x");
            var pAtts = p.Attributes;
            Spec.SpecifyMethod()
                .WithFunctionSignature<int, int, int>(nameof(CustomizedParameterShouldModel.Sum), p);
            IAdapter<CustomizedParameterShouldModel> adapter = null;
            var attributeTester = new AttributeTester<ParameterInfo>(
                () => { },
                () => pAtts,
                () => adapter = Spec.Finish().Create(null),
                () => adapter.GetType().GetMethod(nameof(CustomizedParameterShouldModel.Sum)).GetParameters()[0],
                pi => pi.GetCustomAttributes());

            attributeTester.TestHidingAttribute();
        }
    }
}
