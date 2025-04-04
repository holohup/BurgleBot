using BurgleBot.IOAdapters;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace BurgleBot;

public class ChatBot(IOAdapter iOAdapter)
{
    public async Task Chat(Kernel kernel)
    {
        var history = new ChatHistory("You are a mischievous and jovial assistant. You will crack jokes when a user supplies a prompt.");

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        await iOAdapter.SendMessageToUser("User > ");

        string? userInput;
        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        { 
            ToolCallBehavior = ToolCallBehavior.EnableKernelFunctions
        };
        while (!string.IsNullOrWhiteSpace(userInput = await iOAdapter.GetUserInput()))
        {
            history.AddUserMessage(userInput);

            var result =
                await chatCompletionService.GetChatMessageContentAsync(
                    history,
                    executionSettings: openAIPromptExecutionSettings,
                    kernel: kernel);

            await iOAdapter.SendMessageToUser("Assistant > " + result);

            history.AddMessage(result.Role, result.Content ?? string.Empty);

            await iOAdapter.SendMessageToUser("\n" + "User > ");
        }
    }
}
