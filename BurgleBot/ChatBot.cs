using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using BurgleBot.IOAdapters;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.Agents.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;

namespace BurgleBot;

public class ChatBot(IIoAdapter ioAdapter, DefaultKernel kernel, SmartKernel smartKernel, NotSoSmartKernel notSoSmartKernel)
{
    [Experimental("SKEXP0110")]
    public async Task Run()
    {
        var agents = new Agents(smartKernel.Instance, kernel.Instance, notSoSmartKernel.Instance);
        
        var userInput = """
                        Please, create a prompt to extract all persons mentioned in text.
                        I want to extract name, father’s name, mother’s name, father’s age when the person was born and persons lifespan,
                        but only from the text (leave the fields empty if the information is unknown)
                        The result should be in json formal, e.g.:
                        {"Name":"Joshua","Father":"David", "Mother":"Elena","FatherAgeAtBirth":30,"Lifespan":""}
                        """;
        
        var writerThread = new ChatHistoryAgentThread();
        var prompt = string.Empty;
        var feedback = string.Empty;
        await foreach (ChatMessageContent response in agents.promptWriter.InvokeAsync(userInput))
        {
            prompt = response.Content;
        }
        ioAdapter.SendMessageToUser($"New prompt: {prompt}");
        await foreach (ChatMessageContent response in agents.promptChecker.InvokeAsync(prompt))
        {
            feedback = response.Content;
        }
        ioAdapter.SendMessageToUser($"New feedback: {feedback}");

        var improvedPrompt = string.Empty;
        var accuracy = 0f;
        bool isComplete = false;
        do
        {
            await foreach (var response in agents.promptWriter.InvokeAsync(
                               $"Please refine the prompt. Prompt: {prompt}, Feedback: {feedback}"))
            {
                improvedPrompt = response.Message.Content;
            }
            ioAdapter.SendMessageToUser($"\nNew prompt: {improvedPrompt}");
            await foreach (ChatMessageContent response in agents.promptChecker.InvokeAsync(improvedPrompt))
            {
                feedback = response.Content;
            }
            ioAdapter.SendMessageToUser($"\nNew feedback: {feedback}");
            var accuracyStr = await notSoSmartKernel.Instance.InvokePromptAsync(
                "Extract the extraction accuracy in %, provide only the result. {{$input}}",
                new KernelArguments { ["input"] = feedback });
            if (float.TryParse(accuracyStr.ToString().TrimEnd('%'), out var value))
            {
                accuracy = value;
                ioAdapter.SendMessageToUser($"Extraction accuracy: {accuracy}");
            }
            if (accuracy > 90) isComplete = true;
        } while (!isComplete);
        ioAdapter.SendMessageToUser($"BINGO! Final prompt: {prompt}, accuracy: {accuracy}");
    }
}
