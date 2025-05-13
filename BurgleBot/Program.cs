using BurgleBot;
using BurgleBot.IOAdapters;
using BurgleBot.Plugins.DataFetcher;
using DotNetEnv.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;


var configuration = new ConfigurationBuilder()
    .AddDotNetEnv()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

var token = configuration["OPENAI_API_KEY"]!;
var modelId = configuration["MODEL"]!;
var builder = Kernel.CreateBuilder().AddOpenAIChatCompletion(modelId, token);
// builder.Plugins.AddFromType<DataFetcherPlugin>();
builder.Services.AddLogging(configure => configure.AddConsole());
builder.Services.AddLogging(configure => configure.SetMinimumLevel(LogLevel.Trace));
Kernel kernel = builder.Build();


var services = new ServiceCollection();
services.AddSingleton(kernel);
services.AddSingleton<IIoAdapter, ConsoleAdapter>();
services.AddSingleton<ChatBot>();
var serviceProvider = services.BuildServiceProvider();



var chatBot = serviceProvider.GetRequiredService<ChatBot>();
Task.WaitAll(chatBot.Run());
