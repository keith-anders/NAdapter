using Shouldly;
using Xunit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NAdapter.Test
{
    // Note: this example taken from https://msdn.microsoft.com/en-us/library/system.reflection.customattributedata(v=vs.110).aspx
    // as an example of a complex attribute initialization

    public enum ExampleKind
    {
        FirstKind,
        SecondKind,
        ThirdKind,
        FourthKind
    };
    
    public class ByDefaultAttributesShould1Attribute : Attribute
    {
        // Data for properties.
        private ExampleKind kindValue;
        private string noteValue;
        private string[] arrayStrings;
        private int[] arrayNumbers;

        public ByDefaultAttributesShould1Attribute(ExampleKind initKind, string[] initStrings)
        {
            kindValue = initKind;
            arrayStrings = initStrings;
        }
        public ByDefaultAttributesShould1Attribute(ExampleKind initKind) : this(initKind, null) { }
        public ByDefaultAttributesShould1Attribute() : this(ExampleKind.FirstKind, null) { }

        // Properties. The Note and Numbers properties must be read/write, so they
        // can be used as named parameters.
        //
        public ExampleKind Kind { get { return kindValue; } }
        public string[] Strings { get { return arrayStrings; } }
        public string Note
        {
            get { return noteValue; }
            set { noteValue = value; }
        }
        public int[] Numbers
        {
            get { return arrayNumbers; }
            set { arrayNumbers = value; }
        }
    }

    public class ByDefaultAttributesShould2Attribute : Attribute
    {
        Array _array;

        public IEnumerable Args { get { return _array; } }

        public ByDefaultAttributesShould2Attribute(object[] array)
        {
            _array = array;
        }
    }

    public class ByDefaultAttributesShouldModel
    {
        [ByDefaultAttributesShould1(ExampleKind.SecondKind,
        new string[] { "String array argument, line 1",
                                "String array argument, line 2",
                                "String array argument, line 3" },
                        Note = "This is a note on the property.",
                        Numbers = new int[] { 53, 57, 59 })]
        public string P1 { get; set; }

        [ByDefaultAttributesShould2(new object[]
            {
                typeof(string),
                32,
                "test",
                new string[] { "test2", "test3" },
                null
            })]
        public string P2 { get; set; }

        [ByDefaultAttributesShould2(null)]
        public string P3 { get; set; }
    }
    
    public class ByDefaultAttributesShould: TestBase<ByDefaultAttributesShouldModel>
    {
        [Fact]
        public void GivenComplexAttributeInitialization_WhenAttributeIsInitialized_InitializationIsCarriedOver()
        {
            var propAttribute = GetAdapter(null).GetType().GetProperty(nameof(ByDefaultAttributesShouldModel.P1)).GetCustomAttributes(true).Single() as ByDefaultAttributesShould1Attribute;

            propAttribute.ShouldSatisfyAllConditions(
                () => propAttribute.Kind.ShouldBe(ExampleKind.SecondKind),
                () => propAttribute.Note.ShouldBe("This is a note on the property."),
                () => propAttribute.Numbers.SequenceEqual(new[] { 53, 57, 59 }).ShouldBeTrue(),
                () => propAttribute.Strings.SequenceEqual(new[]
                    {
                        "String array argument, line 1",
                        "String array argument, line 2",
                        "String array argument, line 3"
                    })
                );
        }

        [Fact]
        public void GivenMixedTypeObjectArrayInAttributeInitialization_WhenAttributeIsInitialized_InitializationIsCarriedOver()
        {
            var propAttribute = GetAdapter(null).GetType().GetProperty(nameof(ByDefaultAttributesShouldModel.P2)).GetCustomAttributes(true).Single() as ByDefaultAttributesShould2Attribute;
            
            List<object> recoveredList = new List<object>();
            foreach (object o in propAttribute.Args)
                recoveredList.Add(o);

            recoveredList[0].ShouldBe(typeof(String));

            recoveredList[1].ShouldBe(32);

            recoveredList[2].ShouldBe("test");

            var recovered3 = recoveredList[3].ShouldBeOfType<string[]>();

            recovered3.Length.ShouldBe(2);
            recovered3.SequenceEqual(new[] { "test2", "test3" }).ShouldBeTrue();

            recoveredList[4].ShouldBe(null);
            
            recoveredList.Count.ShouldBe(5);
        }
    }
}
