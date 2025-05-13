using System.Diagnostics.CodeAnalysis;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.OpenAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace BurgleBot;

public class Agents
{
    public ChatCompletionAgent promptChecker;
    public ChatCompletionAgent  mainBot;
    
    public Agents(Kernel kernel)
    {
        mainBot = new ChatCompletionAgent
        {
            Name = "BurgleBot",
            Instructions = "Summarize user input",
            Kernel = kernel,
            Arguments = new KernelArguments(
                new OpenAIPromptExecutionSettings
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                    Temperature = 0,
                    MaxTokens = 4096,
                    TopP = 0.1
                })
        };

        var checkerKernel = kernel.Clone();
        checkerKernel.ImportPluginFromPromptDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Plugins", "TextPlugins"));
        promptChecker = new ChatCompletionAgent
        {
            Name = "PromptChecker",
            Instructions = "You are a professional agent who checks prompts. Your job is to check what the LLM returns given prompt and input, run the tests on the results and return test results.",
            Kernel = checkerKernel,
            Arguments = new KernelArguments(
                new OpenAIPromptExecutionSettings
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                    Temperature = 0,
                    MaxTokens = 4096,
                    TopP = 0.1,
                })
        };
    }
}