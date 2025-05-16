using System.ComponentModel;
using System.Text.Json;
using Microsoft.SemanticKernel;

namespace BurgleBot.Plugins.DataFetcher;

public sealed class PromptValidationPlugin(Kernel kernel)
{
    
    [KernelFunction, Description("Validates the prompt provided using sample data")]
    public async Task<string> ValidatePrompt([Description("Prompt to validate")] string prompt)
    {
        var sampleText = File.ReadAllText("Plugins/Genesis4-5-11.txt");
        var args = new KernelArguments{["prompt"] = prompt, ["input"] = sampleText};
        var result = await kernel.InvokeAsync(pluginName: "CheckerPlugins", functionName: "LlmPromptPlugin", arguments: args);
        var textResult = result.ToString();
        var expected = JsonSerializer.Serialize(PersonHandler.GetExpectedResults());
        args = new KernelArguments{["expected"] = expected, ["extracted"]=textResult};
        result = await kernel.InvokeAsync(pluginName: "CheckerPlugins", functionName: "CompareJsons", arguments: args);
        return result.ToString();
    }
}

public record Person
{
    public required string Name { get; set; } 
    public string? Father { get; set; }
    public string? Mother { get; set; }
    public int? FatherAgeAtBirth { get; set; }
    public int? Lifespan { get; set; }
}



public static class PersonHandler
{
    public static List<Person> GetExpectedResults() => LoadFromJsonLines("Plugins/ExpectedResults.jsonl");
    
    public static List<Person> LoadFromJsonLines(string filePath)
    {
        var records = new List<Person>();
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        foreach (var line in File.ReadLines(filePath))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }
            try
            {
                var record = JsonSerializer.Deserialize<Person>(line, options);
                if (record != null)
                {
                    records.Add(record);
                }
            }
            catch (JsonException ex)
            {
                Console.Error.WriteLine($"Failed to parse line: {ex.Message}");
            }
        }

        return records;
    }
}
