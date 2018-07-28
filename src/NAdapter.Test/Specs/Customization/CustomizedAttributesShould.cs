using Shouldly;
using Xunit;
using System;
using System.Linq;
using System.Reflection;

namespace NAdapter.Test
{
    public class CustomizedAttributesShould1Attribute: Attribute
    {
        public string Name { get; private set; }

        public CustomizedAttributesShould1Attribute(string name) => Name = name;
    }

    public class CustomizedAttributesShould2Attribute: CustomizedAttributesShould1Attribute
    {
        public CustomizedAttributesShould2Attribute(string name): base(name) { }
    }

    public class CustomizedAttributesShould3Attribute: CustomizedAttributesShould2Attribute
    {
        public CustomizedAttributesShould3Attribute(string name): base(name) { }
    }

    public class CustomizedAttributesShould4Attribute: Attribute
    {
        public string Value { get; private set; }

        public CustomizedAttributesShould4Attribute(string value) => Value = value;
    }
    
    [CustomizedAttributesShould1("type")]
    public class CustomizedAttributesShouldModel
    {
        [CustomizedAttributesShould1("test")]
        public string P1 { get; set; }

        [CustomizedAttributesShould2("test2")]
        public string P2 { get; set; }

        [CustomizedAttributesShould3("test3")]
        public string P3 { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class CustomizedAttributesShould5Attribute : Attribute { }
    
    public class CustomizedAttributesShould: TestBase<CustomizedAttributesShouldModel>
    {
        Attribute[] Act(string name)
        {
            return GetAdapter(null).GetType().GetProperty(name).GetCustomAttributes().ToArray();
        }
        
        [Fact]
        public void GivenAttribute_WhenAttributeIsConvertedWithTrueCondition_ValueIfTrueApplies()
        {
            Spec.Attributes.RegisterAttributeConversion<CustomizedAttributesShould1Attribute>(a =>
                a.Name != null && a.Name.Contains("t") ?
                    (Attribute)new CustomizedAttributesShould1Attribute(a.Name.Replace('t', 'q')) :
                    new CustomizedAttributesShould4Attribute(a.Name));

            var attributes = Act(nameof(CustomizedAttributesShouldModel.P1));

            attributes
                .ShouldHaveSingleItem()
                .ShouldBeOfType<CustomizedAttributesShould1Attribute>()
                .Name
                .ShouldBe("qesq");
        }

        [Fact]
        public void GivenAttribute_WhenAttributeIsConvertedWithFalseCondition_ValueIfFalseApplies()
        {
            Spec.Attributes.RegisterAttributeConversion<CustomizedAttributesShould1Attribute>(a =>
                a.Name != null && a.Name.Contains("q") == true ?
                    (Attribute)new CustomizedAttributesShould1Attribute(a.Name.Replace('q', 'v')) :
                    new CustomizedAttributesShould4Attribute("Found!"));

            var attributes = Act(nameof(CustomizedAttributesShouldModel.P1));

            attributes
                .ShouldHaveSingleItem()
                .ShouldBeOfType<CustomizedAttributesShould4Attribute>()
                .Value
                .ShouldBe("Found!");
        }

        [Fact]
        public void GivenAttribute_WhenAttributeIsConverted_ConversionShouldApplyToSubclasses()
        {
            Spec.Attributes.RegisterAttributeConversion<CustomizedAttributesShould1Attribute>(a => new CustomizedAttributesShould4Attribute(a.Name));

            var attributes = Act(nameof(CustomizedAttributesShouldModel.P2));

            attributes
                .ShouldHaveSingleItem()
                .ShouldBeOfType<CustomizedAttributesShould4Attribute>()
                .Value
                .ShouldBe("test2");
        }

        [Fact]
        public void GivenAttribute_WhenAttributeIsConvertedWithNoSubclass_ConversionShouldNotApplyToSubclasses()
        {
            Spec.Attributes.RegisterAttributeConversion<CustomizedAttributesShould1Attribute>(a => new CustomizedAttributesShould4Attribute(a.Name), AttributeConversionBehavior.ThisAttributeTypeOnly);

            var attributes = Act(nameof(CustomizedAttributesShouldModel.P2));

            attributes
                .ShouldHaveSingleItem()
                .ShouldBeOfType<CustomizedAttributesShould2Attribute>()
                .Name
                .ShouldBe("test2");
        }
        
        [Fact]
        public void GivenAttribute_WhenAttributeConversionReturnsNull_AttributeIsHidden()
        {
            Spec.Attributes.RegisterAttributeConversion<CustomizedAttributesShould1Attribute>(a => null);

            Act(nameof(CustomizedAttributesShouldModel.P1)).ShouldBeEmpty();
        }

        [Fact]
        public void GivenAttribute_WhenAttributeConversionReturnsSameAttribute_DefaultConversionApplies()
        {
            Spec.Attributes.RegisterAttributeConversion<CustomizedAttributesShould1Attribute>(a => a);

            Act(nameof(CustomizedAttributesShouldModel.P1))
                .ShouldHaveSingleItem()
                .ShouldBeOfType<CustomizedAttributesShould1Attribute>()
                .Name
                .ShouldBe("test");
        }

        [Fact]
        public void GivenAttributeOnType_WhenAttributeConversionIsRestrictedToMemberType_ConversionDoesNotApplyToProperty()
        {
            Spec.Attributes.RegisterAttributeConversion<CustomizedAttributesShould1Attribute>(a => new CustomizedAttributesShould4Attribute(a.Name), AttributeConversionBehavior.ThisMemberTypeOnly);
            
            Act(nameof(CustomizedAttributesShouldModel.P1))
                .ShouldHaveSingleItem()
                .ShouldBeOfType<CustomizedAttributesShould1Attribute>()
                .Name
                .ShouldBe("test");
        }

        [Fact]
        public void GivenAttributeOnType_WhenAttributeConversionCollidesWithBaseClass_DerivedClassIsPreferred()
        {
            Spec.Attributes.RegisterAttributeConversion<CustomizedAttributesShould1Attribute>(a => new CustomizedAttributesShould4Attribute(a.Name));
            Spec.Attributes.RegisterAttributeConversion<CustomizedAttributesShould2Attribute>(a => new CustomizedAttributesShould2Attribute("derived"));

            Act(nameof(CustomizedAttributesShouldModel.P2))
                .ShouldHaveSingleItem()
                .ShouldBeOfType<CustomizedAttributesShould2Attribute>()
                .Name
                .ShouldBe("derived");
        }

        [Fact]
        public void GivenAttributeOnType_WhenAttributeConversionsOnBaseClassesCollide_DerivedClassIsPreferred()
        {
            Spec.Attributes.RegisterAttributeConversion<CustomizedAttributesShould1Attribute>(a => new CustomizedAttributesShould4Attribute(a.Name));
            Spec.Attributes.RegisterAttributeConversion<CustomizedAttributesShould2Attribute>(a => new CustomizedAttributesShould4Attribute("derived"));
            
            Act(nameof(CustomizedAttributesShouldModel.P3))
                .ShouldHaveSingleItem()
                .ShouldBeOfType<CustomizedAttributesShould4Attribute>()
                .Value
                .ShouldBe("derived");
        }
        
        [Fact]
        public void GivenAttributeOnType_WhenAttributeConversionUsesMagicParamName_ConversionStillApplies()
        {
            Spec.Attributes.RegisterAttributeConversion<CustomizedAttributesShould1Attribute>(_0 =>
                _0.Name != null && _0.Name.Contains("t") ?
                    (Attribute)new CustomizedAttributesShould1Attribute(_0.Name.Replace('t', 'q')) :
                    new CustomizedAttributesShould4Attribute(_0.Name));

            Act(nameof(CustomizedAttributesShouldModel.P1))
                .ShouldHaveSingleItem()
                .ShouldBeOfType<CustomizedAttributesShould1Attribute>()
                .Name
                .ShouldBe("qesq");
        }

        [Fact]
        public void GivenAttributeOnType_WhenAttributeConversionUsesOtherMagicParamName_ConversionStillApplies()
        {
            Spec.Attributes.RegisterAttributeConversion<CustomizedAttributesShould1Attribute>(_1 =>
                _1.Name != null && _1.Name.Contains("t") ?
                    (Attribute)new CustomizedAttributesShould1Attribute(_1.Name.Replace('t', 'q')) :
                    new CustomizedAttributesShould4Attribute(_1.Name));

            Act(nameof(CustomizedAttributesShouldModel.P1))
                .ShouldHaveSingleItem()
                .ShouldBeOfType<CustomizedAttributesShould1Attribute>()
                .Name
                .ShouldBe("qesq");
        }

        [Fact]
        public void GivenAttributeThatOnlyGoesOnProperties_WhenAttributeIsAddedToClass_ExceptionIsThrown()
        {
            Should.Throw<InvalidAttributeSpecificationException>(
                () => Spec.Attributes.AddAttribute(() => new CustomizedAttributesShould5Attribute()));
        }
    }
}
