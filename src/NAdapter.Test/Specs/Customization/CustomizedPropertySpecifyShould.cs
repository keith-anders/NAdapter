using Shouldly;
using Xunit;

namespace NAdapter.Test
{
    public class CustomizedPropertySpecifyShouldModel
    {
        public string String { get; set; }
    }

    public class CustomizedPropertySpecifyShould : TestBase<CustomizedPropertySpecifyShouldModel>
    {
        void TestPropertyReturnsNull(string name, Behavior b)
        {
            Spec.SpecifyProperty(name, behavior: b).ShouldBeNull();
        }

        void TestPropertyThrows(string name, Behavior b)
        {
            Should.Throw<UnexpectedMemberFindBehaviorException>(() => Spec.SpecifyProperty(name, behavior: b));
        }

        void TestPropertyWorks(string name, Behavior b)
        {
            Spec.SpecifyProperty(name, behavior: b)
                .SpecifyAutoImplemented<string>();

            dynamic adapter = GetAdapter();

            switch (name)
            {
                case "NewProperty":
                    object value = adapter.NewProperty;
                    value.ShouldBeNull();
                    break;
                case nameof(CustomizedPropertySpecifyShouldModel.String):
                    string result = adapter.String;
                    result.ShouldBeNull();
                    break;
                default:
                    false.ShouldBeTrue("Unknown property name");
                    break;
            }
        }

        [Fact]
        public void WhenPropertyIsSpecifiedPrivate_ThenAdapterPropertyIsPrivate()
        {
            Spec.SpecifyProperty(m => m.String, Access.Private, Behavior.AddOrGet);

            dynamic adapter = GetAdapter(new CustomizedPropertySpecifyShouldModel());

            AssertThrowsProtectionLevel(() =>
            {
                string tmp = adapter.String;
            });
        }

        [Fact]
        public void WhenNewPropertyIsAdd_ThenPropertyExistsOnAdapter()
        {
            TestPropertyWorks("NewProperty", Behavior.Add);
        }

        [Fact]
        public void WhenExistingPropertyIsAdd_ThenReturnsNull()
        {
            TestPropertyReturnsNull(nameof(CustomizedPropertySpecifyShouldModel.String), Behavior.Add);
        }

        [Fact]
        public void WhenExistingPropertyIsGet_ThenPropertyExistsOnAdapter()
        {
            TestPropertyWorks(nameof(CustomizedPropertySpecifyShouldModel.String), Behavior.Get);
        }

        [Fact]
        public void WhenNewPropertyIsGet_ThenReturnsNull()
        {
            TestPropertyReturnsNull("NewProperty", Behavior.Get);
        }

        [Fact]
        public void WhenNewPropertyIsAddOrThrow_ThenPropertyExistsOnAdapter()
        {
            TestPropertyWorks("NewProperty", Behavior.AddOrThrow);
        }

        [Fact]
        public void WhenExistingPropertyIsAddOrThrow_ThenThrows()
        {
            TestPropertyThrows(nameof(CustomizedPropertySpecifyShouldModel.String), Behavior.AddOrThrow);
        }

        [Fact]
        public void WhenExistingPropertyIsGetOrThrow_ThenPropertyExistsOnAdapter()
        {
            TestPropertyWorks(nameof(CustomizedPropertySpecifyShouldModel.String), Behavior.GetOrThrow);
        }

        [Fact]
        public void WhenNewPropertyIsGetOrThrow_ThenThrows()
        {
            TestPropertyThrows("NewProperty", Behavior.GetOrThrow);
        }

        [Fact]
        public void WhenPropertyIsOnAdapter_ThenPropertyCanBeAccessedByReflection()
        {
            var adapter = GetAdapter();

            adapter.GetType()
                .GetProperty(nameof(CustomizedPropertySpecifyShouldModel.String))
                .ShouldNotBeNull();
        }
    }
}
