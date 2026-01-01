using Telebot.Models;

namespace Telebot;

public interface ITelegramClient
{
    public Task<User> GetMeAsync(GetMeRequestParams requestParams);
    public Task<IReadOnlyList<Update>> GetUpdatesAsync(GetUpdatesRequestParams requestParams);
}

//Error
public class TelebotException(int? code, string message) : Exception(message)
{
    public int? Code { get; } =  code;
}

public partial class Telegram : ITelegramClient
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
        return await RawRequestAsync<User>(requestParams);
    }

    public async Task<IReadOnlyList<Update>> GetUpdatesAsync(GetUpdatesRequestParams requestParams)
    {
        return await RawRequestAsync<IReadOnlyList<Update>>(requestParams);
    }

    public async Task<Message> SendMessageAync(SendMessageRequestParams requestParams)
    {
        return await RawRequestAsync<Message>(requestParams);
    }

    public async Task<Message> SendPhotoAsync(SendPhotoRequestParams requestParams)
    {
        return await RawRequestAsync<Message>(requestParams);
    }
}