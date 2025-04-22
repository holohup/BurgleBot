using System.Diagnostics.CodeAnalysis;
using BurgleBot.IOAdapters;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace BurgleBot;

public class ChatBot(IIoAdapter ioAdapter, Kernel kernel)
{
    [Experimental("SKEXP0001")]
    public async Task Run()
    {
        var history = new ChatHistory();
        var reducer = new ChatHistoryTruncationReducer(targetCount: 2, thresholdCount: 5);
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        await ioAdapter.SendMessageToUser("Hello! I am a helpful agent. What should I do?");

        string? userInput;
        OpenAIPromptExecutionSettings openAiPromptExecutionSettings = new()
        { 
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            ChatSystemPrompt = @"You are a professional assistant, your job is to help. When processing images, 
                                always provide the link to original image. When dealing with documents, always provide 
                                the document name after each quote or summary. Under no circumstances provide links to 
                                documents, just document names. Links are intended only for animal pictures. Do not use
                                any text formatting, output plain text.",
            MaxTokens = 4096,
            TopP = 0.1,
            Temperature = 0.1,
            PresencePenalty = 0,
            FrequencyPenalty = 0
        };
        while (!string.IsNullOrWhiteSpace(userInput = await ioAdapter.GetUserInput()))
        {
            history.AddUserMessage(userInput);
            var reducedMessages = await reducer.ReduceAsync(history);

            if (reducedMessages is not null)
            {
                history = new ChatHistory(reducedMessages);
            }
            var result =
                await chatCompletionService.GetChatMessageContentAsync(
                    history,
                    executionSettings: openAiPromptExecutionSettings,
                    kernel: kernel);

            await ioAdapter.SendMessageToUser(result.Content);
            history.AddAssistantMessage(result.Content ?? string.Empty);
        }
    }
}
