using System.Text.Json;
using System.Text.Json.Serialization;

namespace Telebot;

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
    public string ToJson() => JsonSerializer.Serialize(this, TelebotJson.Options);
}
