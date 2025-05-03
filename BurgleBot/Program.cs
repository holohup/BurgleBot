using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

var kernel = Kernel.CreateBuilder().AddOpenAIChatCompletion(
        serviceId: "chat",
        modelId: "ai/llama3.1:8B-Q4_K_M",
        endpoint: new Uri("http://localhost:12434/engines/v1"),
        apiKey: ""
    ).Build();

var requestSettings = new OpenAIPromptExecutionSettings()
{
    // ResponseFormat = typeof(Response),
    Temperature = 0.1
};

var history = new ChatHistory();
Console.Write("User: ");
var question = Console.ReadLine();
history.AddUserMessage(question);

var chat = kernel.GetRequiredService<IChatCompletionService>();

var tokenStream = chat.GetStreamingChatMessageContentsAsync(history, requestSettings);


await foreach (var token in tokenStream)
{
    Console.Write(token);
}


public record Response
{
    public string Answer { get; init; }
}



// public record Response
// {
//     public List<Step> Steps { get; init; }
//     [Description("Response to users question")]
//     public string FinalAnswer { get; init; }
// }

public record Step
{
    public string Explanation {get; init; }
    public string Result {get; init; }
}


//
// public record Response
// {
//     [Description("Age difference in years between the user and Venya")]
//     public int AgeDifference { get; init; }
//
//     [Description("Years passed since that time")]
//     public int YearsPassed { get; init; }
//     
//     [Description("Answer to the user’s question")]
//     public string FinalAnswer { get; init; }
// }

// var responseSchemaJson = """
//                         {
//                           "type": "object",
//                           "properties": {
//                             "myStringField": { "type": "string" },
//                             "myIntField":    { "type": "integer" }
//                           },
//                           "required": ["myStringField", "myIntField"],
//                           "additionalProperties": false
//                         }
//                         """;
//
// var responseSchemaElement = JsonDocument.Parse(responseSchemaJson).RootElement;
//
// ChatResponseFormatJson chatResponseFormat = ChatResponseFormatJson.ForJsonSchema(responseSchemaElement);
