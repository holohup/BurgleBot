using BurgleBot.IOAdapters;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace BurgleBot;

public class ChatBot(IIoAdapter iIoAdapter, Kernel kernel)
{
    public async Task Run()
    {
        var history = new ChatHistory();

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        await iIoAdapter.SendMessageToUser("Hello! I am a helpful agent. What should I do?");

        string? userInput;
        OpenAIPromptExecutionSettings openAiPromptExecutionSettings = new()
        { 
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            ChatSystemPrompt = @"You are a professional assistant, your job is to help. When processing images, always provide the link to original image.",
            // ChatSystemPrompt = @"You are a professional assistant, your job is to help. Be thoughtful, 
            //                      always check your response. Do not provide your chain of thought in the response, 
            //                      just the result. When processing images, always provide the link to original image.",
            MaxTokens = 1024,
            TopP = 0.1,
            Temperature = 0.1,
            PresencePenalty = 0,
            FrequencyPenalty = 0
        };
        while (!string.IsNullOrWhiteSpace(userInput = await iIoAdapter.GetUserInput()))
        {
            history.AddUserMessage(userInput);

            var result =
                await chatCompletionService.GetChatMessageContentAsync(
                    history,
                    executionSettings: openAiPromptExecutionSettings,
                    kernel: kernel);

            await iIoAdapter.SendMessageToUser(result.Content);
            history.AddAssistantMessage(result.Content ?? string.Empty);
        }
    }
}
