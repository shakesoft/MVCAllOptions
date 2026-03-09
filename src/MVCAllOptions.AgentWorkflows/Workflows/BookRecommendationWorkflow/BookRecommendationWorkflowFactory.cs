using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Configuration;

namespace MVCAllOptions.AgentWorkflows.Workflows.BookRecommendationWorkflow;

/// <summary>
/// Builds the sequential Book Recommendation Workflow.
///
/// Pipeline:
///   [string] → PreferenceAnalyzer → BookFinder → RecommendationFormatter → [RecommendationCard]
///
/// Each executor's output becomes the next executor's input (linear chain).
/// WithOutputFrom(recommendationFormatter) marks the last step's result as the workflow output.
/// </summary>
public static class BookRecommendationWorkflowFactory
{
    public static Workflow Create(IConfiguration config)
    {
        var preferenceAnalyzer      = new PreferenceAnalyzerExecutor(config);
        var bookFinder              = new BookFinderExecutor(config);
        var recommendationFormatter = new RecommendationFormatterExecutor(config);

        return new WorkflowBuilder(preferenceAnalyzer)
            .AddEdge(preferenceAnalyzer,      bookFinder)
            .AddEdge(bookFinder,              recommendationFormatter)
            .WithOutputFrom(recommendationFormatter)
            .WithName("preference-analyzer")
            .Build();
    }

    public static async Task RunAndPrintAsync(Workflow workflow, string userPreferences, CancellationToken ct = default)
    {
        Console.WriteLine($"\n{"═",60}");
        Console.WriteLine($" BOOK RECOMMENDATION WORKFLOW");
        Console.WriteLine($" Input: \"{userPreferences}\"");
        Console.WriteLine($"{"═",60}");

        await using var run = await InProcessExecution.RunAsync<string>(workflow, userPreferences, Guid.NewGuid().ToString(), ct);

        var outputEvent = run.OutgoingEvents
            .OfType<WorkflowOutputEvent>()
            .FirstOrDefault(e => e.Is<RecommendationCard>());

        if (outputEvent is null || !outputEvent.Is<RecommendationCard>(out var card))
        {
            Console.WriteLine("  [!] No output received — check Azure OpenAI configuration.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"  Your preferences: {card.UserPreferenceSummary}");
        Console.WriteLine();

        int i = 1;
        foreach (var rec in card.Recommendations)
        {
            Console.WriteLine($"  [{i++}] {rec.Title}");
            Console.WriteLine($"      by {rec.Author}  |  {rec.Genre}  |  {rec.Price}");
            Console.WriteLine($"      → {rec.WhyItFits}");
            Console.WriteLine();
        }

        Console.WriteLine($"  {card.ClosingNote}");
        Console.WriteLine($"{"═",60}");
    }
}
