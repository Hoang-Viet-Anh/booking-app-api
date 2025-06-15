namespace booking_api.Interfaces;

public interface IAiService
{
    public Task<string> AskGroqAsync(string prompt);
}