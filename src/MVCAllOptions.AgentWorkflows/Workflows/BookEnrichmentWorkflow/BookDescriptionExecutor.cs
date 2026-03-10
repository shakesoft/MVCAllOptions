using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

namespace MVCAllOptions.AgentWorkflows.Workflows.BookEnrichmentWorkflow;

/// <summary>
/// Step 1 of 2 — Book Description Executor.
///
/// Receives basic book metadata and asks the LLM to generate a compelling
/// 2-3 sentence description a bookstore customer would see on the detail page.
///
/// Input:  <see cref="BookCreatedInput"/> (name, type, price, publishDate)
/// Output: <see cref="BookWithDescription"/> (same fields + generated description)
/// </summary>
internal sealed class BookDescriptionExecutor(IConfiguration config)
    : Executor<BookCreatedInput, BookWithDescription>("book-description")
{
    public override async ValueTask<BookWithDescription> HandleAsync(
        BookCreatedInput input,
        IWorkflowContext context,
        CancellationToken ct = default)
    {
        Console.WriteLine($"[BookDescriptionExecutor] Generating description for \"{input.Name}\" ({input.Type}, ${input.Price})...");

        try
        {
            var client = ChatClientFactory.Create(config);

            var prompt = $"""
                You are a professional book editor writing compelling product descriptions for an online bookstore.

                A new book has just been added to our catalogue with these details:
                  Title:        {input.Name}
                  Genre/Type:   {input.Type}
                  Price:        ${input.Price:F2}
                  Published:    {input.PublishDate}

                Write a compelling 2-3 sentence description that would appear on the book's product page.
                - Be engaging and accurate to the genre.
                - Do NOT start with "This book" or "The book".
                - Output ONLY the description, no labels or extra text.
                """;

            var response = await client.CompleteChatAsync(
                [new UserChatMessage(prompt)],
                cancellationToken: ct);

            var description = response.Value.Content[0].Text.Trim();

            Console.WriteLine($"[BookDescriptionExecutor] ✓ Description generated ({description.Length} chars).");

            return new BookWithDescription(
                input.Name,
                input.Type,
                input.Price,
                input.PublishDate,
                description);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BookDescriptionExecutor] ✗ ERROR: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }
}
