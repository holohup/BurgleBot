using BurgleBot;
using BurgleBot.IOAdapters;
using BurgleBot.Plugins.DataFetcher;
using BurgleBot.Plugins.VersionBumper;
using DotNetEnv.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;


var configuration = new ConfigurationBuilder()
    .AddDotNetEnv()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

var token = configuration["OPENAI_API_KEY"]!;
var modelId = configuration["MODEL"]!;
var builder = Kernel.CreateBuilder().AddOpenAIChatCompletion(modelId, token);
var versionBumperPlugin = new AnalyticsPlugin();
builder.Plugins.AddFromType<DataFetcherPlugin>().AddFromObject(versionBumperPlugin);
builder.Services.AddLogging(configure => configure.AddConsole());
builder.Services.AddLogging(configure => configure.SetMinimumLevel(LogLevel.Information));
Kernel kernel = builder.Build();
versionBumperPlugin.Init(kernel);

var services = new ServiceCollection();
services.AddSingleton(kernel);
services.AddSingleton<IIoAdapter, ConsoleAdapter>();
services.AddSingleton<ChatBot>();
var serviceProvider = services.BuildServiceProvider();

var chatBot = serviceProvider.GetRequiredService<ChatBot>();
Task.WaitAll(chatBot.Run());
