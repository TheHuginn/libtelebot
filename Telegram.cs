using System.Text.Json;
using System.Text.Json.Serialization;
using Telebot.Models;
using File = Telebot.Models.File;

namespace Telebot;

/// <summary>
/// Общие настройки <see cref="JsonSerializer"/> для сериализации запросов.
/// <c>null</c>-значения не попадают в JSON — Telegram различает «поле не задано»
/// и «поле со значением по умолчанию».
/// </summary>
internal static class TelebotJson
{
    static TelebotJson()
    {
        Options.TypeInfoResolverChain.Insert(0, TelegramJsonSerializerContext.Default);
    }

    public static readonly JsonSerializerOptions Options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };
}

/// <summary>
/// Типобезопасный клиент Telegram Bot API: по одному методу на каждый endpoint.
/// </summary>
public interface ITelegramClient
{
    /// <summary>Возвращает информацию о самом боте (<c>getMe</c>).</summary>
    Task<User> GetMeAsync(GetMeRequestParams requestParams,
        CancellationToken cancellationToken);

    /// <summary>
    /// Long-polling получение апдейтов (<c>getUpdates</c>).
    /// Пустой список — за время таймаута ничего не пришло.
    /// </summary>
    Task<IReadOnlyList<Update>> GetUpdatesAsync(GetUpdatesRequestParams requestParams,
        CancellationToken cancellationToken);

    /// <summary>Отправляет текстовое сообщение (<c>sendMessage</c>).</summary>
    Task<Message> SendMessageAsync(SendMessageRequestParams requestParams,
        CancellationToken cancellationToken);

    /// <summary>Меняет текст ранее отправленного сообщения (<c>editMessageText</c>).</summary>
    Task<Message> EditMessageTextAsync(EditMessageTextRequestParams requestParams,
        CancellationToken cancellationToken);

    /// <summary>
    /// Меняет или снимает инлайн-клавиатуру ранее отправленного сообщения,
    /// не трогая его текст (<c>editMessageReplyMarkup</c>).
    /// </summary>
    Task<Message> EditMessageReplyMarkupAsync(EditMessageReplyMarkupRequestParams requestParams,
        CancellationToken cancellationToken);

    /// <summary>
    /// Подтверждает приём callback-запроса (<c>answerCallbackQuery</c>).
    /// Без этого вызова у пользователя на кнопке крутится индикатор загрузки.
    /// </summary>
    Task<bool> AnswerCallbackQueryAsync(AnswerCallbackQueryRequestParams requestParams,
        CancellationToken cancellationToken);

    /// <summary>Отправляет фотографию (<c>sendPhoto</c>).</summary>
    Task<Message> SendPhotoAsync(SendPhotoRequestParams requestParams,
        CancellationToken cancellationToken);

    /// <summary>Регистрирует webhook-URL для получения апдейтов (<c>setWebhook</c>).</summary>
    Task<bool> SetWebhookAsync(SetWebhookRequestParams requestParams,
        CancellationToken cancellationToken);

    /// <summary>Отправляет опрос (<c>sendPoll</c>).</summary>
    Task<Message> SendPollAsync(SendPollRequestParams requestParams,
        CancellationToken cancellationToken);

    /// <summary>Принудительно закрывает опрос и возвращает финальную статистику (<c>stopPoll</c>).</summary>
    Task<Poll> StopPollAsync(StopPollRequestParams requestParams,
        CancellationToken cancellationToken);

    /// <summary>
    /// Возвращает метаданные файла по <c>file_id</c> (<c>getFile</c>).
    /// Сам бинарник скачивается отдельно через <see cref="DownloadFileAsync"/>.
    /// </summary>
    Task<File> GetFileAsync(GetFileRequestParams requestParams,
        CancellationToken cancellationToken);

    /// <summary>
    /// Скачивает содержимое файла, полученного через <see cref="GetFileAsync"/>.
    /// Возвращённый поток нужно освободить (<see cref="IDisposable.Dispose"/>).
    /// </summary>
    Task<Stream> DownloadFileAsync(File file, CancellationToken cancellationToken);
}

