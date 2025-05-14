using BurgleBot.Plugins.DataFetcher;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace BurgleBot;

public class Agents
{
    public ChatCompletionAgent promptChecker;
    public ChatCompletionAgent promptWriter;
    
    public Agents(Kernel smartKernel, Kernel defaultKernel)
    {
        var kernel = defaultKernel.Clone();
        promptWriter = new ChatCompletionAgent
        {
            Name = "PromptWriter",
            Instructions = """
                           You are a professional prompt creator. You need to create a prompt which accomplishes the task
                           defined by the user in the best way possible. Prompt creation can be tricky, you must act iteratively:
                           when you see the results of your current prompt, you must decide, whether to improve the current
                           idea, or to come up with a new one. Take into account the feedback you receive. Under no circumstances
                           give up!
                           """,
            Kernel = kernel,
            Arguments = new KernelArguments(
                new OpenAIPromptExecutionSettings
                {
                    Temperature = 0.3,
                    MaxTokens = 4096,
                    TopP = 0.5
                })
        };

        var checkerKernel = smartKernel.Clone();
        checkerKernel.ImportPluginFromPromptDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Plugins", "TextPlugins"));
        checkerKernel.ImportPluginFromType<DataFetcherPlugin>();
        promptChecker = new ChatCompletionAgent
        {
            Name = "PromptChecker",
            Instructions = """
                           You are a professional agent who checks prompts.
                           For each given prompt you must:
                           1) Use the sample data to check the prompt against the LLM
                           2) Use the results returned by the LLM to validate
                           Your job is to check what the LLM returns given prompt and sample input, test the results and report.
                           In your report provide the percentage of correct extractions, and also if you see some regularity
                           in errors, your suspicions. Under no circumstances give away any data from the samples and expected results,
                           since it might bias the LLM creating the prompt to tune the prompt to the sample, and the tests should be
                           fully independent of the implementation. 
                           """,
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