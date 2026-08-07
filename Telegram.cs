using System.Text.Json;
using System.Text.Json.Serialization;
using Telebot.Models;

namespace Telebot;

/// <summary>
/// Единая конфигурация <see cref="JsonSerializer"/> для всей библиотеки.
/// Используется во всех местах, где составные объекты запроса сериализуются
/// в JSON-строку перед укладкой в поле формы Telegram Bot API.
/// </summary>
/// <remarks>
/// Ключевая настройка — <see cref="JsonIgnoreCondition.WhenWritingNull"/>:
/// свойства со значением <c>null</c> не попадают в итоговый JSON. Telegram
/// различает «поле не задано» и «поле со значением по умолчанию», поэтому
/// опциональные незаданные поля нельзя выдавать явно как <c>"field": null</c>
/// — в союз-типах (<c>reply_markup</c>) это ломает распознавание варианта,
/// а в остальных запросах даёт лишний шум в теле и в логах.
/// <para/>
/// Класс <c>internal</c>: наружу опции не торчат, никакого API-сюрприза
/// для потребителя библиотеки не создают.
/// </remarks>
internal static class TelebotJson
{
    /// <summary>
    /// Готовый набор опций для передачи вторым аргументом в
    /// <see cref="JsonSerializer.Serialize{TValue}(TValue, JsonSerializerOptions?)"/>.
    /// Экземпляр один на процесс и потокобезопасен — <see cref="JsonSerializerOptions"/>
    /// после первого использования кешируется и переиспользуется.
    /// </summary>
    public static readonly JsonSerializerOptions Options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };
}

/// <summary>
/// Высокоуровневый контракт клиента Telegram Bot API: предоставляет
/// типобезопасные методы для конкретных endpoint'ов вместо «универсального»
/// вызова с произвольным именем метода.
/// </summary>
/// <remarks>
/// Слой ответственности: <see cref="ITelegramClient"/> описывает <em>что</em>
/// бот умеет делать в терминах прикладной модели (получить пользователя,
/// отправить сообщение и т.п.), а <see cref="ITelegramTransport"/> отвечает
/// за <em>как</em> — формирование HTTP-запроса и разбор ответа.
/// Такое разделение позволяет тестировать прикладную логику без сети
/// (подмена транспорта моком) и развивать API независимо от транспорта.
/// </remarks>
public interface ITelegramClient
{
    /// <summary>
    /// Вызывает метод <c>getMe</c> — возвращает информацию о самом боте.
    /// Стандартный способ убедиться, что токен валиден и API доступен.
    /// </summary>
    Task<User> GetMeAsync(GetMeRequestParams requestParams,
        CancellationToken cancellationToken);

    /// <summary>
    /// Вызывает метод <c>getUpdates</c> — long-polling получение новых апдейтов.
    /// Возвращает упорядоченный список <see cref="Update"/>; пустой список
    /// означает, что за время <c>timeout</c> ничего нового не пришло.
    /// </summary>
    Task<IReadOnlyList<Update>> GetUpdatesAsync(GetUpdatesRequestParams requestParams,
        CancellationToken cancellationToken);

    /// <summary>
    /// Вызывает метод <c>sendMessage</c> — отправляет текстовое сообщение
    /// и возвращает уже отправленное <see cref="Message"/> с проставленным
    /// сервером <c>message_id</c> и временем.
    /// </summary>
    Task<Message> SendMessageAsync(SendMessageRequestParams requestParams,
        CancellationToken cancellationToken);

    /// <summary>
    /// Вызывает метод <c>editMessageText</c> — заменяет текст (и, опционально,
    /// инлайн-клавиатуру) ранее отправленного сообщения. Возвращает уже
    /// изменённое <see cref="Message"/>.
    /// </summary>
    Task<Message> EditMessageTextAsync(EditMessageTextRequestParams requestParams,
        CancellationToken cancellationToken);

    /// <summary>
    /// Вызывает метод <c>editMessageReplyMarkup</c> — меняет или снимает
    /// инлайн-клавиатуру у ранее отправленного сообщения, не трогая его текст.
    /// Возвращает уже изменённое <see cref="Message"/>.
    /// </summary>
    Task<Message> EditMessageReplyMarkupAsync(EditMessageReplyMarkupRequestParams requestParams,
        CancellationToken cancellationToken);

    /// <summary>
    /// Вызывает метод <c>answerCallbackQuery</c> — подтверждает приём
    /// callback-запроса от нажатия кнопки. Возвращает <c>true</c> при успехе.
    /// Без этого вызова у пользователя на кнопке продолжает крутиться
    /// индикатор загрузки.
    /// </summary>
    Task<bool> AnswerCallbackQueryAsync(AnswerCallbackQueryRequestParams requestParams,
        CancellationToken cancellationToken);

