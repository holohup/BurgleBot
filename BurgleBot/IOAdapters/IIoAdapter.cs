namespace BurgleBot.IOAdapters;

public interface IIoAdapter
{
    public Task<string> GetUserInput();
    public Task SendMessageToUser(string? text);
}