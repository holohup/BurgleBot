using Microsoft.Extensions.DependencyInjection;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace BurgleBot.Services;

public interface ISemanticKernelService
{
    Task<string> AskLlmAboutImage(string prompt, string imageUrl);
    Task<SearchResult> FetchRecipeByVector(string query);
    Task<SearchResult> FetchComputerSpecByVector(string query);
    Task<SearchResult> FetchApplianceSpecByVector(string query);
}


public class SemanticKernelAdapter : ISemanticKernelService
{
    public Kernel? SKernel { get; set; }
    public IKernelMemory? Memory { get; set; }
    
    public async Task<string> AskLlmAboutImage(string prompt, string imageUrl)
    {
        if (SKernel is null)  throw new InvalidOperationException("Kernel has not been initialized.");
        var chatHistory = new ChatHistory(prompt);
        chatHistory.AddUserMessage([new ImageContent(new Uri(imageUrl))]);
        var completionService = SKernel.Services.GetRequiredService<IChatCompletionService>();
        var result = await completionService.GetChatMessageContentAsync(chatHistory, new OpenAIPromptExecutionSettings());
        return result.Content  ?? "Could not get chat message content.";
    }

    public async Task<SearchResult> FetchRecipeByVector(string query)
    {
        return await InvokeMemorySearchByIndex(query, "recipes");
    }

    public async Task<SearchResult> FetchComputerSpecByVector(string query)
    {
        return await InvokeMemorySearchByIndex(query, "computers");
    }

    public async Task<SearchResult> FetchApplianceSpecByVector(string query)
    {
        return await InvokeMemorySearchByIndex(query, "appliances");
    }


    private async Task<SearchResult> InvokeMemorySearchByIndex(string query, string index)
    {
        return await Memory!.SearchAsync(query, index: index, minRelevance: 0.8);

    }
}
