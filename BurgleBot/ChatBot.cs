using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using BurgleBot.IOAdapters;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;

namespace BurgleBot;

public class ChatBot(IIoAdapter ioAdapter, DefaultKernel kernel, SmartKernel smartKernel)
{
    [Experimental("SKEXP0110")]
    public async Task Run()
    {
        var agents = new Agents(smartKernel.Instance, kernel.Instance);
        var mainChatThread = new ChatHistoryAgentThread();
        
        var userInput = "Please create a prompt to extract all person names from a given text";

        var selectionFunction = AgentGroupChat.CreatePromptFunctionForStrategy(
            """
            Examine the provided response and choose the next participant. Return only the name
            of the participant, without any explanation, extra text of formatting. Never choose the participant
            named in the response.
            
            You may choose between those participants: PromptWriter, PromptChecker
            
            Always follow the rules:
            • If response is user input, it is PromptWriter’s turn
            
            Response:
            {{$lastmessage}}
            """, safeParameterNames: "lastmessage");
        
        const string TerminationToken = "PROMPT WORKS";
        ChatHistoryTruncationReducer historyReducer = new(1);
        
        var terminationFunction = AgentGroupChat.CreatePromptFunctionForStrategy(
                $$$"""
                    You are a result evaluator.
                    Rules:
                    - If the test result shows accuracy **above 90%** AND there are **no suggestions or improvements**, then set "shouldTerminate": true.
                    - Otherwise, set "shouldTerminate": false.

                    Respond with a valid JSON object with a single boolean field named shouldTerminate. Do not explain.

                    Example outputs:
                    { "shouldTerminate": true }
                    { "shouldTerminate": false }

                    RESPONSE:
                    {{$lastmessage}}
                    """,
                safeParameterNames: "lastmessage");

        var chat = new AgentGroupChat(agents.promptChecker, agents.promptWriter)
        {
            ExecutionSettings = new AgentGroupChatSettings
            {
                SelectionStrategy = new KernelFunctionSelectionStrategy(selectionFunction, kernel.Instance)
                {
                    InitialAgent = agents.promptWriter,
                    HistoryReducer = historyReducer,
                    HistoryVariableName = "lastmessage",
                    ResultParser = (result) => result.GetValue<string>() ?? agents.promptWriter.Name,
                },
                TerminationStrategy = new KernelFunctionTerminationStrategy(terminationFunction, kernel.Instance)
                {
                    Agents = [agents.promptChecker],
                    HistoryReducer = historyReducer,
                    HistoryVariableName = "lastmessage",
                    MaximumIterations = 12,
                    ResultParser = result =>
                    {
                        var raw = result.GetValue<string>()?.Trim();
                        Console.WriteLine($"[DEBUG] Termination function raw result: {raw}");

                        try
                        {
                            var parsed = JsonSerializer.Deserialize<JsonElement>(raw ?? "");
                            return parsed.TryGetProperty("shouldTerminate", out var val) && val.GetBoolean();
                        }
                        catch
                        {
                            return false; // If anything goes wrong, assume it's not done
                        }
                    }
                }
            }
        };
        var isComplete = false;
        chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, userInput));
        do
        {
            await foreach (var response in chat.InvokeAsync())
            {
                ioAdapter.SendMessageToUser($"""
                                             {response.AuthorName}:
                                             {response.Content}
                                             =========================================================================
                                             """);
            }
        } while (!chat.IsComplete);
    }
}
