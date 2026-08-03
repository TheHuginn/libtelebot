using System.Text.Json;
using System.Text.Json.Serialization;

namespace Telebot;

/// <summary>
/// Общий контракт для всех вариантов поля <c>reply_markup</c> в методах отправки
/// сообщений (см. <see href="https://core.telegram.org/bots/api#sendmessage"/>).
/// </summary>
/// <remarks>
/// В Bot API <c>reply_markup</c> — это единый параметр, принимающий один из
/// четырёх типов (<see cref="InlineKeyboardMarkup"/>, <see cref="ReplyKeyboardMarkup"/>,
/// <see cref="ReplyKeyboardRemove"/>, <see cref="ForceReply"/>), а Telegram различает
/// их по наличию «маркерных» полей (<c>inline_keyboard</c>, <c>keyboard</c>,
/// <c>remove_keyboard</c>, <c>force_reply</c>). Явного тега нет — union без тега.
/// <para/>
/// Поэтому в C# мы отдаём наружу общий интерфейс с одним методом
/// <see cref="ToJson"/>: каждая реализация сама знает, как выглядит её
/// JSON-представление, а запрос-отправитель просто вкладывает результат
/// в поле формы <c>reply_markup</c>.
/// </remarks>
public interface IReplyMarkup
{
    /// <summary>
    /// Возвращает готовую JSON-строку в том виде, в котором её ожидает Telegram
    /// в поле формы <c>reply_markup</c>. Реализация обязана включить своё
    /// маркерное поле — без него Telegram не сможет распознать тип разметки.
    /// </summary>
    string ToJson();
}

/// <summary>
/// Общие настройки сериализации всех вариантов <see cref="IReplyMarkup"/>:
/// null-поля не попадают в JSON. Telegram отличает «поле не задано» от
/// «поле со значением по умолчанию», поэтому опциональные незаданные поля
/// нельзя выдавать явно — иначе можно случайно перезаписать поведение сервера.
/// </summary>
internal static class ReplyMarkupJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };
}

/// <summary>
/// Кнопка инлайн-клавиатуры (см. <see href="https://core.telegram.org/bots/api#inlinekeyboardbutton"/>).
/// </summary>
/// <remarks>
/// В полной спецификации у <c>InlineKeyboardButton</c> ~10 полей (web_app,
/// login_url, switch_inline_query, pay и т.д.). Здесь оставлен минимально
/// необходимый набор — обычный текст, ссылка и callback_data, покрывающие
/// подавляющее большинство сценариев. Остальные поля можно добавлять по мере
/// появления реальных потребностей.
/// </remarks>
/// <param name="Text">Текст на кнопке, обязателен.</param>
/// <param name="Url">HTTP/tg-URL, который откроется при нажатии.</param>
/// <param name="CallbackData">
/// Данные, которые прилетят обратно как <c>callback_query</c> при нажатии,
/// 1–64 байта UTF-8. Используется для обработки нажатия ботом.
/// </param>
public sealed record InlineKeyboardButton(
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("url")] string? Url = null,
    [property: JsonPropertyName("callback_data")] string? CallbackData = null
);

/// <summary>
/// Инлайн-клавиатура, отображаемая под сообщением
/// (см. <see href="https://core.telegram.org/bots/api#inlinekeyboardmarkup"/>).
/// </summary>
/// <param name="InlineKeyboard">
/// Двумерный массив кнопок: внешний уровень — ряды, внутренний — кнопки в ряду.
/// Именно наличие поля <c>inline_keyboard</c> в JSON отличает этот вариант
/// разметки от остальных.
/// </param>
public sealed record InlineKeyboardMarkup(
    [property: JsonPropertyName("inline_keyboard")]
    IReadOnlyList<IReadOnlyList<InlineKeyboardButton>> InlineKeyboard
) : IReplyMarkup
{
    /// <inheritdoc />
    public string ToJson() => JsonSerializer.Serialize(this, ReplyMarkupJson.Options);
}

/// <summary>
/// Кнопка обычной (reply) клавиатуры
/// (см. <see href="https://core.telegram.org/bots/api#keyboardbutton"/>).
/// </summary>
/// <remarks>
/// В полной спецификации у <c>KeyboardButton</c> есть ещё поля request_contact,
/// request_location, request_poll и т.п. Здесь оставлен минимум — только текст.
/// </remarks>
/// <param name="Text">
/// Текст на кнопке. Нажатие отправит в чат сообщение с этим же текстом.
/// </param>
public sealed record KeyboardButton(
    [property: JsonPropertyName("text")] string Text
);

