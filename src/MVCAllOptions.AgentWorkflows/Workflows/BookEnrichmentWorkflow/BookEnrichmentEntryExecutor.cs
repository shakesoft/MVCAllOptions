using System.Text.Json;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

namespace MVCAllOptions.AgentWorkflows.Workflows.BookEnrichmentWorkflow;

/// <summary>
/// Entry point for the Book Enrichment Workflow.
///
/// Accepts a <c>string</c> so that:
/// <list type="bullet">
///   <item>DevUI (which always sends plain text through /v1/responses) can trigger the workflow.</item>
///   <item>The programmatic /api/book-enrichment endpoint can pass a JSON-serialised <see cref="BookCreatedInput"/>.</item>
/// </list>
/// Strategy:
/// 1. Try to deserialise a JSON string (fast path — used by the API endpoint).
/// 2. If the input is not valid JSON, call the LLM to extract book metadata
///    from natural language (DevUI path, e.g. "Tell me about a sci-fi book called Nova").
///
/// Input:  <c>string</c>
/// Output: <see cref="BookCreatedInput"/> — passed to <see cref="BookDescriptionExecutor"/>
/// </summary>
internal sealed class BookEnrichmentEntryExecutor(IConfiguration config)
    : Executor<string, BookCreatedInput>("book-enrichment-entry")
{
    private static readonly JsonSerializerOptions _jsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    public override async ValueTask<BookCreatedInput> HandleAsync(
        string input,
        IWorkflowContext context,
        CancellationToken ct = default)
    {
        Console.WriteLine("[BookEnrichmentEntry] Received input, parsing book metadata...");

        // ── Fast path: JSON from the programmatic API ──────────────────────
        if (input.TrimStart().StartsWith('{'))
        {
            try
            {
                var parsed = JsonSerializer.Deserialize<BookCreatedInput>(input, _jsonOpts);
                if (parsed is not null)
                {
                    Console.WriteLine($"[BookEnrichmentEntry] Parsed from JSON → \"{parsed.Name}\" ({parsed.Type}, ${parsed.Price:F2})");
                    return parsed;
                }
            }
            catch
            {
                // Not valid JSON — fall through to LLM parsing
            }
        }

        // ── LLM path: extract metadata from DevUI natural-language input ───
        Console.WriteLine("[BookEnrichmentEntry] Using LLM to extract book metadata from natural language...");

        var client = ChatClientFactory.Create(config);

        var prompt = $"""
            Extract book metadata from the following text and respond with ONLY a JSON object using these exact fields:
            - "Name"        (string)  : book title
            - "Type"        (string)  : genre or book type
            - "Price"       (number)  : price in USD (use 0 if not mentioned)
            - "PublishDate" (string)  : publish date or year (use "Unknown" if not mentioned)

            Rules:
            - Output ONLY the JSON object — no markdown fences, no explanation.
            - If a field cannot be determined, use a sensible default ("Unknown" for strings, 0 for numbers).

            Text: {input}
            """;

        var response = await client.CompleteChatAsync(
            [new UserChatMessage(prompt)],
            cancellationToken: ct);

        var json = response.Value.Content[0].Text.Trim();

        // Strip markdown code fences if the LLM wraps in ```json ... ```
        if (json.StartsWith("```"))
            json = string.Join('\n', json.Split('\n').Skip(1).SkipLast(1)).Trim();

        try
        {
            var result = JsonSerializer.Deserialize<BookCreatedInput>(json, _jsonOpts);
            if (result is not null)
            {
                Console.WriteLine($"[BookEnrichmentEntry] LLM extracted → \"{result.Name}\" ({result.Type}, ${result.Price:F2})");
                return result;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BookEnrichmentEntry] ✗ Failed to deserialise LLM response: {ex.Message}");
            Console.WriteLine($"[BookEnrichmentEntry]   Raw LLM output: {json}");
        }

        // Final fallback — return a placeholder so the pipeline can continue
        var fallback = new BookCreatedInput("Unknown Book", "General", 0f, "Unknown");
        Console.WriteLine($"[BookEnrichmentEntry] Using fallback input: {fallback}");
        return fallback;
    }
}
