
using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.AIManagement.Workspaces;

namespace MVCAllOptions;

public class WorkspaceDataSeederContributor
    : IDataSeedContributor, ITransientDependency
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ApplicationWorkspaceManager _workspaceManager;

    public WorkspaceDataSeederContributor(IWorkspaceRepository workspaceRepository, ApplicationWorkspaceManager workspaceManager)
    {
        _workspaceRepository = workspaceRepository;
        _workspaceManager = workspaceManager;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        if (await _workspaceRepository.GetCountAsync() > 0)
        {
            return;
        }

        var ollamaAssistantWorkspace = await _workspaceManager.CreateAsync(
            name: "OllamaAssistant",
            provider: "Ollama",
            modelName: "llama3.2");
        ollamaAssistantWorkspace.ApiBaseUrl = "http://localhost:11434";
        ollamaAssistantWorkspace.ApiKey = "";
        ollamaAssistantWorkspace.ApplicationName = "MVCAllOptions.Web";

        await _workspaceRepository.InsertAsync(ollamaAssistantWorkspace);
        
        var openAiAssistantWorkspace = await _workspaceManager.CreateAsync(
            name: "OpenAIAssistant",
            provider: "OpenAI",
            modelName: "gpt-5");
        openAiAssistantWorkspace.ApiBaseUrl = "https://api.openai.com/v1";
        openAiAssistantWorkspace.ApiKey = "";
        openAiAssistantWorkspace.ApplicationName = "MVCAllOptions.Web";

        await _workspaceRepository.InsertAsync(openAiAssistantWorkspace);
    }
}