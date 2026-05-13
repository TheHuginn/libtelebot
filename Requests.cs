using System.Text.Json;

namespace Telebot;

/// <summary>
/// Базовый тип для всех описаний запросов к Telegram Bot API.
/// Хранит имя метода (endpoint) и реализует <see cref="ITelegramEncodable"/>
/// пустыми коллекциями полей и файлов: наследники переопределяют только то,
/// что им действительно нужно передать.
/// </summary>
/// <remarks>
/// Архитектурное разделение: сам запрос знает <em>что</em> отправлять
/// (имя метода и параметры), а транспорт (<see cref="ITelegramTransport"/>)
/// знает <em>как</em> отправлять (form-urlencoded или multipart, HTTP-вызов,
/// разбор ответа). Это позволяет добавлять новые методы Telegram, не меняя
/// транспортный слой, и подменять транспорт в тестах, не меняя запросы.
/// </remarks>
/// <param name="Endpoint">
/// Имя метода Telegram Bot API (например, <c>sendMessage</c>),
/// которое попадает в URL запроса <c>/bot{token}/{endpoint}</c>.
/// </param>
public record TelegramRequest(string Endpoint) : ITelegramEncodable
{
    /// <summary>
    /// По умолчанию запрос не имеет скалярных параметров.
    /// Наследники переопределяют метод и через <c>yield return</c>
    /// возвращают только те поля, значения которых заданы.
    /// </summary>
    public virtual IEnumerable<TelegramRequestField> GetRequestFields()
    {
        return Enumerable.Empty<TelegramRequestField>();
    }

    /// <summary>
    /// По умолчанию запрос не передаёт файлов.
    /// Переопределяется только в тех методах, которые отправляют медиа
    /// или документы (например, <see cref="SendPhotoRequestParams"/>).
    /// </summary>
    public virtual IEnumerable<TelegramRequestFile> GetRequestFiles()
    {
        return Enumerable.Empty<TelegramRequestFile>();
    }
}

/// <summary>
/// Параметры вызова метода <c>getMe</c> — простейший запрос без аргументов,
/// возвращающий информацию о самом боте. Используется для проверки
/// валидности токена и доступности API.
/// </summary>
public sealed record GetMeRequestParams() : TelegramRequest("GetMe"), ITelegramEncodable;

/// <summary>
/// Параметры вызова метода <c>getUpdates</c> — long-polling получение
/// новых апдейтов с серверов Telegram. Все поля опциональны: если ни одно
/// не задано, Telegram вернёт все накопленные апдейты с настройками по умолчанию.
/// </summary>
/// <param name="Offset">
/// Идентификатор первого ожидаемого апдейта. Обычно равен <c>update_id</c>
/// последнего обработанного апдейта плюс один — так Telegram понимает,
/// что предыдущие можно считать подтверждёнными и удалить.
/// </param>
/// <param name="Limit">
/// Максимальное количество апдейтов в ответе (1–100, по умолчанию 100).
/// </param>
/// <param name="Timeout">
/// Таймаут long-polling в секундах: сколько ждать на стороне Telegram,
/// если новых апдейтов нет. 0 означает короткий polling.
/// </param>
/// <param name="AllowedUpdates">
/// Список типов апдейтов, которые нас интересуют (например, <c>message</c>,
/// <c>callback_query</c>). Сериализуется в JSON-массив, как требует API.
/// </param>
public sealed record GetUpdatesRequestParams(
    int? Offset = null,
    int? Limit = null,
    int? Timeout = null,
    IReadOnlyList<string>? AllowedUpdates = null
) : TelegramRequest("GetUpdates"), ITelegramEncodable
{
    /// <summary>
    /// Возвращает только заданные параметры: Telegram отличает «параметр не задан»
    /// от «параметр со значением по умолчанию», поэтому пропускать <c>null</c>
    /// принципиально — иначе можно случайно перезаписать настройки сервера.
    /// </summary>
    public override IEnumerable<TelegramRequestField> GetRequestFields()
    {
        if (Offset is not null)
            yield return new TelegramRequestField("offset", Offset.Value.ToString());
        if (Limit is not null)
            yield return new TelegramRequestField("limit", Limit.Value.ToString());
        if (Timeout is not null)
            yield return new TelegramRequestField("timeout", Timeout.Value.ToString());
        // allowed_updates ожидается Telegram именно как JSON-массив строк,
        // а не как form-поле со значением вида "a,b,c", поэтому сериализуем явно.
        if (AllowedUpdates is not null)
            yield return new TelegramRequestField("allowed_updates", JsonSerializer.Serialize(AllowedUpdates));
    }
}