/// <summary>
/// Единое исключение библиотеки для всех сбоев: сетевых, протокольных
/// и прикладных (Telegram вернул <c>ok=false</c>).
/// </summary>
/// <param name="code">HTTP-статус или <c>error_code</c> Telegram; <c>null</c>, если код неприменим.</param>
/// <param name="message">Описание ошибки.</param>
public class TelebotException(int? code, string message) : Exception(message)
{
    /// <summary>HTTP-статус или <c>error_code</c> Telegram. <c>null</c>, если ошибка без кода.</summary>
    public int? Code { get; } = code;
}

/// <summary>
/// Стандартная реализация <see cref="ITelegramClient"/> — тонкая обёртка
/// над <see cref="ITelegramTransport"/>.
/// </summary>
public sealed class Telegram : ITelegramClient
{
    private readonly string _token;
    private readonly ITelegramTransport _transport;

    /// <summary>Клиент со стандартным транспортом.</summary>
    /// <param name="token">Bot-токен из BotFather.</param>
    public Telegram(string token) : this(new DefaultTelegramTransport(), token) {}

    /// <summary>Клиент с явным транспортом — для тестов или нестандартных сценариев (прокси, свой <see cref="HttpClient"/>).</summary>
    /// <param name="transport">Реализация транспорта.</param>
    /// <param name="token">Bot-токен из BotFather.</param>
    public Telegram(ITelegramTransport transport, string token)
    {
        _transport = transport;
        _token = token;
    }

    /// <inheritdoc />
    public async Task<User> GetMeAsync(GetMeRequestParams requestParams,
        CancellationToken cancellationToken)
    {
        return await _transport.RequestAsync<User>(requestParams, _token, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Update>> GetUpdatesAsync(GetUpdatesRequestParams requestParams,
        CancellationToken cancellationToken)
    {
        return await _transport.RequestAsync<IReadOnlyList<Update>>(requestParams, _token, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Message> SendMessageAsync(SendMessageRequestParams requestParams,
        CancellationToken cancellationToken)
    {
        return await _transport.RequestAsync<Message>(requestParams, _token, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Message> EditMessageTextAsync(EditMessageTextRequestParams requestParams,
        CancellationToken cancellationToken)
    {
        return await _transport.RequestAsync<Message>(requestParams, _token, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Message> EditMessageReplyMarkupAsync(EditMessageReplyMarkupRequestParams requestParams,
        CancellationToken cancellationToken)
    {
        return await _transport.RequestAsync<Message>(requestParams, _token, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> AnswerCallbackQueryAsync(AnswerCallbackQueryRequestParams requestParams,
        CancellationToken cancellationToken)
    {
        return await _transport.RequestAsync<bool>(requestParams, _token, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Message> SendPhotoAsync(SendPhotoRequestParams requestParams,
        CancellationToken cancellationToken)
    {
        return await _transport.RequestAsync<Message>(requestParams, _token, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> SetWebhookAsync(SetWebhookRequestParams requestParams,
        CancellationToken cancellationToken)
    {
        return await _transport.RequestAsync<bool>(requestParams, _token, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Message> SendPollAsync(SendPollRequestParams requestParams,
        CancellationToken cancellationToken)
    {
        return await _transport.RequestAsync<Message>(requestParams, _token, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Poll> StopPollAsync(StopPollRequestParams requestParams,
        CancellationToken cancellationToken)
    {
        return await _transport.RequestAsync<Poll>(requestParams, _token, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<File> GetFileAsync(GetFileRequestParams requestParams,
        CancellationToken cancellationToken)
    {
        return await _transport.RequestAsync<File>(requestParams, _token, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Stream> DownloadFileAsync(File file, CancellationToken cancellationToken)
    {
        return await _transport.DownloadAsync(file, _token, cancellationToken);
    }
}

// Типы используемые внутри библиотеки при явной сериализации
[JsonSerializable(typeof(InputPollOption))]
[JsonSerializable(typeof(LinkPreviewOptions))]
[JsonSerializable(typeof(ReplyParameters))]
[JsonSerializable(typeof(ReplyKeyboardMarkup))]
[JsonSerializable(typeof(InlineKeyboardMarkup))]
[JsonSerializable(typeof(ForceReply))]
// Типы используемые для ответа, так как только он будет десериализовываться.
// Все типы должны быть явно перечислены, это отвественность разработчика.
[JsonSerializable(typeof(User))]
internal partial class TelegramJsonSerializerContext : JsonSerializerContext
{
}
