using BurgleBot;
using BurgleBot.IOAdapters;
using BurgleBot.Plugins.DataFetcher;
using BurgleBot.Plugins.DataProcessor;
using BurgleBot.Services;
using DotNetEnv.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

string textPluginDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Plugins", "TextPlugin");
var semanticKernelAdapter = new SemanticKernelAdapter();
builder.Services.AddSingleton<ISemanticKernelService>(semanticKernelAdapter);
builder.Plugins.AddFromType<DataFetcherPlugin>();
builder.Plugins.AddFromType<DataProcessorPlugin>();
builder.Plugins.AddFromPromptDirectory(textPluginDirectory);

Kernel kernel = builder.Build();
semanticKernelAdapter.Kernel = kernel;

var services = new ServiceCollection();
services.AddSingleton(kernel);
services.AddSingleton<IIoAdapter, ConsoleAdapter>();
services.AddSingleton<ChatBot>();

var serviceProvider = services.BuildServiceProvider();

var chatBot = serviceProvider.GetRequiredService<ChatBot>();

Task.WaitAll(chatBot.Run());
