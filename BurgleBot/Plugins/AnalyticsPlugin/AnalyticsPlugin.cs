using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using OpenAI.Chat;

namespace BurgleBot.Plugins.VersionBumper;

public sealed class AnalyticsPlugin
{
    Kernel? _kernel;

    public void Init(Kernel kernel)
    {
        _kernel = kernel;
    }
    
    // [KernelFunction, Description("Solves all mathematical problems")]
    // public async Task<string> Calculator(
    //     [Description("Problem to solve in text form")] string problem)
    // {
    //     var history = new ChatHistory("You are a professional mathematician with IQ of 120.");
    //     history.AddUserMessage($"Please, solve this problem: {problem}. Provide the result in plain text (no formatting).");
    //     var chatService = _kernel.GetRequiredService<IChatCompletionService>();
    //     var result = await chatService.GetChatMessageContentsAsync(history);
    //     var resultStr = string.Join("; ", result.Select(i => i.Content));
    //     Console.WriteLine($"Math: {resultStr}");
    //     return resultStr;
    // }
    
    [KernelFunction, Description("Provides recipe for a bread")]
    [Experimental("SKEXP0010")]
    public async Task<string> BreadRecipeProvider(
        [Description("Which bread does the user want to make")] string bread)
    {
        var history = new ChatHistory("You are a professional baker. ");
        history.AddUserMessage($"Help the user create a bread recipe: {bread}");
        var chatService = _kernel.GetRequiredService<IChatCompletionService>();
        var executionSettings = new AzureOpenAIPromptExecutionSettings
        {
            ResponseFormat = typeof(Strategy)
        }; 
        var result = await chatService.GetChatMessageContentsAsync(history, executionSettings);
        var resultStr = string.Join("; ", result.Select(i => i.Content));
        Console.WriteLine($"Plugin: {resultStr}");
        return resultStr;
    }
}

// public record Strategy
// {
//     public string Result { get; set; }
// }


// public record Strategy
// {
//     public List<Step> Steps { get; set; }
//     public string Result { get; set; }
// }

public record Step
{
    public string Reasoning { get; set; }
    public string Result { get; set; }
}





public record Strategy
{
    public string breadOriginalRegion { get; set; }
    public string breadDescriptionInOriginalRegionLanguage { get; set; }
    public bool isThisRyeBread { get; set; }
    public bool isThisWheatBread { get; set; }
    public bool isTheBreadSourdough { get; set; }
    public bool isBreadYeast { get; set; }
    public bool doesBreadRequireSpecialSourdough { get; set; }
    public string? specialSourdoughName { get; set; }
    public bool doesBreadRequireSpecificYeastStrain { get; set; }
    public string? specialYeastStranName { get; set; }
    public int bestRyeBreadFermentationTemperature { get; set; }
    public int bestWheatBreadFermentationTemperature { get; set; }
    public int specialSourdoughFermentationTemperature {get; set;}
    public int yeastFermentationTemperature {get; set;}
    public int doesBreadRequireRyeScald { get; set; }
    public int doughTemperatureAfterMixing { get; set; }
    public int doughFermentingTemperature  { get; set; }
    public int doughProofingTemperature { get; set; }
    public string specialShapingInstructions { get; set; }
    public string specialBakingInstructions { get; set; }
    public bool doesBreadRequireBakingStones { get; set; }
    public int initialOvenTemperature { get; set; }
    public bool doesBakingRequireVapor { get; set; }
    public int breadBakingTemperature { get; set; }
    public string recipeWithWeightsFor500GramsOfFlour { get; set; }
    public string Result { get; set; }
}