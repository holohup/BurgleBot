using BurgleBot;
using BurgleBot.IOAdapters;
using DotNetEnv.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;


var configuration = new ConfigurationBuilder()
    .AddDotNetEnv()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

var token = configuration["OPENAI_API_KEY"]!;
var modelId = configuration["MODEL"]!;
var builder = Kernel.CreateBuilder().AddOpenAIChatCompletion(modelId, token);
Kernel kernel = builder.Build();
var iOAdapter = new ConsoleAdapter();

var chatBot = new ChatBot(iOAdapter);
Task.WaitAll(chatBot.Chat(kernel));
