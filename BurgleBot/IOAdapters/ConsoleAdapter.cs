namespace BurgleBot.IOAdapters;

public class ConsoleAdapter : IOAdapter
{
    public async Task<string> GetUserInput()
    {
        return Console.ReadLine() ?? string.Empty;
    }

    public async Task SendMessageToUser(string text)
    {
        Console.Write(text);
    }
}
