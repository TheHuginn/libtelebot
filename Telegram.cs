using Telebot.Models;

namespace Telebot;

public interface ITelegramClient
{
    Task<User> GetMeAsync(GetMeRequestParams requestParams);
    Task<IReadOnlyList<Update>> GetUpdatesAsync(GetUpdatesRequestParams requestParams);
    Task<Message> SendMessageAync(SendMessageRequestParams requestParams);
    Task<Message> SendPhotoAsync(SendPhotoRequestParams requestParams);
}

public class TelebotException(int? code, string message) : Exception(message)
{
    public int? Code { get; } = code;
}

public sealed partial class Telegram : ITelegramClient, IDisposable
{
    private readonly HttpClient _httpClient;
    private string token;


    public Telegram(string token)
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("https://api.telegram.org/bot" + token + "/");
        _httpClient.Timeout = new TimeSpan(0, 0, 30);
        this.token = token;
    }


    public async Task<User> GetMeAsync(GetMeRequestParams requestParams)
    {
        return await RequestAsync<User>(requestParams);
    }

    public async Task<IReadOnlyList<Update>> GetUpdatesAsync(GetUpdatesRequestParams requestParams)
    {
        return await RequestAsync<IReadOnlyList<Update>>(requestParams);
    }

    public async Task<Message> SendMessageAync(SendMessageRequestParams requestParams)
    {
        return await RequestAsync<Message>(requestParams);
    }

    public async Task<Message> SendPhotoAsync(SendPhotoRequestParams requestParams)
    {
        return await RequestAsync<Message>(requestParams);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}