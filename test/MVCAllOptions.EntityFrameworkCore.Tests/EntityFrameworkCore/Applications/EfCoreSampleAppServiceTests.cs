using MVCAllOptions.Samples;
using Xunit;

namespace MVCAllOptions.EntityFrameworkCore.Applications;

[Collection(MVCAllOptionsTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<MVCAllOptionsEntityFrameworkCoreTestModule>
{

}
