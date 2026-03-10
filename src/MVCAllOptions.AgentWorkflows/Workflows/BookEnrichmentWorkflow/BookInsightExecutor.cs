using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

namespace MVCAllOptions.AgentWorkflows.Workflows.BookEnrichmentWorkflow;

/// <summary>
/// Step 2 of 2 — Book Insight Executor.
///
/// Receives the book with its AI description and generates deeper insights:
/// who should read it, key themes, and a punchy one-liner marketing blurb.
///
/// Input:  <see cref="BookWithDescription"/>
/// Output: <see cref="BookEnrichmentResult"/> (the workflow's final output)
/// </summary>
internal sealed class BookInsightExecutor(IConfiguration config)
    : Executor<BookWithDescription, BookEnrichmentResult>("book-insight")
{
    public override async ValueTask<BookEnrichmentResult> HandleAsync(
        BookWithDescription input,
        IWorkflowContext context,
        CancellationToken ct = default)
    {
        Console.WriteLine($"[BookInsightExecutor] Generating insights for \"{input.Name}\"...");

        try
        {
            var client = ChatClientFactory.Create(config);

        var prompt = $"""
            You are a marketing analyst for a bookstore. Analyse this book and return three things.

            Book details:
              Title:       {input.Name}
              Genre:       {input.Type}
              Price:       ${input.Price:F2}
              Description: {input.Description}

            Return ONLY the following three lines, nothing else:
            TARGET_AUDIENCE: <one sentence describing who this book is for>
            KEY_THEMES: <3-5 comma-separated themes>
            MARKETING_BLURB: <a single punchy marketing line, max 12 words>
            """;

        var response = await client.CompleteChatAsync(
            [new UserChatMessage(prompt)],
            cancellationToken: ct);

        var raw  = response.Value.Content[0].Text.Trim();
        var audience = ExtractLine(raw, "TARGET_AUDIENCE:");
        var themes   = ExtractLine(raw, "KEY_THEMES:");
        var blurb    = ExtractLine(raw, "MARKETING_BLURB:");

        Console.WriteLine($"[BookInsightExecutor] ✓ Insights generated.");

        return new BookEnrichmentResult(
            BookName:        input.Name,
            Description:     input.Description,
            TargetAudience:  audience,
            KeyThemes:       themes,
            MarketingBlurb:  blurb);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BookInsightExecutor] ✗ ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    private static string ExtractLine(string text, string label)
    {
        foreach (var line in text.Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith(label, StringComparison.OrdinalIgnoreCase))
                return trimmed[label.Length..].Trim();
        }
        return string.Empty;
    }
}
