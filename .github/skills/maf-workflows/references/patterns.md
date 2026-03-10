# MAF Patterns Reference

Full code skeletons for every major MAF pattern used in this project.

---

## Pattern 1 — Tool-Calling Agent

```
User → Agent → [picks tool] → Tool method → Agent → Response
```

```csharp
// Tools/MyTools.cs
public static class MyTools
{
    [Description("Searches items by keyword")]
    public static List<Item> Search(
        [Description("Search term")] string query) =>
        ItemDatabase.Search(query);
}

// Agents/MyAgentFactory.cs
public static class MyAgentFactory
{
    public static AIAgent Create(IConfiguration config)
    {
        var chatClient = ChatClientFactory.Create(config);

        return chatClient.AsAIAgent(
            instructions: "You help users find items...",
            name:         "MyAgent",
            description:  "Searches and retrieves item data.",
            tools: [
                AIFunctionFactory.Create(MyTools.Search),
            ]);
    }
}
```

**Registration:**
```csharp
builder.AddAIAgent("MyAgent", (_, _) => MyAgentFactory.Create(config));
```

---

## Pattern 2 — Agent-as-a-Tool (Nested Agent)

```
User → OuterAgent → [calls inner agent as tool] → InnerAgent → [calls real tools] → Response
```

```csharp
public static class OuterAgentFactory
{
    public static AIAgent Create(IConfiguration config, AIAgent innerAgent)
    {
        var chatClient = ChatClientFactory.Create(config);

        // Wrap inner agent as an AIFunction
        AIFunction innerTool = innerAgent.AsAIFunction(
            options: new AIFunctionFactoryOptions
            {
                Name        = "query_inner_agent",
                Description = "Delegates complex search to the inner agent.",
            });

        return chatClient.AsAIAgent(
            instructions: "You are a planner. Use query_inner_agent to get data.",
            name:         "OuterAgent",
            description:  "High-level orchestrator that nests InnerAgent.",
            tools:        [innerTool]);
    }
}
```

**Registration (inner first):**
```csharp
var inner = InnerAgentFactory.Create(config);
var outer = OuterAgentFactory.Create(config, inner);
builder.AddAIAgent("InnerAgent", (_, _) => inner);
builder.AddAIAgent("OuterAgent", (_, _) => outer);
```

---

## Pattern 3 — Sequential Pipeline

```
Input → StepA → StepB → StepC → Output
```

Each step's `TOutput` must match the next step's `TInput`.

```csharp
// Step A
internal sealed class StepAExecutor(IConfiguration config)
    : Executor<string, IntermediateA>("step-a")
{
    public override async ValueTask<IntermediateA> HandleAsync(
        string input, IWorkflowContext ctx, CancellationToken ct = default)
    {
        var client = ChatClientFactory.Create(config);
        // ... call LLM, transform input
        return new IntermediateA(...);
    }
}

// Step B
internal sealed class StepBExecutor(IConfiguration config)
    : Executor<IntermediateA, IntermediateB>("step-b")
{
    public override async ValueTask<IntermediateB> HandleAsync(
        IntermediateA input, IWorkflowContext ctx, CancellationToken ct = default)
    {
        // ... further transform
        return new IntermediateB(...);
    }
}

// Step C (final)
internal sealed class StepCExecutor(IConfiguration config)
    : Executor<IntermediateB, FinalOutput>("step-c")
{
    public override async ValueTask<FinalOutput> HandleAsync(
        IntermediateB input, IWorkflowContext ctx, CancellationToken ct = default)
    {
        var output = new FinalOutput(...);
        await ctx.YieldOutputAsync(output, ct);
        return output;
    }
}

// Factory
public static Workflow Create(IConfiguration config)
{
    var a = new StepAExecutor(config);
    var b = new StepBExecutor(config);
    var c = new StepCExecutor(config);

    return new WorkflowBuilder(a)
        .AddEdge(a, b)
        .AddEdge(b, c)
        .WithOutputFrom(c)
        .WithName("step-a")    // matches StepAExecutor id
        .Build();
}
```

**Registration:**
```csharp
builder.AddWorkflow("step-a", (_, _) => MyPipelineFactory.Create(config));
```

---

## Pattern 4 — Fan-Out / Fan-In

```
               ┌─→ BranchExecutor1 ─┐
Dispatcher ────┼─→ BranchExecutor2 ─┼──→ AggregatorExecutor ──→ [Output]
               └─→ BranchExecutor3 ─┘
```

### Dispatcher (entry)

```csharp
internal sealed class MyDispatcher() : Executor<string, BranchMessage>("dispatcher-id")
{
    public override ValueTask<BranchMessage> HandleAsync(
        string input, IWorkflowContext ctx, CancellationToken ct = default)
    {
        // Validate / enrich input, return the same message to ALL branches
        return ValueTask.FromResult(new BranchMessage(input));
    }
}
```

