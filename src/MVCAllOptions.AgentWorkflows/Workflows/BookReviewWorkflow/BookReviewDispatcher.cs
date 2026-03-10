using Microsoft.Agents.AI.Workflows;
using MVCAllOptions.AgentWorkflows.Data;

namespace MVCAllOptions.AgentWorkflows.Workflows.BookReviewWorkflow;

/// <summary>
/// Entry executor: validates the book name and dispatches it to all three review branches.
/// Pattern: Executor&lt;TIn, TOut&gt; — one input, one output type that fans out to multiple edges.
/// </summary>
internal sealed class BookReviewDispatcher() : Executor<string, BookNameMessage>("book-review-dispatcher")
{
    public override ValueTask<BookNameMessage> HandleAsync(string bookName, IWorkflowContext context, CancellationToken ct = default)
    {
        bookName = bookName.Trim();

        var book = BookstoreData.GetByNameExact(bookName)
            ?? BookstoreData.SearchByName(bookName).FirstOrDefault();

        if (book is null)
            throw new ArgumentException($"Book '{bookName}' not found in the catalogue.");

        Console.WriteLine($"[BookReviewDispatcher] Fanning out review for: {book.Name}");

        // The same BookNameMessage is sent along ALL outgoing edges simultaneously (fan-out)
        return ValueTask.FromResult(new BookNameMessage(book.Name));
    }
}
