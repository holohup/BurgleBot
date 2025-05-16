using BurgleBot;
using BurgleBot.IOAdapters;
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
var smartModelId = configuration["SMART_MODEL"]!;
var notSoSmartModelId = configuration["NOT_SO_SMART_MODEL"]!;
var builder = Kernel.CreateBuilder().AddOpenAIChatCompletion(modelId, token);
builder.Services.AddLogging(configure => configure.AddConsole());
builder.Services.AddLogging(configure => configure.SetMinimumLevel(LogLevel.Information));
Kernel kernel = builder.Build();

var smartBuilder = Kernel.CreateBuilder().AddOpenAIChatCompletion(smartModelId, token);
smartBuilder.Services.AddLogging(configure => configure.AddConsole());
smartBuilder.Services.AddLogging(configure => configure.SetMinimumLevel(LogLevel.Information));
Kernel smartKernel = smartBuilder.Build();

var notSmartBuilder = Kernel.CreateBuilder().AddOpenAIChatCompletion(notSoSmartModelId, token);
notSmartBuilder.Services.AddLogging(configure => configure.AddConsole());
notSmartBuilder.Services.AddLogging(configure => configure.SetMinimumLevel(LogLevel.Information));
Kernel notSmartKernel = notSmartBuilder.Build();

var services = new ServiceCollection();
services.AddSingleton(new DefaultKernel(kernel));
services.AddSingleton(new SmartKernel(smartKernel));
services.AddSingleton(new NotSoSmartKernel(notSmartKernel));
services.AddSingleton<IIoAdapter, ConsoleAdapter>();
services.AddSingleton<ChatBot>();
var serviceProvider = services.BuildServiceProvider();

var chatBot = serviceProvider.GetRequiredService<ChatBot>();
#pragma warning disable SKEXP0110
Task.WaitAll(chatBot.Run());
#pragma warning restore SKEXP0110


public class DefaultKernel(Kernel kernel)
{
    public Kernel Instance { get; } = kernel;
}

public class SmartKernel(Kernel kernel)
{
    public Kernel Instance { get; } = kernel;
}

public class NotSoSmartKernel(Kernel kernel)
{
    public Kernel Instance { get; } = kernel;
}