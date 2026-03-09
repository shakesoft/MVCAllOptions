using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Configuration;

namespace MVCAllOptions.AgentWorkflows.Workflows.BookReviewWorkflow;

/// <summary>
/// Builds the fan-out Book Review Workflow.
/// 
/// Graph topology:
/// 
///                  ┌─→ ContentReviewExecutor    ─┐
///                  │                              │
///   Dispatcher ────┼─→ PricingAnalysisExecutor  ─┼──→ ReviewAggregatorExecutor ──→ [output]
///                  │                              │
///                  └─→ GenreClassificationExecutor┘
///
/// The three branch executors run in parallel (fan-out).
/// The aggregator collects all three results, stores them in workflow state, and
/// calls YieldOutputAsync when all three arrive (fan-in).
/// WorkflowBuilder.WithOutputFrom marks the aggregator as the source of the
/// final WorkflowOutputEvent that callers can extract as BookReviewReport.
/// </summary>
public static class BookReviewWorkflowFactory
{
    public static Workflow Create(IConfiguration config)
    {
        var dispatcher = new BookReviewDispatcher();
        var content    = new ContentReviewExecutor(config);
        var pricing    = new PricingAnalysisExecutor(config);
        var genre      = new GenreClassificationExecutor(config);
        var aggregator = new ReviewAggregatorExecutor(config);

        return new WorkflowBuilder(dispatcher)
            .AddEdge(dispatcher, content)
            .AddEdge(dispatcher, pricing)
            .AddEdge(dispatcher, genre)
            .AddEdge(content,    aggregator)
            .AddEdge(pricing,    aggregator)
            .AddEdge(genre,      aggregator)
            .WithOutputFrom(aggregator)   // marks aggregator's YieldOutputAsync as the workflow result
            .WithName("book-review-dispatcher")
            .Build();
    }

    /// <summary>
    /// Convenience method: runs the workflow and prints the BookReviewReport.
    /// </summary>
    public static async Task RunAndPrintAsync(Workflow workflow, string bookName, CancellationToken ct = default)
    {
        Console.WriteLine($"\n{"═",60}");
        Console.WriteLine($" BOOK REVIEW WORKFLOW  →  {bookName}");
        Console.WriteLine($"{"═",60}");

        await using var run = await InProcessExecution.RunAsync<string>(workflow, bookName, Guid.NewGuid().ToString(), ct);

        // Extract the BookReviewReport from the workflow's output events
        var outputEvent = run.OutgoingEvents
            .OfType<WorkflowOutputEvent>()
            .FirstOrDefault(e => e.Is<BookReviewReport>());

        if (outputEvent is null || !outputEvent.Is<BookReviewReport>(out var report))
        {
            Console.WriteLine("  [!] No output received — check Azure OpenAI configuration.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"  Book : {report.BookName}");
        Console.WriteLine();
        Console.WriteLine("  ── Content Review ──");
        Console.WriteLine($"  Literary merit : {report.ContentReview.LiteraryMerit}");
        Console.WriteLine($"  Readability    : {report.ContentReview.ReadabilityScore}");
        Console.WriteLine($"  Audience       : {report.ContentReview.TargetAudience}");
        Console.WriteLine();
        Console.WriteLine("  ── Pricing Analysis ──");
        Console.WriteLine($"  Value          : {report.PricingAnalysis.ValueForMoney}");
        Console.WriteLine($"  Category       : {report.PricingAnalysis.PriceCategory}");
        Console.WriteLine($"  Market         : {report.PricingAnalysis.MarketComparison}");
        Console.WriteLine();
        Console.WriteLine("  ── Genre Classification ──");
        Console.WriteLine($"  Genre          : {report.GenreClassification.PrimaryGenre}");
        Console.WriteLine($"  Sub-genres     : {report.GenreClassification.SubGenres}");
        Console.WriteLine($"  Themes         : {report.GenreClassification.Themes}");
        Console.WriteLine();
        Console.WriteLine("  ── Combined Verdict ──");
        Console.WriteLine($"  {report.CombinedVerdict}");
        Console.WriteLine($"{"═",60}");
    }
}
