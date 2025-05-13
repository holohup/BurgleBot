using BurgleBot.IOAdapters;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace BurgleBot;

public class ChatBot(IIoAdapter ioAdapter, Kernel kernel)
{
    public async Task Run()
    {
        var agents = new Agents(kernel);

        var mainChatThread = new ChatHistoryAgentThread();
        var isComplete = false;


        await ioAdapter.SendMessageToUser("Hello! I am a professional prompt creator. ");

        string? userInput;
        while (!string.IsNullOrWhiteSpace(userInput = await ioAdapter.GetUserInput()))
        {
            var message = new ChatMessageContent(AuthorRole.User, userInput);
            await foreach (var response in agents.promptChecker.InvokeAsync(message, mainChatThread))
            {
                ioAdapter.SendMessageToUser($"{response.Message}");
            }
        }
    }
}
