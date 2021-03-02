using Shouldly;
using Xunit;

namespace NAdapter.Test
{
    public class CustomizedMethodFinderShouldModel
    {
        public int Sum(int x, int y)
        {
            return x + y;
        }
    }
    
    public class CustomizedMethodFinderShould: TestBase<CustomizedMethodFinderShouldModel>
    {
        [Fact]
        public void WhenFindingMethodBySignature_ReturnTypeIsIgnored()
        {
            Spec.SpecifyMethod()
                .WithFunctionSignature<int, int, string>(nameof(CustomizedMethodFinderShouldModel.Sum))
                .SpecifyLinq(c => c.Sum(Spec.Linq.Arg<int>(1), Spec.Linq.Arg<int>(2)).ToString());

            dynamic adapter = GetAdapter(new CustomizedMethodFinderShouldModel());
            string result = adapter.Sum(3, 5);

            result.ShouldBe("8");
        }

        [Fact]
        public void WhenFindingMethodBySignature_OverloadsAreAllowed()
        {
            Spec.SpecifyMethod()
                .WithFunctionSignature<int, int, int>("GetFirst")
                .SpecifyLinq(x => Spec.Linq.Arg<int>(1));

            Spec.SpecifyMethod()
                .WithFunctionSignature<int, int>("GetFirst")
                .SpecifyLinq(x => Spec.Linq.Arg<int>(1));

            dynamic adapter = GetAdapter(new CustomizedMethodFinderShouldModel());

            int three = adapter.GetFirst(3, 4);
            three.ShouldBe(3);
            int two = adapter.GetFirst(2);
            two.ShouldBe(2);
        }
    }
}
