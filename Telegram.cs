using Telebot.Models;

namespace Telebot;

public interface ITelegramClient
{
    Task<User> GetMeAsync(GetMeRequestParams requestParams, 
        CancellationToken cancellationToken);
    Task<IReadOnlyList<Update>> GetUpdatesAsync(GetUpdatesRequestParams requestParams,
        CancellationToken cancellationToken);
    Task<Message> SendMessageAsync(SendMessageRequestParams requestParams, 
        CancellationToken cancellationToken);
    Task<Message> SendPhotoAsync(SendPhotoRequestParams requestParams, 
        CancellationToken cancellationToken);
    Task<bool> SetWebhookAsync(SetWebhookRequestParams requestParams, 
        CancellationToken cancellationToken);
}

public class TelebotException(int? code, string message) : Exception(message)
{
    public int? Code { get; } = code;
}

public sealed class Telegram : ITelegramClient
{
    private readonly string _token;
    private readonly ITelegramTransport _transport;

    public Telegram(string token) : this(new DefaultTelegramTransport(), token) {}

    public Telegram(ITelegramTransport transport, string token)
    {
        _transport = transport;
        _token = token;
    }

    public async Task<User> GetMeAsync(GetMeRequestParams requestParams, 
        CancellationToken cancellationToken)
    {
        return await _transport.RequestAsync<User>(requestParams, _token, cancellationToken);
    }

    public async Task<IReadOnlyList<Update>> GetUpdatesAsync(GetUpdatesRequestParams requestParams,
        CancellationToken cancellationToken)
    {
        return await _transport.RequestAsync<IReadOnlyList<Update>>(requestParams, _token, cancellationToken);
    }

    public async Task<Message> SendMessageAsync(SendMessageRequestParams requestParams,
        CancellationToken cancellationToken)
    {
        return await _transport.RequestAsync<Message>(requestParams, _token, cancellationToken);
    }

    public async Task<Message> SendPhotoAsync(SendPhotoRequestParams requestParams, 
        CancellationToken cancellationToken)
    {
        return await _transport.RequestAsync<Message>(requestParams, _token, cancellationToken);
    }

    public async Task<bool> SetWebhookAsync(SetWebhookRequestParams requestParams, 
        CancellationToken cancellationToken)
    {
        return await _transport.RequestAsync<bool>(requestParams, _token, cancellationToken);
    }
}