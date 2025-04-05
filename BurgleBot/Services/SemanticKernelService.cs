using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace BurgleBot.Services;

public interface ISemanticKernelService
{
    Task<string> AskLlmAboutImage(string prompt, string imageUrl);
}


public class SemanticKernelAdapter : ISemanticKernelService
{
    public Kernel? Kernel { get; set; }
    public async Task<string> AskLlmAboutImage(string prompt, string imageUrl)
    {
        if (Kernel is null)  throw new InvalidOperationException("Kernel has not been initialized.");
        var chatHistory = new ChatHistory(prompt);
        chatHistory.AddUserMessage([new ImageContent(new Uri(imageUrl))]);
        var completionService = Kernel.Services.GetRequiredService<IChatCompletionService>();
        var result = await completionService.GetChatMessageContentAsync(chatHistory, new OpenAIPromptExecutionSettings());
        return result.Content  ?? "Could not get chat message content.";
    }
}
