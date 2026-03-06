using Microsoft.Extensions.AI;
using OllamaSharp;
using System.Threading.Tasks;
using Volo.AIManagement.Workspaces;
using Volo.AIManagement.Factory;
using Volo.Abp.DependencyInjection;

namespace MVCAllOptions;

public class OllamaChatClientFactory : IChatClientFactory, ITransientDependency
{
    public string Provider => "Ollama";

    public Task<IChatClient> CreateAsync(ChatClientCreationConfiguration configuration)
    {
        var client = new OllamaApiClient(
            configuration.ApiBaseUrl ?? "http://localhost:11434",
            configuration.ModelName);

        return Task.FromResult<IChatClient>(client);
    }
}