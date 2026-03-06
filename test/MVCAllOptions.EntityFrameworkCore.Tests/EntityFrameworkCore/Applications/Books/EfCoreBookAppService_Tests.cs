using MVCAllOptions.Books;
using Xunit;

namespace MVCAllOptions.EntityFrameworkCore.Applications.Books;

[Collection(MVCAllOptionsTestConsts.CollectionDefinitionName)]
public class EfCoreBookAppService_Tests : BookAppService_Tests<MVCAllOptionsEntityFrameworkCoreTestModule>
{

}