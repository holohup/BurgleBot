using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace BurgleBot.Plugins.DataFetcher;

public sealed class DataFetcherPlugin
{
    [KernelFunction, Description("Determines the current datetime in UTC")]
    public DateTime GetCurrentDateTime() => DateTime.UtcNow;
}