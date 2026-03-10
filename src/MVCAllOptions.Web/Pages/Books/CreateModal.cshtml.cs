using System.Threading.Tasks;
using MVCAllOptions.Books;
using MVCAllOptions.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace MVCAllOptions.Web.Pages.Books
{
    public class CreateModalModel : MVCAllOptionsPageModel
    {
        [BindProperty]
        public CreateUpdateBookDto Book { get; set; }

        private readonly IBookAppService _bookAppService;
        private readonly BookEnrichmentApiClient _enrichmentClient;

        public CreateModalModel(
            IBookAppService bookAppService,
            BookEnrichmentApiClient enrichmentClient)
        {
            _bookAppService    = bookAppService;
            _enrichmentClient  = enrichmentClient;
        }

        public void OnGet()
        {
            Book = new CreateUpdateBookDto();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var book = await _bookAppService.CreateAsync(Book);

            // Fire-and-forget: trigger the Book Enrichment Workflow in AgentWorkflows.
            // This is non-blocking — the AI result is printed in the AgentWorkflows terminal.
            _ = _enrichmentClient.NotifyBookCreatedAsync(
                name:        book.Name,
                type:        book.Type.ToString(),
                price:       book.Price,
                publishDate: book.PublishDate.ToString("yyyy-MM-dd"));

            return NoContent();
        }
    }
}