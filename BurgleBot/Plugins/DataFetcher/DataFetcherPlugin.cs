using System.ComponentModel;
using System.Text.Json;
using BurgleBot.Models;
using Microsoft.SemanticKernel;

namespace BurgleBot.Plugins.DataFetcher;

public sealed class DataFetcherPlugin
{
    private static readonly HttpClient HttpClient = new HttpClient();
    
    [KernelFunction, Description("Determines the current datetime in UTC")]
    public DateTime GetCurrentDateTime() => DateTime.UtcNow;

    [KernelFunction, Description("Gets a link to an animal picture from the internet")]
    public async Task<string> GetAnimalImageUrlAsync([Description("Type of an animal to give a link to")] Animal animal)
    {
        string? apiUrl = animal switch
        {
            Animal.Cat => "https://api.thecatapi.com/v1/images/search",
            Animal.Dog => "https://dog.ceo/api/breeds/image/random",
            _ => null
        };

        if (apiUrl == null)
        {
            return "Please select a valid animal type.";
        }

        try
        {
            HttpResponseMessage response = await HttpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            string? imageUrl = animal switch
            {
                Animal.Cat => JsonDocument.Parse(responseBody).RootElement[0].GetProperty("url").GetString(),
                Animal.Dog => JsonDocument.Parse(responseBody).RootElement.GetProperty("message").GetString(),
                _ => null
            };

            return imageUrl ?? "Image URL not found in the response.";
        }
        catch (Exception ex)
        {
            return $"An error occurred: {ex.Message}";
        }
    }
}
