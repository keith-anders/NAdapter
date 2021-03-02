using Shouldly;
using Xunit;

namespace NAdapter.Test
{
    public class ByDefaultAdapterShouldModel { public string String { get; set; } }
    
    public class ByDefaultAdapterShould: TestBase<ByDefaultAdapterShouldModel>
    {
        [Fact]
        public void GivenNoSetUp_WhenAdapterIsMade_AdapterSourceIsGivenComponent()
        {
            ByDefaultAdapterShouldModel source = new ByDefaultAdapterShouldModel();

            GetAdapter(source).Source.ShouldBeSameAs(source);
        }

        [Fact]
        public void GivenNoSetUp_WhenAdapterComponentIsSet_AdapterPropretiesUseNewComponent()
        {
            ByDefaultAdapterShouldModel source1 = new ByDefaultAdapterShouldModel();
            ByDefaultAdapterShouldModel source2 = new ByDefaultAdapterShouldModel();
            var adapter = GetAdapter(source1);
            dynamic dynamicAdapter = adapter;

            source1.String = "test1";
            source2.String = "test2";

            string result = dynamicAdapter.String;
            result.ShouldBe("test1");
            adapter.Source = source2;
            result = dynamicAdapter.String;
            result.ShouldBe("test2");
        }
    }
}
