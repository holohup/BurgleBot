using BurgleBot;
using BurgleBot.IOAdapters;
using BurgleBot.Plugins.DataFetcher;
using BurgleBot.Services;
using DotNetEnv.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.Configuration;
using Microsoft.SemanticKernel;


var configuration = new ConfigurationBuilder()
    .AddDotNetEnv()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

var token = configuration["OPENAI_API_KEY"] ?? throw new ApplicationException("OPENAI_API_KEY not found");
var modelId = configuration["MODEL"] ?? throw new ApplicationException("MODEL not found");
var builder = Kernel.CreateBuilder().AddOpenAIChatCompletion(modelId, token);

builder.Services.AddLogging(configure => configure.AddConsole());
builder.Services.AddLogging(configure => configure.SetMinimumLevel(LogLevel.Information));

var semanticKernelAdapter = new SemanticKernelAdapter();
builder.Services.AddSingleton<ISemanticKernelService>(semanticKernelAdapter);
builder.Plugins.AddFromType<DataFetcherPlugin>();

Kernel kernel = builder.Build();
semanticKernelAdapter.SKernel = kernel;

var services = new ServiceCollection();
services.AddSingleton(kernel);
services.AddSingleton<IIoAdapter, ConsoleAdapter>();
services.AddSingleton<ChatBot>();


var memoryConnector = GetMemoryConnector(token, false);
semanticKernelAdapter.Memory = memoryConnector;

var serviceProvider = services.BuildServiceProvider();

var chatBot = serviceProvider.GetRequiredService<ChatBot>();

kernel.ImportPluginFromObject(new MemoryPlugin(memoryConnector, waitForIngestionToComplete: true), "memory");

await LoadAllPdfDocumentsAsync(memoryConnector);

#pragma warning disable SKEXP0001
Task.WaitAll(chatBot.Run());
#pragma warning restore SKEXP0001


#region Memory
static IKernelMemory GetMemoryConnector(string token, bool serverless = false)
{
    if (!serverless) return new MemoryWebClient("http://127.0.0.1:9001/", token);

    return new KernelMemoryBuilder()
        .WithOpenAITextEmbeddingGeneration(new OpenAIConfig
        {
            EmbeddingModel = "text-embedding-3-small",
            APIKey = token,
        })
        .WithOpenAITextGeneration(new OpenAIConfig
        {
            TextModel = "gpt-4o-mini",
            APIKey = token,
        })
        .WithCustomTextPartitioningOptions(
            new TextPartitioningOptions
            {
                MaxTokensPerParagraph = 600,
                OverlappingTokens = 50
            })
        .Build<MemoryServerless>();
}

static async Task<bool> DocumentExistsAsync(IKernelMemory memoryConnector, string documentId, string? index)
{
    try
    {
        var documentExists = await memoryConnector.IsDocumentReadyAsync(documentId, index: index);
        Console.WriteLine($"{documentId} exists: {documentExists}");
        return await memoryConnector.IsDocumentReadyAsync(documentId, index: index);
    }
    catch (Exception)
    {
        return false;
    }
}
static async Task LoadAllPdfDocumentsAsync(IKernelMemory memoryConnector)
{
    string docsFolder = Path.Combine(Directory.GetCurrentDirectory(), "docs");

    if (!Directory.Exists(docsFolder))
    {
        Console.WriteLine($"Docs folder not found at '{docsFolder}'.");
        return;
    }
    
    var pdfFiles = Directory.GetFiles(docsFolder, "*.pdf");

    foreach (var pdfFile in pdfFiles)
    {
        string documentId = Path.GetFileNameWithoutExtension(pdfFile);
        var index = documentId.ToLowerInvariant() switch
        {
            var name when name.Contains("recipe") => "recipes",
            var name when name.Contains("computer") => "computers",
            var name when name.Contains("appliance") => "appliances",
            _ => null
        };
        if (await DocumentExistsAsync(memoryConnector, documentId, index)) continue;
        

        Console.WriteLine($"Importing document '{documentId}' from file '{pdfFile}, index: {index}'.");
        if (index is not null) await memoryConnector.ImportDocumentAsync(filePath: pdfFile, documentId: documentId, index: index);
        else await memoryConnector.ImportDocumentAsync(filePath: pdfFile, documentId: documentId);
    }
}

#endregion
