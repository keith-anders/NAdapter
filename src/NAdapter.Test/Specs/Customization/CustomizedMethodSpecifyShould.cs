using Shouldly;
using Xunit;
using System.Linq;

namespace NAdapter.Test
{
    public class CustomizedMethodSpecifyShouldModel
    {
        public string Run(string input) { return new string(input.Reverse().ToArray()); }
    }
    
    public class CustomizedMethodSpecifyShould : TestBase<CustomizedMethodSpecifyShouldModel>
    {
        const string NewMethodName = "NewName";
        const string ExistingMethodName = nameof(CustomizedMethodSpecifyShouldModel.Run);

        void TestMethodReturnsNull(string name, Behavior b)
        {
            var result = Spec.SpecifyMethod(behavior: b)
                .WithFunctionSignature<string, string>(name);

            result.ShouldBeNull();
        }

        void TestMethodThrows(string name, Behavior b)
        {
            Should.Throw<UnexpectedMemberFindBehaviorException>(() => Spec.SpecifyMethod(behavior: b).WithFunctionSignature<string, string>(name));
        }

        void TestMethodWorks(string name, Behavior b)
        {
            Spec.SpecifyMethod(behavior: b)
                .WithFunctionSignature<string, string>(name)
                .SpecifyLinq(x => Spec.Linq.Arg<string>(1) + " works");

            dynamic adapter = GetAdapter();

            switch (name)
            {
                case NewMethodName:
                    string newName = adapter.NewName("test");
                    newName.ShouldBe("test works");
                    break;
                case nameof(CustomizedMethodSpecifyShouldModel.Run):
                    string run = adapter.Run("test");
                    run.ShouldBe("test works");
                    break;
                default:
                    false.ShouldBeTrue("Unknown property name: " + name);
                    break;
            }
        }

        [Fact]
        public void WhenNewMethodIsAdded_ThenMethodAppearsOnAdapter()
        {
            TestMethodWorks(NewMethodName, Behavior.Add);
        }

        [Fact]
        public void WhenExistingMethodIsAdded_ThenNullIsReturned()
        {
            TestMethodReturnsNull(ExistingMethodName, Behavior.Add);
        }

        [Fact]
        public void WhenNewMethodIsAddOrThrown_ThenMethodAppearsOnAdapter()
        {
            TestMethodWorks(NewMethodName, Behavior.AddOrThrow);
        }

        [Fact]
        public void WhenExistingMethodIsAddOrThrown_ThenThrow()
        {
            TestMethodThrows(ExistingMethodName, Behavior.AddOrThrow);
        }

        [Fact]
        public void WhenExistingMethodIsGet_ThenMethodAppearsOnAdapter()
        {
            TestMethodWorks(ExistingMethodName, Behavior.Get);
        }

        [Fact]
        public void WhenNewMethodIsGet_ThenNullIsReturned()
        {
            TestMethodReturnsNull(NewMethodName, Behavior.Get);
        }

        [Fact]
        public void WhenNewMethodIsGetOrThrow_ThenThrow()
        {
            TestMethodThrows(NewMethodName, Behavior.GetOrThrow);
        }

        [Fact]
        public void WhenExistingMethodIsGetOrThrow_ThenMethodAppearsOnAdapter()
        {
            TestMethodWorks(ExistingMethodName, Behavior.GetOrThrow);
        }

        [Fact]
        public void WhenNewMethodIsAddOrGet_ThenMethodAppearsOnAdapter()
        {
            TestMethodWorks(NewMethodName, Behavior.AddOrGet);
        }
        
        [Fact]
        public void WhenExistingMethodIsAddOrGet_ThenMethodAppearsOnAdapter()
        {
            TestMethodWorks(ExistingMethodName, Behavior.AddOrGet);
        }
    }
}
