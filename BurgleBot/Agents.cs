using BurgleBot.Plugins.DataFetcher;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace BurgleBot;

public class Agents
{
    public ChatCompletionAgent promptChecker;
    public ChatCompletionAgent promptWriter;
    
    public Agents(Kernel smartKernel, Kernel defaultKernel, Kernel notSoSmartKernel)
    {
        var kernel = smartKernel.Clone();
        kernel.ImportPluginFromPromptDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Plugins", "WriterPlugins"));
        promptWriter = new ChatCompletionAgent
        {
            Name = "PromptWriter",
            Instructions = """
                           You are a professional LLM prompt creator. Your job is to create and refine prompts.
                           **Never** emit analysis, commentary, or instructions to — only output the final text of the
                           LLM-prompt itself, plain text without formatting.
                           """,
            Kernel = kernel,
            Arguments = new KernelArguments(
                new OpenAIPromptExecutionSettings
                {
                    Temperature = 0.4,
                    MaxTokens = 4096,
                    TopP = 0.5,
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                })
        };

        var checkerKernel = defaultKernel.Clone();
        var workingKernel = defaultKernel.Clone();
        workingKernel.ImportPluginFromPromptDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Plugins", "CheckerPlugins"));
        var plugin = new PromptValidationPlugin(workingKernel);
        checkerKernel.ImportPluginFromObject(plugin, "PromptValidationPlugin");
        promptChecker = new ChatCompletionAgent
        {
            Name = "PromptChecker",
            Instructions = """
                           Your job is to validate each prompt you receive using ValidatePrompt function and
                           provide the following feedback:
                           • the percentage of correct extractions
                           • if you see some regularity in errors, your suspicions.
                           • if you have, improvement suggestions

                           Under no circumstances create your own prompt, or rewrite other prompts - your job is just to
                           validate using the functions you have and provide analytics on the results.
                           
                           Never give away any data from the samples and expected results,
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