using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;

namespace BurgleBot.Services;

public interface ISemanticKernelService
{
    Task<SearchResult> FetchRecipeByVector(string query);
    Task<SearchResult> FetchComputerSpecByVector(string query);
    Task<SearchResult> FetchApplianceSpecByVector(string query);
}


public class SemanticKernelAdapter : ISemanticKernelService
{
    public Kernel? SKernel { get; set; }
    public IKernelMemory? Memory { get; set; }
    
    public async Task<SearchResult> FetchRecipeByVector(string query)
    {
        return await InvokeMemorySearchByIndex(query, "recipes");
    }

    public async Task<SearchResult> FetchComputerSpecByVector(string query)
    {
        return await InvokeMemorySearchByIndex(query, "computers");
    }

    public async Task<SearchResult> FetchApplianceSpecByVector(string query)
    {
        return await InvokeMemorySearchByIndex(query, "appliances");
    }


    private async Task<SearchResult> InvokeMemorySearchByIndex(string query, string index)
    {
        return await Memory!.SearchAsync(query, index: index, minRelevance: 0.75);

    }
}