### Branch Executor

```csharp
internal sealed class Branch1Executor(IConfiguration config)
    : Executor<BranchMessage, Branch1Result>("branch-1")
{
    public override async ValueTask<Branch1Result> HandleAsync(
        BranchMessage msg, IWorkflowContext ctx, CancellationToken ct = default)
    {
        var client = ChatClientFactory.Create(config);
        // ... LLM call specific to this branch
        return new Branch1Result(...);
    }
}
```

### Aggregator (fan-in)

```csharp
internal sealed class MyAggregator(IConfiguration config) : Executor("aggregator-id")
{
    protected override RouteBuilder ConfigureRoutes(RouteBuilder rb) => rb
        .AddHandler<Branch1Result>(Handle1Async)
        .AddHandler<Branch2Result>(Handle2Async)
        .AddHandler<Branch3Result>(Handle3Async);

    private async ValueTask Handle1Async(Branch1Result r, IWorkflowContext ctx, CancellationToken ct)
    {
        await ctx.QueueStateUpdateAsync("branch1", r, cancellationToken: ct);
        await TryEmitAsync(ctx, ct);
    }

    private async ValueTask Handle2Async(Branch2Result r, IWorkflowContext ctx, CancellationToken ct)
    {
        await ctx.QueueStateUpdateAsync("branch2", r, cancellationToken: ct);
        await TryEmitAsync(ctx, ct);
    }

    private async ValueTask Handle3Async(Branch3Result r, IWorkflowContext ctx, CancellationToken ct)
    {
        await ctx.QueueStateUpdateAsync("branch3", r, cancellationToken: ct);
        await TryEmitAsync(ctx, ct);
    }

    private async ValueTask TryEmitAsync(IWorkflowContext ctx, CancellationToken ct)
    {
        var r1 = await ctx.ReadStateAsync<Branch1Result>("branch1", cancellationToken: ct);
        var r2 = await ctx.ReadStateAsync<Branch2Result>("branch2", cancellationToken: ct);
        var r3 = await ctx.ReadStateAsync<Branch3Result>("branch3", cancellationToken: ct);

        if (r1 is null || r2 is null || r3 is null) return;

        var output = new FinalOutput(r1, r2, r3);
        await ctx.YieldOutputAsync(output, ct);
    }
}
```

### Factory

```csharp
public static Workflow Create(IConfiguration config)
{
    var dispatcher  = new MyDispatcher();
    var branch1     = new Branch1Executor(config);
    var branch2     = new Branch2Executor(config);
    var branch3     = new Branch3Executor(config);
    var aggregator  = new MyAggregator(config);

    return new WorkflowBuilder(dispatcher)
        .AddEdge(dispatcher, branch1)
        .AddEdge(dispatcher, branch2)
        .AddEdge(dispatcher, branch3)
        .AddEdge(branch1, aggregator)
        .AddEdge(branch2, aggregator)
        .AddEdge(branch3, aggregator)
        .WithOutputFrom(aggregator)
        .WithName("dispatcher-id")
        .Build();
}
```

**Registration:**
```csharp
builder.AddWorkflow("dispatcher-id", (_, _) => MyFanOutFactory.Create(config));
```

---

## Message / Result Records

Keep all message types in a single `*Messages.cs` file per workflow:

```csharp
namespace MVCAllOptions.AgentWorkflows.Workflows.MyWorkflow;

// Shared dispatch payload
public record BranchMessage(string InputData);

// Branch outputs
public record Branch1Result(string Data1);
public record Branch2Result(string Data2);
public record Branch3Result(string Data3);

// Workflow final output
public record FinalOutput(Branch1Result R1, Branch2Result R2, Branch3Result R3);
```

---

## LLM Call in an Executor

```csharp
var client = ChatClientFactory.Create(config);

var response = await client.CompleteChatAsync(
    [new UserChatMessage("Your prompt here...")],
    cancellationToken: ct);

string text = response.Value.Content[0].Text;
```

For structured JSON extraction, use `JsonDocument.Parse` with try/catch fallback (see `PreferenceAnalyzerExecutor.cs` for the exact pattern).

---

## DevUI Name Alignment Rules

| Code location | Value must equal |
|---------------|-----------------|
| `Executor<,>(id)` constructor string — first executor | `WorkflowBuilder.WithName(name)` |
| `WorkflowBuilder.WithName(name)` | First argument to `builder.AddWorkflow(name, ...)` |
| `"AgentName"` in `builder.AddAIAgent(name, ...)` | String shown in DevUI entity selector |

Mismatches cause silent failures (workflow not visible in DevUI) — always triple-check these strings.