/// <summary>
/// Обычная (не инлайновая) клавиатура, заменяющая клавиатуру устройства
/// (см. <see href="https://core.telegram.org/bots/api#replykeyboardmarkup"/>).
/// </summary>
/// <param name="Keyboard">
/// Двумерный массив кнопок: внешний уровень — ряды, внутренний — кнопки в ряду.
/// Наличие поля <c>keyboard</c> в JSON — маркер, по которому Telegram
/// распознаёт этот тип разметки.
/// </param>
/// <param name="IsPersistent">
/// Если <c>true</c>, клавиатура остаётся видимой даже когда стандартная
/// клавиатура ввода была бы скрыта.
/// </param>
/// <param name="ResizeKeyboard">
/// Если <c>true</c>, клиент Telegram уменьшит высоту клавиатуры до необходимой.
/// </param>
/// <param name="OneTimeKeyboard">
/// Если <c>true</c>, клавиатура скроется сразу после первого нажатия.
/// </param>
/// <param name="InputFieldPlaceholder">
/// Placeholder в поле ввода, пока клавиатура активна, 1–64 символа.
/// </param>
/// <param name="Selective">
/// Если <c>true</c>, клавиатура покажется только упомянутым в тексте
/// пользователям и/или автору сообщения, на которое отвечает бот.
/// </param>
public sealed record ReplyKeyboardMarkup(
    [property: JsonPropertyName("keyboard")]
    IReadOnlyList<IReadOnlyList<KeyboardButton>> Keyboard,
    [property: JsonPropertyName("is_persistent")]
    bool? IsPersistent = null,
    [property: JsonPropertyName("resize_keyboard")]
    bool? ResizeKeyboard = null,
    [property: JsonPropertyName("one_time_keyboard")]
    bool? OneTimeKeyboard = null,
    [property: JsonPropertyName("input_field_placeholder")]
    string? InputFieldPlaceholder = null,
    [property: JsonPropertyName("selective")]
    bool? Selective = null
) : IReplyMarkup
{
    /// <inheritdoc />
    public string ToJson() => JsonSerializer.Serialize(this, ReplyMarkupJson.Options);
}

/// <summary>
/// Указание клиенту убрать обычную (reply) клавиатуру
/// (см. <see href="https://core.telegram.org/bots/api#replykeyboardremove"/>).
/// </summary>
/// <param name="Selective">
/// Если <c>true</c>, клавиатура будет убрана только у выделенного набора
/// пользователей (упомянутых в тексте / автора reply-цели).
/// </param>
public sealed record ReplyKeyboardRemove(
    bool? Selective = null
) : IReplyMarkup
{
    /// <inheritdoc />
    /// <remarks>
    /// Маркерное поле <c>remove_keyboard: true</c> добавляется здесь, а не в
    /// теле record: значение всегда фиксировано, хранить его в объекте не имеет
    /// смысла. Telegram распознаёт вариант именно по этому полю.
    /// </remarks>
    public string ToJson() => JsonSerializer.Serialize(new
    {
        remove_keyboard = true,
        selective = Selective,
    }, ReplyMarkupJson.Options);
}

/// <summary>
/// Указание клиенту сразу открыть ответ на сообщение бота
/// (см. <see href="https://core.telegram.org/bots/api#forcereply"/>).
/// </summary>
/// <param name="InputFieldPlaceholder">
/// Placeholder в поле ввода при force-reply, 1–64 символа.
/// </param>
/// <param name="Selective">
/// Если <c>true</c>, force-reply сработает только для выделенных пользователей.
/// </param>
public sealed record ForceReply(
    string? InputFieldPlaceholder = null,
    bool? Selective = null
) : IReplyMarkup
{
    /// <inheritdoc />
    /// <remarks>
    /// Как и в <see cref="ReplyKeyboardRemove"/>, маркерное поле
    /// <c>force_reply: true</c> добавляется на этапе сериализации:
    /// его значение всегда одинаково и в объекте хранить его смысла нет.
    /// </remarks>
    public string ToJson() => JsonSerializer.Serialize(new
    {
        force_reply = true,
        input_field_placeholder = InputFieldPlaceholder,
        selective = Selective,
    }, ReplyMarkupJson.Options);
}