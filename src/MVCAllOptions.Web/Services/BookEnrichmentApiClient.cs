using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace MVCAllOptions.Web.Services;

/// <summary>
/// Calls the AgentWorkflows service to trigger the Book Enrichment Workflow
/// when a new book is created in the ABP bookstore.
///
/// Registered as transient via <see cref="ITransientDependency"/>.
/// The AgentWorkflows service is expected to run on http://localhost:5000.
/// </summary>
public class BookEnrichmentApiClient : ITransientDependency
{
    private readonly IHttpClientFactory _httpClientFactory;

    // Configurable via appsettings if needed; hardcoded here for simplicity.
    private const string AgentWorkflowsBaseUrl = "http://localhost:5001";

    public BookEnrichmentApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Fires a POST to /api/book-enrichment in the AgentWorkflows service.
    /// Returns immediately after the HTTP call (the workflow runs asynchronously
    /// on the AgentWorkflows side and prints results to its console).
    /// Swallows connection errors so book creation is never blocked.
    /// </summary>
    public async Task NotifyBookCreatedAsync(
        string name,
        string type,
        float  price,
        string publishDate)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();

            var payload = JsonSerializer.Serialize(new
            {
                name,
                type,
                price,
                publishDate
            });

            using var content = new StringContent(payload, Encoding.UTF8, "application/json");
            await client.PostAsync($"{AgentWorkflowsBaseUrl}/api/book-enrichment", content);
        }
        catch
        {
            // AgentWorkflows is an optional companion service.
            // A connection failure must never break book creation.
        }
    }
}
