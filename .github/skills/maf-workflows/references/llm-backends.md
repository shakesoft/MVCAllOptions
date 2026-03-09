# LLM Backends — OpenRouter vs Azure OpenAI

This project's `ChatClientFactory.cs` currently uses **OpenRouter**.
Both implementations produce a `ChatClient` from the standard `OpenAI` package — no other MAF code changes when switching backends.

---

## Active: OpenRouter

OpenRouter exposes an OpenAI-compatible API, so the standard `OpenAI` NuGet package works directly by pointing its endpoint at `https://openrouter.ai/api/v1`.

### NuGet packages required

```xml
<PackageReference Include="OpenAI" Version="2.*" />
```

### ChatClientFactory.cs (current implementation)

```csharp
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

internal static class ChatClientFactory
{
    private const string OpenRouterEndpoint = "https://openrouter.ai/api/v1";

    public static ChatClient Create(IConfiguration config)
    {
        var apiKey = config["OpenRouter:ApiKey"]
            ?? throw new InvalidOperationException(
                "OpenRouter:ApiKey is required. Add it to appsettings.json or set OPENROUTER__APIKEY env var.");

        var model = config["OpenRouter:Model"] ?? "openai/gpt-4o-mini";

        return new OpenAIClient(
            new ApiKeyCredential(apiKey),
            new OpenAIClientOptions { Endpoint = new Uri(OpenRouterEndpoint) })
            .GetChatClient(model);
    }
}
```

### appsettings.json

```json
{
  "OpenRouter": {
    "ApiKey": "sk-or-v1-...",
    "Model":  "openai/gpt-4o-mini"
  }
}
```

### Environment variables

```bash
export OPENROUTER__APIKEY="sk-or-v1-..."
export OPENROUTER__MODEL="openai/gpt-4o-mini"
```

### Choosing a model

Browse available models at `https://openrouter.ai/models`. Models that conform to the OpenAI Chat Completions schema work with tool-calling agents. Recommended models:

| Model slug | Notes |
|-----------|-------|
| `openai/gpt-4o-mini` | Default — fast, cheap, good tool-calling |
| `openai/gpt-4o` | More capable for complex reasoning |
| `anthropic/claude-3.5-sonnet` | Strong reasoning; tool support may vary |
| `google/gemini-flash-1.5` | Budget option |

---

## Alternative: Azure OpenAI

### NuGet packages required

```xml
<PackageReference Include="Azure.AI.OpenAI"  Version="2.*" />
<PackageReference Include="Azure.Identity"   Version="1.*" />
```

### ChatClientFactory.cs (Azure OpenAI variant)

Replace the body of `ChatClientFactory.cs` with this when switching to Azure OpenAI:

```csharp
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

internal static class ChatClientFactory
{
    public static ChatClient Create(IConfiguration config)
    {
        var endpoint = config["AzureOpenAI:Endpoint"]
            ?? throw new InvalidOperationException(
                "AzureOpenAI:Endpoint is required. Add it to appsettings.json or set AZUREOPENAI__ENDPOINT env var.");

        var deploymentName = config["AzureOpenAI:DeploymentName"] ?? "gpt-4o-mini";

        // AzureCliCredential: uses the token from 'az login' — no API key stored in config.
        // Swap with DefaultAzureCredential for production / managed identity environments.
        var credential = new AzureCliCredential();

        return new AzureOpenAIClient(new Uri(endpoint), credential)
            .GetChatClient(deploymentName);
    }
}
```

### appsettings.json

```json
{
  "AzureOpenAI": {
    "Endpoint":       "https://<your-resource>.openai.azure.com/",
    "DeploymentName": "gpt-4o-mini"
  }
}
```

### Environment variables

```bash
export AZUREOPENAI__ENDPOINT="https://<your-resource>.openai.azure.com/"
export AZUREOPENAI__DEPLOYMENTNAME="gpt-4o-mini"
```

### Authentication options

| Credential class | When to use |
|-----------------|-------------|
| `AzureCliCredential` | Local development after `az login` |
| `DefaultAzureCredential` | Production; tries managed identity, workload identity, CLI, etc. |
| `ClientSecretCredential` | Service principal with client secret (CI/CD) |
| `ManagedIdentityCredential` | Azure-hosted app with managed identity |

### Pre-requisites

1. `az login` (for `AzureCliCredential`)
2. An Azure OpenAI resource with a deployed model
3. Your Azure AD account must have the **Cognitive Services OpenAI User** role on the resource

---

## Switching Backends — Checklist

1. Replace `ChatClientFactory.cs` body with the target implementation
2. Update `appsettings.json` with the new config section
3. Add / remove NuGet packages as needed
4. Run `dotnet build src/MVCAllOptions.AgentWorkflows` — no other files should need changes
5. Verify `dotnet run` prints the console demo with real LLM responses

> **No MAF workflow or executor code changes** — `ChatClient` from the `OpenAI` package is the shared abstraction used by both `AsAIAgent()` and direct `CompleteChatAsync()` calls throughout the project.
