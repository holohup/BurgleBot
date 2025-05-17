using System.Diagnostics.CodeAnalysis;
using System.Text;
using BurgleBot.IOAdapters;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OpenAI.Chat;
using ChatMessageContent = Microsoft.SemanticKernel.ChatMessageContent;

namespace BurgleBot;

public class ChatBot(IIoAdapter iIoAdapter, Kernel kernel)
{
    [Experimental("SKEXP0010")]
    public async Task Run()
    {
        var history = new ChatHistory();

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        await iIoAdapter.SendMessageToUser("Hello! I am a helpful agent. What should I do?");

        string? userInput;
        OpenAIPromptExecutionSettings openAiPromptExecutionSettings = new()
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            ChatSystemPrompt =
                "You are a professional assistant, your job is to help. Be thoughtful, always check your response. Do not provide your chain of thought in the response, just the result.",
            MaxTokens = 1024,
            TopP = 0.9,
            Temperature = 0.0,
            PresencePenalty = 0,
            FrequencyPenalty = 0,
            Logprobs = true
        };
        while (!string.IsNullOrWhiteSpace(userInput = await iIoAdapter.GetUserInput()))
        {
            history.AddUserMessage(userInput);

            var result =
                await chatCompletionService.GetChatMessageContentAsync(
                    history,
                    executionSettings: openAiPromptExecutionSettings,
                    kernel: kernel);
            HighlightResponse(result);
            history.AddAssistantMessage(result.Content ?? string.Empty);
        }
    }

    private void HighlightResponse(ChatMessageContent? resultContent)
    {
        Console.Write("Assistant: ");
        if (resultContent is OpenAIChatMessageContent openAiContent &&
            openAiContent.InnerContent is ChatCompletion chatCompletion &&
            chatCompletion.ContentTokenLogProbabilities is { Count: > 0 })
        {
            foreach (var tokenDetail in chatCompletion.ContentTokenLogProbabilities)
            {
                var token = tokenDetail.Token;
                var logProb = tokenDetail.LogProbability;

                var prob = Math.Exp(logProb);
                var clamped = Math.Clamp(prob, 0.0, 1.0);
                var red = (int)((1 - clamped) * 255);
                var green = (int)(clamped * 255);
                var coloredToken = $"\u001b[38;2;{red};{green};0m{token}\u001b[0m";
                Console.Write(coloredToken);
            }
        }

        Console.WriteLine();
    }
}
