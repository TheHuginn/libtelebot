using System.Runtime.CompilerServices;
using Telebot.Models;

namespace Telebot;

public interface ITelegramClient
{
    Task<User> GetMeAsync(GetMeRequestParams requestParams);
    Task<IReadOnlyList<Update>> GetUpdatesAsync(GetUpdatesRequestParams requestParams);
    Task<Message> SendMessageAsync(SendMessageRequestParams requestParams);
    Task<Message> SendPhotoAsync(SendPhotoRequestParams requestParams);
}

public class TelebotException(int? code, string message) : Exception(message)
{
    public int? Code { get; } = code;
}

public sealed partial class Telegram : ITelegramClient
{
    private string _token;
    private ITelegramTransport _transport;
    
    public Telegram(string token) : this(new DefaultTelegramTransport(), token){}

    internal Telegram(ITelegramTransport transport, string token)
    {
        _transport = transport;
        _token = token;
    }
    
    public async Task<User> GetMeAsync(GetMeRequestParams requestParams)
    {
        return await _transport.RequestAsync<User>(requestParams, _token);
    }

    public async Task<IReadOnlyList<Update>> GetUpdatesAsync(GetUpdatesRequestParams requestParams)
    {
        return await _transport.RequestAsync<IReadOnlyList<Update>>(requestParams, _token);
    }

    public async Task<Message> SendMessageAsync(SendMessageRequestParams requestParams)
    {
        return await _transport.RequestAsync<Message>(requestParams, _token);
    }

    public async Task<Message> SendPhotoAsync(SendPhotoRequestParams requestParams)
    {
        return await _transport.RequestAsync<Message>(requestParams, _token);
    }
}