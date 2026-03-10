# MAF API Cheatsheet

Quick syntax reference for `Microsoft.Agents.AI.*` — grounded in this project's codebase.

---

## Namespaces

```csharp
using Microsoft.Agents.AI;               // AIAgent, AIFunctionFactoryOptions
using Microsoft.Agents.AI.DevUI;         // AddDevUI, MapDevUI
using Microsoft.Agents.AI.Hosting;       // AddAIAgent, AddWorkflow
using Microsoft.Agents.AI.Hosting.OpenAI;// AddOpenAIResponses, AddOpenAIConversations
using Microsoft.Agents.AI.Workflows;     // WorkflowBuilder, Executor<,>, IWorkflowContext,
                                         // InProcessExecution, WorkflowOutputEvent,
                                         // RouteBuilder
using Microsoft.Extensions.AI;           // AIFunction, AIFunctionFactory
using OpenAI.Chat;                       // ChatClient, OpenAIChatClientExtensions.AsAIAgent
```

---

## AIAgent — Tool-Calling

```csharp
// Create
AIAgent agent = chatClient.AsAIAgent(
    instructions: "System prompt...",
    name:         "MyAgent",           // shown in DevUI
    description:  "What this agent does.",
    tools:        [tool1, tool2]);

// Create a tool from a static method
AIFunction tool = AIFunctionFactory.Create(MyTools.SearchBooks);

// Wrap agent as a tool (agent-as-a-tool)
AIFunction agentTool = innerAgent.AsAIFunction(
    options: new AIFunctionFactoryOptions
    {
        Name        = "tool_name",    // snake_case recommended
        Description = "Description visible to the outer agent LLM."
    });
```

---

## Executor<TIn, TOut> — Single-Handler Executor

```csharp
internal sealed class MyExecutor(IConfiguration config)
    : Executor<TInput, TOutput>("my-executor-id")
{
    public override async ValueTask<TOutput> HandleAsync(
        TInput input, IWorkflowContext context, CancellationToken ct = default)
    {
        // process input, return output
        return new TOutput(...);
    }
}
```

- Output is forwarded to **all** outgoing edges (`WorkflowBuilder.AddEdge`).

---

## Executor — Multi-Handler (Fan-In Aggregator)

```csharp
internal sealed class AggregatorExecutor(IConfiguration config)
    : Executor("aggregator-id")
{
    protected override RouteBuilder ConfigureRoutes(RouteBuilder rb) => rb
        .AddHandler<BranchResultA>(HandleAAsync)
        .AddHandler<BranchResultB>(HandleBAsync);

    private async ValueTask HandleAAsync(BranchResultA r, IWorkflowContext ctx, CancellationToken ct)
    {
        await ctx.QueueStateUpdateAsync("key-a", r, cancellationToken: ct);
        await TryEmitAsync(ctx, ct);
    }

    private async ValueTask HandleBAsync(BranchResultB r, IWorkflowContext ctx, CancellationToken ct)
    {
        await ctx.QueueStateUpdateAsync("key-b", r, cancellationToken: ct);
        await TryEmitAsync(ctx, ct);
    }

    private async ValueTask TryEmitAsync(IWorkflowContext ctx, CancellationToken ct)
    {
        var a = await ctx.ReadStateAsync<BranchResultA>("key-a", cancellationToken: ct);
        var b = await ctx.ReadStateAsync<BranchResultB>("key-b", cancellationToken: ct);
        if (a is null || b is null) return;  // branches not all arrived yet

        await ctx.YieldOutputAsync(new FinalOutput(a, b), ct);
    }
}
```

---

## IWorkflowContext

| Method | Purpose |
|--------|---------|
| `QueueStateUpdateAsync(key, value, ct)` | Safely store partial result (concurrency-safe) |
| `ReadStateAsync<T>(key, ct)` | Read stored value; returns `null` if not yet set |
| `YieldOutputAsync(value, ct)` | Emit the final workflow output (`WorkflowOutputEvent`) |

---

## WorkflowBuilder

```csharp
Workflow workflow = new WorkflowBuilder(entryExecutor)
    .AddEdge(from, to)          // directed edge: output of 'from' → input of 'to'
    .WithOutputFrom(executor)   // which executor's YieldOutputAsync is the workflow result
    .WithName("entry-executor-id")  // MUST match entry executor's id and AddWorkflow() name
    .Build();
```

---

## Program.cs — Registration Order

```csharp
var builder = WebApplication.CreateBuilder(args);

// 1. DevUI + OpenAI endpoints
builder.AddDevUI();
builder.AddOpenAIResponses();
builder.AddOpenAIConversations();

// 2. Create concrete instances (agents can be shared across registrations)
var myAgent    = MyAgentFactory.Create(config);
var myWorkflow = MyWorkflowFactory.Create(config);

// 3. Register with DI — name must match exactly
builder.AddAIAgent("MyAgent",    (_, _) => myAgent);
builder.AddWorkflow("entry-id",  (_, _) => myWorkflow);

var app = builder.Build();

// 4. Map HTTP endpoints
app.MapDevUI();
app.MapOpenAIResponses();
app.MapOpenAIConversations();

await app.RunAsync();
```

---

## InProcessExecution (Programmatic Run)

```csharp
await using var run = await InProcessExecution.RunAsync<TInput>(
    workflow,
    input: myInput,
    conversationId: Guid.NewGuid().ToString(),
    cancellationToken: ct);

var evt = run.OutgoingEvents
    .OfType<WorkflowOutputEvent>()
    .FirstOrDefault(e => e.Is<TOutput>());

if (evt?.Is<TOutput>(out var result) == true)
{
    // use result
}
```

---

## ChatClientFactory — OpenRouter (active) and Azure OpenAI

See [LLM Backends reference](./llm-backends.md) for full implementations of both.

**OpenRouter** (current):
```csharp
// Reads OpenRouter:ApiKey and OpenRouter:Model from IConfiguration
ChatClient client = ChatClientFactory.Create(config);
```

**Azure OpenAI** (alternative — requires `az login`):
```csharp
var cred   = new AzureCliCredential();
var client = new AzureOpenAIClient(new Uri(endpoint), cred)
                 .GetChatClient(deploymentName);
```

To switch backends, replace the body of `src/MVCAllOptions.AgentWorkflows/ChatClientFactory.cs`. No other files need to change — `ChatClient` is the same type in both paths.