/// <summary>
/// Параметры вызова метода <c>sendMessage</c> — отправка текстового сообщения
/// в чат. <see cref="ChatId"/> и <see cref="Text"/> обязательны, остальные
/// поля управляют форматированием, уведомлениями и привязкой к ответам.
/// </summary>
/// <param name="ChatId">Идентификатор чата-получателя (пользователь, группа или канал).</param>
/// <param name="Text">Текст сообщения, до 4096 символов после разбора форматирования.</param>
/// <param name="MessageThreadId">
/// Идентификатор треда внутри супергруппы с форумом — позволяет отправить
/// сообщение в конкретную тему.
/// </param>
/// <param name="ParseMode">
/// Режим разбора разметки в <see cref="Text"/>: <c>Markdown</c>, <c>MarkdownV2</c>
/// или <c>HTML</c>. Если не задан — текст уходит как plain text.
/// </param>
/// <param name="DisableNotification">
/// Если <c>true</c>, сообщение придёт без звука — полезно для фоновых уведомлений.
/// </param>
/// <param name="ProtectContent">
/// Если <c>true</c>, Telegram запретит пересылку и сохранение сообщения.
/// </param>
/// <param name="ReplyToMessageId">
/// Идентификатор сообщения, на которое отвечаем — в чате появится цитата-привязка.
/// </param>
/// <param name="AllowSendingWithoutReply">
/// Если <c>true</c>, сообщение всё равно будет отправлено, даже если
/// <see cref="ReplyToMessageId"/> указывает на удалённое сообщение.
/// </param>
public sealed record SendMessageRequestParams(
    long ChatId,
    string Text,
    int? MessageThreadId = null,
    string? ParseMode = null,
    bool? DisableNotification = null,
    bool? ProtectContent = null,
    int? ReplyToMessageId = null,
    bool? AllowSendingWithoutReply = null
) : TelegramRequest("sendMessage")
{
    /// <summary>
    /// Обязательные поля (chat_id, text) выдаются всегда; остальные —
    /// только если явно заданы. Булевы значения сериализуются как литералы
    /// <c>"true"</c>/<c>"false"</c>, как этого требует Bot API.
    /// </summary>
    public override IEnumerable<TelegramRequestField> GetRequestFields()
    {
        yield return new TelegramRequestField("chat_id", ChatId.ToString());
        yield return new TelegramRequestField("text", Text);

        if(MessageThreadId is not null)
            yield return new TelegramRequestField("message_thread_id", MessageThreadId.Value.ToString());
        if (ParseMode is not null)
            yield return new TelegramRequestField("parse_mode", ParseMode);
        if (DisableNotification is not null)
            yield return new TelegramRequestField("disable_notification",
                DisableNotification.Value ? "true" : "false");
        if (ProtectContent is not null)
            yield return new TelegramRequestField("protect_content",
                ProtectContent.Value ? "true" : "false");
        if (ReplyToMessageId is not null)
            yield return new TelegramRequestField("reply_to_message_id", ReplyToMessageId.Value.ToString());
        if (AllowSendingWithoutReply is not null)
            yield return new TelegramRequestField("allow_sending_without_reply",
                AllowSendingWithoutReply.Value ? "true" : "false");

    }
}

/// <summary>
/// Параметры вызова метода <c>sendPhoto</c> — отправка фотографии в чат.
/// В отличие от <see cref="SendMessageRequestParams"/>, помимо скалярных полей
/// передаёт ещё и файл-фото (<see cref="Photo"/>), поэтому переопределяет
/// и <see cref="GetRequestFields"/>, и <see cref="GetRequestFiles"/>.
/// </summary>
/// <param name="ChatId">Идентификатор чата-получателя.</param>
/// <param name="Photo">
/// Источник фотографии: <see cref="InputFileWithId"/>, <see cref="InputFileWithUrl"/>
/// или <see cref="InputFileWithStream"/>. От выбранного варианта зависит,
/// будет ли использоваться multipart-транспорт.
/// </param>
/// <param name="Caption">Подпись к фото, до 1024 символов.</param>
/// <param name="ParseMode">Режим разбора разметки в <see cref="Caption"/>.</param>
/// <param name="HasSpoiler">Если <c>true</c>, изображение придёт с эффектом «спойлер».</param>
/// <param name="DisableNotification">Отправка без звука.</param>
/// <param name="ProtectContent">Запрет на пересылку и сохранение.</param>
/// <param name="ReplyToMessageId">Идентификатор сообщения, на которое отвечаем.</param>
/// <param name="AllowSendingWithoutReply">
/// Разрешает отправку, даже если цель ответа удалена.
/// </param>
public sealed record SendPhotoRequestParams(
    long ChatId,
    InputFile Photo,
    string? Caption = null,
    string? ParseMode = null,
    bool? HasSpoiler = null,
    bool? DisableNotification = null,
    bool? ProtectContent = null,
    int? ReplyToMessageId = null,
    bool? AllowSendingWithoutReply = null
) : TelegramRequest("sendPhoto")
{
    /// <summary>
    /// Возвращает скалярные параметры. Сам файл <see cref="Photo"/> в этот
    /// набор не входит — он попадает в <see cref="GetRequestFiles"/>,
    /// откуда транспорт решит, как его кодировать.
    /// </summary>
    public override IEnumerable<TelegramRequestField> GetRequestFields()
    {
        yield return new TelegramRequestField("chat_id", ChatId.ToString());

        if (Caption is not null)
            yield return new TelegramRequestField("caption", Caption);

        if (ParseMode is not null)
            yield return new TelegramRequestField("parse_mode", ParseMode);

        if (HasSpoiler is not null)
            yield return new TelegramRequestField(
                "has_spoiler",
                HasSpoiler.Value ? "true" : "false"
            );

        if (DisableNotification is not null)
            yield return new TelegramRequestField(
                "disable_notification",
                DisableNotification.Value ? "true" : "false"
            );

        if (ProtectContent is not null)
            yield return new TelegramRequestField(
                "protect_content",
                ProtectContent.Value ? "true" : "false"
            );

        if (ReplyToMessageId is not null)
            yield return new TelegramRequestField(
                "reply_to_message_id",
                ReplyToMessageId.Value.ToString()
            );

        if (AllowSendingWithoutReply is not null)
            yield return new TelegramRequestField(
                "allow_sending_without_reply",
                AllowSendingWithoutReply.Value ? "true" : "false"
            );
    }

    /// <summary>
    /// Возвращает единственный файл с именем поля <c>photo</c> — именно так
    /// этот параметр ожидает Telegram Bot API. Если <see cref="Photo"/> является
    /// <see cref="InputFileWithStream"/>, транспорт автоматически выберет
    /// multipart-кодирование.
    /// </summary>
    public override IEnumerable<TelegramRequestFile> GetRequestFiles()
    {
        yield return new TelegramRequestFile("photo", Photo);
    }
}

