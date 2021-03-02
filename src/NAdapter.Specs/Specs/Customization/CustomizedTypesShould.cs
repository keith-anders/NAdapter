using System;
using System.Reflection;
using Xunit;

namespace NAdapter.Test
{
    public class CustomizedTypesShouldAttribute: Attribute { }

    [AdapterTest("test")]
    public class CustomizedTypesShouldModel { }
    
    public class CustomizedTypesShould: TestBase<CustomizedTypesShouldModel>
    {
        IAdapter<CustomizedTypesShouldModel> Adapter;
        dynamic Dynamic { get { return Adapter; } }
        AttributeTester<Type> AttributeTesterViaType;
        
        void PrepType() => Spec = Specification.New<CustomizedTypesShouldModel>();
        
        void FinishType() => Adapter = Spec.Finish().Create(null);
        
        public CustomizedTypesShould()
        {
            AttributeTesterViaType = new AttributeTester<Type>(
                PrepType,
                () => Spec.Attributes,
                FinishType,
                () => Adapter.GetType(),
                t => t.GetCustomAttributes());
            Spec = null;
            Adapter = null;
        }

        [Fact]
        public void GivenType_WhenAttributeIsAdded_AdapterHasAttribute()
        {
            AttributeTesterViaType.TestNewAttribute();
        }

        [Fact]
        public void GivenType_WhenAttributeIsConverted_AdapterHasConvertedAttribute()
        {
            AttributeTesterViaType.TestConvertingAttribute();
        }

        [Fact]
        public void GivenType_WhenAttributeIsHidden_AdapterDoesNotHaveHiddenAttribute()
        {
            AttributeTesterViaType.TestHidingAttribute();
        }
    }
}
