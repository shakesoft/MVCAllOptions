# MVCAllOptions.AgentWorkflows

A complete, runnable demo of **Microsoft Agent Framework + Workflows + DevUI** built around the MVCAllOptions Bookstore domain. Explores the full agentic stack: tool-calling agents, agent-as-a-tool composition, fan-out/fan-in workflows, sequential pipelines, and the DevUI visualiser.

---

## Prerequisites

| Requirement | Version |
|---|---|
| .NET SDK | 10.0+ |
| Azure OpenAI resource | any region |
| Azure CLI (`az login`) | for `AzureCliCredential` |

---

## Configuration

Edit `appsettings.json` (or set environment variables):

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://<your-resource>.openai.azure.com/",
    "DeploymentName": "gpt-4o-mini"
  }
}
```

Alternatively, set these environment variables before running:

```bash
export AZUREOPENAI__ENDPOINT="https://<your-resource>.openai.azure.com/"
export AZUREOPENAI__DEPLOYMENTNAME="gpt-4o-mini"
```

Sign in to Azure so `AzureCliCredential` can obtain a token:

```bash
az login
```

---

## Run

```bash
cd src/MVCAllOptions.AgentWorkflows
dotnet run
```

The app starts an ASP.NET Core host. A console demo runs in the background after startup.

Open the DevUI: [http://localhost:5000/devui](http://localhost:5000/devui)

---

## What Gets Demonstrated

### Agents

| Agent | Pattern | Description |
|---|---|---|
| `BookCatalogAgent` | Tool-calling | Uses 6 `AIFunctionFactory.Create()` tools to search and filter the in-memory catalogue |
| `BookRecommenderAgent` | Agent-as-a-tool | Wraps `BookCatalogAgent` via `catalogAgent.AsAIFunction()` — one agent calling another |

### Workflows

| Workflow | Pattern | Input → Output |
|---|---|---|
| `BookReviewWorkflow` | Fan-out / fan-in | `string bookName` → `BookReviewReport` (3 parallel LLM branches, aggregated) |
| `BookRecommendationWorkflow` | Sequential pipeline | `string preferences` → `RecommendationCard` (3 chained executors) |

---

## Architecture

### BookReviewWorkflow (fan-out)

```
                 ┌─→ ContentReviewExecutor     ─┐
                 │   (literary merit, audience)  │
BookReviewDispatcher ─→ PricingAnalysisExecutor ─┼──→ ReviewAggregatorExecutor ──→ [BookReviewReport]
                 │   (value, price category)     │
                 └─→ GenreClassificationExecutor─┘
                     (genre, sub-genres, themes)
```

Key patterns:
- `Executor<string, BookNameMessage>` — typed single-handler executor
- `Executor("id")` + `ConfigureRoutes(RouteBuilder)` — multi-handler executor (fan-in aggregator)
- `IWorkflowContext.QueueStateUpdateAsync` — accumulate partial results in state
- `IWorkflowContext.YieldOutputAsync` — emit final output when all branches arrive
- `WorkflowBuilder.WithOutputFrom(aggregator)` — marks the output executor

### BookRecommendationWorkflow (sequential)

```
[string preferences]
   → PreferenceAnalyzerExecutor   (LLM: free-text → StructuredPreferences)
   → BookFinderExecutor           (filter catalogue + LLM shortlist)
   → RecommendationFormatterExecutor (LLM: format final card)
   → [RecommendationCard]
```

Key patterns:
- `Executor<TIn, TOut>` chained via `WorkflowBuilder.AddEdge`
- Each executor's output type becomes the next executor's input type

---

## Key API Patterns

### Agent creation

```csharp
// Tool-calling agent
var chatClient = new AzureOpenAIClient(endpoint, credential).GetChatClient(deployment);

AIAgent catalogAgent = chatClient.AsAIAgent(
    instructions: "You are a bookstore assistant...",
    name:         "BookCatalogAgent",
    description:  "Searches the bookstore catalogue.",
    tools: [
        AIFunctionFactory.Create(BookstoreTools.SearchBooks),
        AIFunctionFactory.Create(BookstoreTools.GetBooksByGenre),
    ]);
```

### Agent-as-a-tool composition

```csharp
// Wrap inner agent as a tool for the outer agent
AIFunction catalogTool = catalogAgent.AsAIFunction(
    options: new AIFunctionFactoryOptions
    {
        Name        = "query_book_catalogue",
        Description = "Queries the bookstore catalogue.",
    });

AIAgent recommenderAgent = chatClient.AsAIAgent(
    instructions: "You are a book recommender...",
    name:         "BookRecommenderAgent",
    tools:        [catalogTool]);