/// <summary>
/// Параметры вызова метода <c>setWebhook</c> — регистрация webhook-URL,
/// на который Telegram будет POST-ить апдейты вместо ожидания long-polling.
/// </summary>
/// <param name="Url">HTTPS-URL, на который Telegram отправит апдейты. Обязательное поле.</param>
/// <param name="Certificate">
/// Самоподписанный публичный сертификат сервера. Передаётся только как поток,
/// поскольку Telegram должен получить непосредственно содержимое PEM-файла —
/// именно поэтому тип сужен до <see cref="InputFileWithStream"/>.
/// </param>
/// <param name="IpAddress">
/// Фиксированный IP, на который Telegram будет резолвить <see cref="Url"/>
/// — помогает обойти проблемы с DNS.
/// </param>
/// <param name="MaxConnections">
/// Максимум одновременных HTTPS-соединений для доставки апдейтов (1–100,
/// по умолчанию 40). Меньшее значение снижает нагрузку на сервер.
/// </param>
/// <param name="AllowedUpdates">
/// Список интересующих типов апдейтов; сериализуется в JSON-массив.
/// </param>
/// <param name="DropPendingUpdates">
/// Если <c>true</c>, Telegram сбросит все накопленные, но ещё не доставленные
/// апдейты — полезно при перезапуске бота с чистого листа.
/// </param>
/// <param name="SecretToken">
/// Произвольная строка, которую Telegram будет присылать в заголовке
/// <c>X-Telegram-Bot-Api-Secret-Token</c> — простой способ проверить,
/// что входящий запрос действительно от Telegram, а не от стороннего источника.
/// </param>
public sealed record SetWebhookRequestParams(
    string Url,
    InputFileWithStream? Certificate = null,
    string? IpAddress = null,
    int? MaxConnections = null,
    IReadOnlyList<string>? AllowedUpdates = null,
    bool? DropPendingUpdates = null,
    string? SecretToken = null
) : TelegramRequest("setWebhook")
{
    /// <summary>
    /// Возвращает скалярные поля webhook'а. <see cref="Certificate"/>
    /// сюда не попадает — это файл, он отдаётся через <see cref="GetRequestFiles"/>.
    /// </summary>
    public override IEnumerable<TelegramRequestField> GetRequestFields()
    {
        yield return new TelegramRequestField("url", Url);

        if (IpAddress is not null)
            yield return new TelegramRequestField("ip_address", IpAddress);

        if (MaxConnections is not null)
            yield return new TelegramRequestField(
                "max_connections",
                MaxConnections.Value.ToString()
            );

        if (AllowedUpdates is not null)
            yield return new TelegramRequestField(
                "allowed_updates",
                JsonSerializer.Serialize(AllowedUpdates)
            );

        if (DropPendingUpdates is not null)
            yield return new TelegramRequestField(
                "drop_pending_updates",
                DropPendingUpdates.Value ? "true" : "false"
            );

        if (SecretToken is not null)
            yield return new TelegramRequestField("secret_token", SecretToken);
    }

    /// <summary>
    /// Возвращает сертификат как файл с именем поля <c>certificate</c>,
    /// только если он задан. Наличие потока в файлах автоматически
    /// заставит транспорт использовать multipart/form-data.
    /// </summary>
    public override IEnumerable<TelegramRequestFile> GetRequestFiles()
    {
        if (Certificate is not null)
            yield return new TelegramRequestFile("certificate", Certificate);
    }
}