using System.Net;
using BurgleBot;
using BurgleBot.IOAdapters;
using BurgleBot.Plugins.DataFetcher;
using DotNetEnv.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

var configuration = new ConfigurationBuilder()
    .AddDotNetEnv()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

var services = new ServiceCollection();
services.AddLogging(c => c.AddConsole().SetMinimumLevel(LogLevel.Debug));
services.AddSingleton<HttpLoggingHandler>();

var token = configuration["OPENAI_API_KEY"]!;
var modelId = configuration["MODEL"]!;

var builder = Kernel.CreateBuilder();
builder.AddOpenAIChatCompletion(modelId: modelId, apiKey: token);
builder.Services.AddSingleton<HttpLoggingHandler>();
builder.Services.ConfigureHttpClientDefaults(builder =>
{
    builder.AddHttpMessageHandler<HttpLoggingHandler>();
});
builder.Plugins.AddFromType<DataFetcherPlugin>();
builder.Services.AddLogging(configure => configure.AddConsole());
builder.Services.AddLogging(configure => configure.SetMinimumLevel(LogLevel.Debug));

Kernel kernel = builder.Build();

services.AddSingleton(kernel);
services.AddSingleton<IIoAdapter, ConsoleAdapter>();
services.AddSingleton<ChatBot>();
var serviceProvider = services.BuildServiceProvider();

var chatBot = serviceProvider.GetRequiredService<ChatBot>();
Task.WaitAll(chatBot.Run());
