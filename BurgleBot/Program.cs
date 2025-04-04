using BurgleBot;
using BurgleBot.IOAdapters;
using DotNetEnv.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;


var configuration = new ConfigurationBuilder()
    .AddDotNetEnv()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

var token = configuration["OPENAI_API_KEY"]!;
var modelId = configuration["MODEL"]!;
var builder = Kernel.CreateBuilder().AddOpenAIChatCompletion(modelId, token);
Kernel kernel = builder.Build();
var services = new ServiceCollection();
services.AddSingleton(kernel);
services.AddSingleton<IOAdapter, ConsoleAdapter>();
services.AddSingleton<ChatBot>();

var serviceProvider = services.BuildServiceProvider();

var chatBot = serviceProvider.GetRequiredService<ChatBot>();
Task.WaitAll(chatBot.Run());
