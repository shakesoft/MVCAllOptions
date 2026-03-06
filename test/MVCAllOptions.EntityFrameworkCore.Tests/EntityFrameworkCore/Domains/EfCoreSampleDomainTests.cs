using MVCAllOptions.Samples;
using Xunit;

namespace MVCAllOptions.EntityFrameworkCore.Domains;

[Collection(MVCAllOptionsTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<MVCAllOptionsEntityFrameworkCoreTestModule>
{

}
