using System.ComponentModel;
using System.Text.Json;
using BurgleBot.Models;
using BurgleBot.Services;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;

namespace BurgleBot.Plugins.DataFetcher;

public sealed class DataFetcherPlugin(ISemanticKernelService kernelService)
{
    private const int MaxCharsInResponse = 100000;
    
    [KernelFunction, Description("Returns results of recipe vector search by user query in a json.")]
    public async Task<string> FetchRecipe([Description("User query")] string query)
    {
        var result = await kernelService.FetchRecipeByVector(query);
        return GroupAndTruncateResponse(result);
    }

    [KernelFunction, Description("Returns results of vector search in computer manuals.")]
    public async Task<string> FetchComputerSpecs([Description("User query")] string query)
    {
        var result = await kernelService.FetchComputerSpecByVector(query);
        return GroupAndTruncateResponse(result);
    }
    
    [KernelFunction, Description("Returns results of vector search to in kitchen appliance manuals.")]
    public async Task<string> FetchFromApplianceManual([Description("User query")] string query)
    {
        var result = await kernelService.FetchApplianceSpecByVector(query);
        return GroupAndTruncateResponse(result);
    }
    
    private static string GroupAndTruncateResponse(SearchResult result)
    {
        var topPartitions = result.Results
            .SelectMany(citation => citation.Partitions.Select(partition => new
            {
                SourceName = citation.SourceName,
                Text = partition.Text,
                Relevance = partition.Relevance
            }))
            .OrderByDescending(x => x.Relevance)
            .Take(10)
            .Select(x => new
            {
                x.SourceName,
                x.Text
            });
        
        string json = JsonSerializer.Serialize(
            topPartitions,
            new JsonSerializerOptions { WriteIndented = true }
        );
        
        int length = Math.Min(json.Length, MaxCharsInResponse);
        return json.Substring(0, length);
    }
}