```

### Executor (single handler)

```csharp
internal sealed class ContentReviewExecutor(IConfiguration config)
    : Executor<BookNameMessage, ContentReviewResult>
{
    public override async ValueTask<ContentReviewResult> HandleAsync(
        BookNameMessage message, IWorkflowContext context, CancellationToken ct)
    {
        // call LLM, return typed result
    }
}
```

### Executor (multi-handler / fan-in)

```csharp
internal sealed class ReviewAggregatorExecutor(IConfiguration config)
    : Executor("review-aggregator")
{
    protected override RouteBuilder ConfigureRoutes(RouteBuilder routeBuilder) =>
        routeBuilder
            .AddHandler<ContentReviewResult>(HandleContentAsync)
            .AddHandler<PricingAnalysisResult>(HandlePricingAsync)
            .AddHandler<GenreClassificationResult>(HandleGenreAsync);

    private async ValueTask HandleContentAsync(
        ContentReviewResult result, IWorkflowContext ctx, CancellationToken ct)
    {
        await ctx.QueueStateUpdateAsync(ContentKey, result, cancellationToken: ct);
        await TryEmitAsync(ctx, ct);
    }

    private async ValueTask TryEmitAsync(IWorkflowContext ctx, CancellationToken ct)
    {
        var content = await ctx.ReadStateAsync<ContentReviewResult>(ContentKey, cancellationToken: ct);
        var pricing = await ctx.ReadStateAsync<PricingAnalysisResult>(PricingKey, cancellationToken: ct);
        var genre   = await ctx.ReadStateAsync<GenreClassificationResult>(GenreKey, cancellationToken: ct);
        if (content is null || pricing is null || genre is null) return; // wait for all branches
        await ctx.YieldOutputAsync(new BookReviewReport(...), ct);
    }
}
```

### Workflow construction and execution

```csharp
// Build
Workflow workflow = new WorkflowBuilder(dispatcher)
    .AddEdge(dispatcher, contentReviewer)
    .AddEdge(dispatcher, pricingAnalyzer)
    .AddEdge(dispatcher, genreClassifier)
    .AddEdge(contentReviewer,  aggregator)
    .AddEdge(pricingAnalyzer,  aggregator)
    .AddEdge(genreClassifier,  aggregator)
    .WithOutputFrom(aggregator)   // marks final output executor
    .Build();

// Run and extract typed result
await using var run = await InProcessExecution.RunAsync<string>(
    workflow, "Dune", runId: Guid.NewGuid().ToString(), ct);

var outputEvent = run.OutgoingEvents
    .OfType<WorkflowOutputEvent>()
    .FirstOrDefault(e => e.Is<BookReviewReport>());

if (outputEvent?.Is<BookReviewReport>(out var report) == true)
    Console.WriteLine(report.CombinedVerdict);
```

### DevUI setup

```csharp
// In Program.cs / builder setup
builder.AddDevUI();
builder.Services.AddSingleton<AIAgent>(catalogAgent);
builder.Services.AddSingleton<AIAgent>(recommenderAgent);
builder.Services.AddSingleton<Workflow>(_ => BookReviewWorkflowFactory.Create(config));
builder.Services.AddSingleton<Workflow>(_ => BookRecommendationWorkflowFactory.Create(config));

// Map endpoints
app.MapDevUI();      // SPA at /devui
app.MapEntities();   // /v1/entities — agent & workflow discovery
app.MapMeta();       // /meta — environment metadata
```

---

## Project Structure

```
MVCAllOptions.AgentWorkflows/
├── appsettings.json
├── MVCAllOptions.AgentWorkflows.csproj
├── Program.cs                                    ← ASP.NET Core host + DevUI + demo runner
│
├── Data/
│   └── BookstoreData.cs                          ← In-memory catalogue (12 books)
│
├── Tools/
│   └── BookstoreTools.cs                         ← Static methods → AIFunctionFactory.Create()
│
├── Agents/
│   ├── BookCatalogAgentFactory.cs                ← Tool-calling agent
│   └── BookRecommenderAgentFactory.cs            ← Agent-as-a-tool pattern
│
└── Workflows/
    ├── BookReviewWorkflow/
    │   ├── BookReviewMessages.cs                 ← Record types (messages / results)
    │   ├── BookReviewDispatcher.cs               ← Fan-out dispatcher
    │   ├── ContentReviewExecutor.cs              ← Branch 1: content analysis
    │   ├── PricingAnalysisExecutor.cs            ← Branch 2: pricing analysis
    │   ├── GenreClassificationExecutor.cs        ← Branch 3: genre classification
    │   ├── ReviewAggregatorExecutor.cs           ← Fan-in aggregator (multi-handler)
    │   └── BookReviewWorkflowFactory.cs          ← DAG construction + RunAndPrintAsync
    │
    └── BookRecommendationWorkflow/
        ├── BookRecommendationMessages.cs         ← Record types
        ├── PreferenceAnalyzerExecutor.cs         ← Step 1: parse user preferences
        ├── BookFinderExecutor.cs                 ← Step 2: filter catalogue & shortlist
        ├── RecommendationFormatterExecutor.cs    ← Step 3: format recommendation card
        └── BookRecommendationWorkflowFactory.cs  ← Pipeline construction + RunAndPrintAsync
```

---

## Packages Used

| Package | Version |
|---|---|
| `Microsoft.Agents.AI` | 1.0.0-preview.260121.1 |
| `Microsoft.Agents.AI.OpenAI` | 1.0.0-preview.260121.1 |
| `Microsoft.Agents.AI.Workflows` | 1.0.0-preview.260121.1 |
| `Microsoft.Agents.AI.DevUI` | 1.0.0-preview.260121.1 |
| `Azure.AI.OpenAI` | 2.2.0-beta.4 |
| `Azure.Identity` | 1.14.0 |