    /// <summary>
    /// Вызывает метод <c>sendPhoto</c> — отправляет фотографию.
    /// Способ задания файла (file_id, URL или поток) выбирается в
    /// <see cref="SendPhotoRequestParams.Photo"/>.
    /// </summary>
    Task<Message> SendPhotoAsync(SendPhotoRequestParams requestParams,
        CancellationToken cancellationToken);

    /// <summary>
    /// Вызывает метод <c>setWebhook</c> — регистрирует HTTPS-URL,
    /// на который Telegram будет POST-ить апдейты вместо long-polling.
    /// Возвращает <c>true</c> при успешной регистрации.
    /// </summary>
    Task<bool> SetWebhookAsync(SetWebhookRequestParams requestParams,
        CancellationToken cancellationToken);
    
    /// <summary>
    /// Вызывает метод <c>sendPoll</c> — отправляет опрос с двумя-десятью
    /// вариантами ответа. Возвращает уже отправленное <see cref="Message"/>
    /// с проставленным сервером <c>message_id</c>; сам опрос доступен
    /// через <see cref="Message.Poll"/>.
    /// </summary>
    Task<Message> SendPollAsync(SendPollRequestParams requestParams,
        CancellationToken cancellationToken);

    /// <summary>
    /// Вызывает метод <c>stopPoll</c> — принудительно закрывает опрос,
    /// ранее отправленный ботом, и возвращает уже закрытый <see cref="Poll"/>
    /// со всей финальной статистикой голосов.
    /// </summary>
    Task<Poll> StopPollAsync(StopPollRequestParams requestParams,
        CancellationToken cancellationToken);
}

/// <summary>
/// Единое исключение библиотеки, к которому сводятся все сбои:
/// сетевые (нет связи, не-2xx HTTP), протокольные (пустой/некорректный JSON)
/// и прикладные (Telegram вернул <c>ok=false</c> с описанием).
/// </summary>
/// <remarks>
/// Преимущество единого типа: вызывающий код может ловить одно исключение
/// и при необходимости различать причины по <see cref="Code"/> —
/// HTTP-код или <c>error_code</c> от Telegram, в зависимости от стадии,
/// на которой произошёл сбой. <c>null</c> в <see cref="Code"/> означает,
/// что ошибка не связана с конкретным числовым кодом (например, нарушение
/// инварианта протокола или сбой десериализации).
/// </remarks>
/// <param name="code">HTTP-статус или <c>error_code</c> Telegram; <c>null</c>, если код неприменим.</param>
/// <param name="message">Человекочитаемое описание ошибки.</param>
public class TelebotException(int? code, string message) : Exception(message)
{
    /// <summary>
    /// Числовой код ошибки: либо HTTP-статус (для транспортных сбоев),
    /// либо поле <c>error_code</c> из ответа Telegram (для прикладных).
    /// <c>null</c>, если ошибка не привязана к коду.
    /// </summary>
    public int? Code { get; } = code;
}

/// <summary>
/// Стандартная реализация <see cref="ITelegramClient"/>: тонкая обёртка
/// над <see cref="ITelegramTransport"/>, фиксирующая для каждого endpoint
/// его типизированный результат.
/// </summary>
/// <remarks>
/// Каждый публичный метод сводится к одному вызову
/// <see cref="ITelegramTransport.RequestAsync{T}"/> с подставленным <c>T</c>
/// — таким образом тип результата (например, <see cref="User"/> для <c>getMe</c>
/// или <see cref="Message"/> для <c>sendMessage</c>) проверяется компилятором,
/// а не приводится вручную в месте вызова.
/// </remarks>
public sealed class Telegram : ITelegramClient
{
    private readonly string _token;
    private readonly ITelegramTransport _transport;

    /// <summary>
    /// Удобный конструктор для типичного сценария: создаёт клиент со
    /// стандартным транспортом <see cref="DefaultTelegramTransport"/>.
    /// </summary>
    /// <param name="token">Bot-токен, выданный BotFather.</param>
    public Telegram(string token) : this(new DefaultTelegramTransport(), token) {}

    /// <summary>
    /// Конструктор с явной подменой транспорта — нужен для тестов
    /// (мок транспорта) и для нестандартных сценариев (например, прокси,
    /// собственный <see cref="HttpClient"/> с другим таймаутом).
    /// </summary>
    /// <param name="transport">Реализация транспортного слоя.</param>
    /// <param name="token">Bot-токен, выданный BotFather.</param>
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
}