using DotNetEnv.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;


var configuration = new ConfigurationBuilder()
    .AddDotNetEnv()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

var token = configuration["OPENAI_API_KEY"]!;
var modelId = configuration["MODEL"]!;
var builder = Kernel.CreateBuilder().AddOpenAIChatCompletion(modelId, token);
Kernel kernel = builder.Build();

var history = new ChatHistory("You are a mischievous and jovial assistant. You will crack jokes when a user supplies a prompt.");

var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();


Console.Write("User > ");

string? userInput;
OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
{ 
    ToolCallBehavior = ToolCallBehavior.EnableKernelFunctions
};
while ((userInput = Console.ReadLine()) != null)
{
    // Add user input
    history.AddUserMessage(userInput);


    // Get the response from the AI
    var result = 
        await chatCompletionService.GetChatMessageContentAsync(
            history,
            executionSettings: openAIPromptExecutionSettings,
            kernel: kernel);


    // Print the results
    Console.WriteLine("Assistant > " + result);


    // Add the message from the agent to the chat history
    history.AddMessage(result.Role, result.Content ?? string.Empty);

    // Get user input again
    Console.Write("User > ");
}
