using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace MVCAllOptions.Pages;

[Collection(MVCAllOptionsTestConsts.CollectionDefinitionName)]
public class Index_Tests : MVCAllOptionsWebTestBase
{
    [Fact]
    public async Task Welcome_Page()
    {
        var response = await GetResponseAsStringAsync("/");
        response.ShouldNotBeNull();
    }
}
