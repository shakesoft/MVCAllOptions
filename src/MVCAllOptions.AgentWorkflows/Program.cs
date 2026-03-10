using Microsoft.Agents.AI;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Hosting.OpenAI;
using Microsoft.Agents.AI.Workflows;
using MVCAllOptions.AgentWorkflows.Agents;
using MVCAllOptions.AgentWorkflows.Workflows.BookReviewWorkflow;
using MVCAllOptions.AgentWorkflows.Workflows.BookRecommendationWorkflow;
using MVCAllOptions.AgentWorkflows.Workflows.BookEnrichmentWorkflow;

// ──────────────────────────────────────────────────────────────────────────────
//  MVCAllOptions.AgentWorkflows
//  Microsoft Agent Framework + DevUI demo — Bookstore themed
// ──────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);
var config  = builder.Configuration;

// ── DevUI ─────────────────────────────────────────────────────────────────────
// Open http://localhost:5001/devui after startup to visualise agents & workflows.
builder.AddDevUI();

// ── Create agents eagerly so the recommender can nest the catalog agent ───────
var catalogAgent     = BookCatalogAgentFactory.Create(config);
var recommenderAgent = BookRecommenderAgentFactory.Create(config, catalogAgent);

// ── Register agents with named DI keys (required by HostedAgentResponseExecutor)
// AddAIAgent(name, factory) registers each agent under its name as a keyed service,
// enabling /v1/responses to resolve "entity_id" → agent instance.
builder.AddAIAgent("BookCatalogAgent",     (_, _) => catalogAgent);
builder.AddAIAgent("BookRecommenderAgent", (_, _) => recommenderAgent);

// ── Register workflows with named DI keys ────────────────────────────────────
// The name must match the workflow's internal Name (= first executor's name).
builder.AddWorkflow("book-review-dispatcher",  (_, _) => BookReviewWorkflowFactory.Create(config));
builder.AddWorkflow("preference-analyzer",     (_, _) => BookRecommendationWorkflowFactory.Create(config));
// BookEnrichmentWorkflow — fires when ABP bookstore creates a new book
builder.AddWorkflow("book-enrichment-entry",   (_, _) => BookEnrichmentWorkflowFactory.Create(config));

// ── Register OpenAI-compatible services (required by MapOpenAIResponses/Conversations)
builder.Services.AddOpenAIResponses();
builder.Services.AddOpenAIConversations();

// ──────────────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Map DevUI HTTP endpoints ──────────────────────────────────────────────────
//  /devui            — serves the React single-page application
app.MapDevUI();

// ── Map OpenAI-compatible endpoints (DevUI uses these to run agents/workflows) ─
//  POST /v1/responses       — streams AI responses (chat with agents/workflows)
//  GET+POST /v1/conversations — conversation history management
app.MapOpenAIResponses();
app.MapOpenAIConversations();

// ── Book Enrichment endpoint — called by ABP Web when a book is created ───────
// POST /api/book-enrichment
// Body: { "name": "...", "type": "...", "price": 0.0, "publishDate": "..." }
// Runs the BookEnrichmentWorkflow asynchronously and returns 202 Accepted.
app.MapPost("/api/book-enrichment", async (BookCreatedInput input, IServiceProvider sp) =>
{
    var enrichmentWorkflow = sp.GetKeyedService<Workflow>("book-enrichment-entry");

    if (enrichmentWorkflow is null)
        return Results.Problem("BookEnrichmentWorkflow not registered.");

    // Run the workflow in the background so the HTTP call returns immediately
    _ = Task.Run(async () =>
    {
        try
        {
            await BookEnrichmentWorkflowFactory.RunAndPrintAsync(enrichmentWorkflow, input);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BookEnrichment] Unhandled error: {ex.GetType().Name}: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    });

    return Results.Accepted();
}).WithName("BookEnrichment").WithTags("BookStore");

// ── Console demo ──────────────────────────────────────────────────────────────
_ = Task.Run(async () =>
{
    // Give ASP.NET Core a moment to fully start before writing demo output
    await Task.Delay(2000);

    try
    {
        Console.WriteLine("\n" + new string('═', 62));
        Console.WriteLine(" MVCAllOptions Agent Workflows — Demo");
        Console.WriteLine(" DevUI → http://localhost:5001/devui");
        Console.WriteLine(new string('═', 62));

        var workflows = app.Services.GetServices<Workflow>().ToList();
        // workflows[0] = BookReviewWorkflow (fan-out)
        // workflows[1] = BookRecommendationWorkflow (sequential)

        // ── Demo 1: Fan-out Book Review ───────────────────────────────────
        if (workflows.Count > 0)
        {
            Console.WriteLine("\n▶ Running BookReviewWorkflow (fan-out) for \"Dune\"...\n");
            await BookReviewWorkflowFactory.RunAndPrintAsync(workflows[0], "Dune");
        }

        // ── Demo 2: Sequential Book Recommendation ────────────────────────
        if (workflows.Count > 1)
        {
            Console.WriteLine("\n▶ Running BookRecommendationWorkflow (sequential)...\n");
            await BookRecommendationWorkflowFactory.RunAndPrintAsync(
                workflows[1],
                "I love science fiction and fantasy, something thought-provoking under $20");
        }

        Console.WriteLine("\n" + new string('═', 62));
        Console.WriteLine(" Demo complete. DevUI still running at http://localhost:5001/devui");
        Console.WriteLine(" Press Ctrl+C to stop.");
        Console.WriteLine(new string('═', 62));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n[Demo Error] {ex.GetType().Name}: {ex.Message}");
        Console.WriteLine("  Ensure Azure OpenAI is configured in appsettings.json and 'az login' has been run.");
    }
});

app.Run();
