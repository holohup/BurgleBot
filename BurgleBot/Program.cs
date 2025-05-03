using System.Text;
using System.Text.Json;
using System.Text.Json.Schema;
using OllamaSharp;
using OllamaSharp.Models;

var uri = new Uri("http://192.168.2.32:11434");
var ollama = new OllamaApiClient(uri);
ollama.SelectedModel = "mistral:7b";

var request = new GenerateRequest
{
    Prompt = "When Venya was 3 years old, I was three times older. How old am I now if Venya is 30?",
    Format = JsonSerializerOptions.Default.GetJsonSchemaAsNode(typeof(Response)),
    Options = new RequestOptions {Temperature = 0}
};

var result = new StringBuilder();

await foreach (var stream in ollama.GenerateAsync(request))
{
    result.Append(stream.Response);
    Console.Write(stream.Response);
}

// var response = JsonSerializer.Deserialize<Response>(result.ToString());


internal record Response
{
    public List<Step> Steps { get; set; }
    public string FinalAnswer { get; init; }
}

internal record Step
{
    public string Explanation {get; init; }
    public string Result {get; init; }
}
