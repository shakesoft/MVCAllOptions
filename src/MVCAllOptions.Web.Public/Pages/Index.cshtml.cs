using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace MVCAllOptions.Web.Public.Pages;

public class IndexModel : MVCAllOptionsPublicPageModel
{
    public void OnGet()
    {

    }

    public async Task OnPostLoginAsync()
    {
        await HttpContext.ChallengeAsync("oidc");
    }
}
