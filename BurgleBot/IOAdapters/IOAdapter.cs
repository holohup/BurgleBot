namespace BurgleBot.IOAdapters;

public interface IOAdapter
{
    public Task<string> GetUserInput();
    public Task SendMessageToUser(string text);
}