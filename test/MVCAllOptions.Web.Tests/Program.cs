using Microsoft.AspNetCore.Builder;
using MVCAllOptions;
using Volo.Abp.AspNetCore.TestBase;

var builder = WebApplication.CreateBuilder();
builder.Environment.ContentRootPath = GetWebProjectContentRootPathHelper.Get("MVCAllOptions.Web.csproj"); 
await builder.RunAbpModuleAsync<MVCAllOptionsWebTestModule>(applicationName: "MVCAllOptions.Web");

public partial class Program
{
}
