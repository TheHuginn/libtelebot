using System.Text.Json;
using System.Text.Json.Serialization;

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
/// Описание одного варианта ответа в опросе для метода <c>sendPoll</c>.
/// В полной спецификации Bot API у <c>InputPollOption</c> есть ещё
/// <c>text_parse_mode</c> и <c>text_entities</c> для форматирования подписи,
/// а также медиа-варианты — здесь они намеренно опущены, чтобы оставить
/// минимально необходимый набор: только текст варианта.
/// </summary>
/// <param name="Text">
/// Текст варианта ответа, 1–100 символов. Сериализуется в поле <c>text</c>
/// JSON-объекта, как этого ожидает Telegram.
/// </param>
public sealed record InputPollOption(
    [property: JsonPropertyName("text")] string Text
);

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
/// Параметры ответа на сообщение для метода <c>sendMessage</c> и других методов
/// отправки (см. <see href="https://core.telegram.org/bots/api#replyparameters"/>).
/// </summary>
/// <remarks>
/// Заменяет устаревшую пару <c>reply_to_message_id</c> + <c>allow_sending_without_reply</c>
/// (введено в Bot API 7.0, декабрь 2023). Позволяет не только сослаться на сообщение,
/// но и:
/// <list type="bullet">
///   <item><description>отвечать на сообщение из другого чата — через <see cref="ChatId"/>;</description></item>
///   <item><description>подсветить в цитате конкретный фрагмент — через <see cref="Quote"/> и <see cref="QuotePosition"/>.</description></item>
/// </list>
/// В Telegram Bot API этот объект передаётся как JSON-строка в поле формы
/// <c>reply_parameters</c> — сериализацию выполняет вызывающий запрос
/// (см. <see cref="SendMessageRequestParams.GetRequestFields"/>).
/// </remarks>
/// <param name="MessageId">
/// Идентификатор сообщения, на которое отвечаем в текущем чате
/// (или в <see cref="ChatId"/>, если он указан). Обязателен, если не задан
/// <see cref="EphemeralMessageId"/>.
/// </param>
/// <param name="ChatId">
/// Идентификатор чата, из которого сообщение-цель, если оно не в текущем чате.
/// В API поле принимает как число, так и строку <c>@username</c>; здесь оставлен
/// только числовой вариант ради консистентности с остальным API библиотеки.
/// </param>
/// <param name="EphemeralMessageId">
/// Идентификатор входящего эфемерного сообщения-цели в текущем чате.
/// Альтернатива <see cref="MessageId"/>: одно из двух обязано быть задано.
/// </param>
/// <param name="AllowSendingWithoutReply">
/// Если <c>true</c>, сообщение всё равно будет отправлено, даже если цель ответа
/// не найдена (удалена и т.п.).
/// </param>
/// <param name="Quote">
/// Цитируемый фрагмент исходного сообщения, 0–1024 символа после разбора форматирования.
/// </param>
/// <param name="QuoteParseMode">
/// Режим разбора разметки в <see cref="Quote"/>.
/// </param>
/// <param name="QuotePosition">
/// Позиция цитаты в исходном сообщении в UTF-16 code units.
/// </param>
/// <param name="ChecklistTaskId">
/// Идентификатор конкретной задачи чек-листа, на которую отвечаем.
/// </param>
/// <param name="PollOptionId">
/// Устойчивый идентификатор конкретного варианта опроса, на который отвечаем.
/// </param>
public sealed record ReplyParameters(
    [property: JsonPropertyName("message_id")] int? MessageId = null,
    [property: JsonPropertyName("chat_id")] long? ChatId = null,
    [property: JsonPropertyName("ephemeral_message_id")] int? EphemeralMessageId = null,
    [property: JsonPropertyName("allow_sending_without_reply")] bool? AllowSendingWithoutReply = null,
    [property: JsonPropertyName("quote")] string? Quote = null,
    [property: JsonPropertyName("quote_parse_mode")] string? QuoteParseMode = null,
    [property: JsonPropertyName("quote_position")] int? QuotePosition = null,
    [property: JsonPropertyName("checklist_task_id")] int? ChecklistTaskId = null,
    [property: JsonPropertyName("poll_option_id")] string? PollOptionId = null
);

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
/// <param name="ReplyParameters">
/// Параметры ответа на другое сообщение (см. <see cref="Telebot.ReplyParameters"/>).
/// Пришли на смену устаревшим <c>reply_to_message_id</c> и
/// <c>allow_sending_without_reply</c> в Bot API 7.0.
/// </param>
/// <param name="ReplyMarkup">
/// Разметка под сообщением: инлайн-клавиатура, обычная клавиатура,
/// её удаление или force-reply (см. <see cref="IReplyMarkup"/>). Union без явного
/// тега — Telegram распознаёт вариант по маркерному полю в JSON,
/// поэтому сериализацию каждой реализации знает она сама через
/// <see cref="IReplyMarkup.ToJson"/>.
/// </param>
public sealed record SendMessageRequestParams(
    long ChatId,
    string Text,
    int? MessageThreadId = null,
    string? ParseMode = null,
    bool? DisableNotification = null,
    bool? ProtectContent = null,
    ReplyParameters? ReplyParameters = null,
    IReplyMarkup? ReplyMarkup = null
) : TelegramRequest("sendMessage")
{
    /// <summary>
    /// Обязательные поля (chat_id, text) выдаются всегда; остальные —
    /// только если явно заданы. Булевы значения сериализуются как литералы
    /// <c>"true"</c>/<c>"false"</c>, как этого требует Bot API.
    /// <see cref="ReplyParameters"/> и <see cref="ReplyMarkup"/> уходят как
    /// JSON-строки: Telegram ожидает именно такое представление составных
    /// объектов в form-запросе.
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
        if (ReplyParameters is not null)
            yield return new TelegramRequestField("reply_parameters",
                JsonSerializer.Serialize(ReplyParameters));
        if (ReplyMarkup is not null)
            yield return new TelegramRequestField("reply_markup", ReplyMarkup.ToJson());
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

/// <summary>
/// Параметры вызова метода <c>sendPoll</c> — отправка опроса в чат.
/// Здесь оставлены только три обязательных поля Bot API: чат-получатель,
/// текст вопроса и список вариантов ответа. Опциональные параметры
/// (анонимность, множественный выбор, режим викторины, объяснение,
/// таймеры закрытия и т.п.) намеренно опущены — их можно добавить
/// в отдельной перегрузке, не ломая базовый сценарий.
/// </summary>
/// <param name="ChatId">Идентификатор чата-получателя (пользователь, группа или канал).</param>
/// <param name="Question">Текст вопроса, 1–300 символов.</param>
/// <param name="Options">
/// Список вариантов ответа (2–10 элементов). Передаётся в API как JSON-массив
/// объектов <see cref="InputPollOption"/>, поэтому сериализуется явно через
/// <see cref="JsonSerializer"/>, а не как form-поле «через запятую».
/// </param>
public sealed record SendPollRequestParams(
    long ChatId,
    string Question,
    IReadOnlyList<InputPollOption> Options
) : TelegramRequest("sendPoll")
{
    /// <summary>
    /// Все три поля обязательны, поэтому выдаются безусловно.
    /// <c>options</c> уходит именно как JSON-массив: Telegram не принимает
    /// его в form-urlencoded виде со скалярным значением.
    /// </summary>
    public override IEnumerable<TelegramRequestField> GetRequestFields()
    {
        yield return new TelegramRequestField("chat_id", ChatId.ToString());
        yield return new TelegramRequestField("question", Question);
        yield return new TelegramRequestField("options", JsonSerializer.Serialize(Options));
    }
}