namespace BurgleBot.IOAdapters;

public class ConsoleAdapter : IIoAdapter
{
    public async Task<string> GetUserInput()
    {
        Console.Write("User: ");
        return Console.ReadLine() ?? string.Empty;
    }

    public async Task SendMessageToUser(string? text)
    {
        Console.WriteLine("Assistant: " + text);
    }
}
