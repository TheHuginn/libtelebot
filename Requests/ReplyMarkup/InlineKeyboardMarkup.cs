using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Telebot;

/// <summary>
/// Кнопка инлайн-клавиатуры
/// (<see href="https://core.telegram.org/bots/api#inlinekeyboardbutton"/>).
/// Покрыт минимальный набор полей: текст, ссылка, callback_data.
/// </summary>
/// <param name="Text">Текст на кнопке.</param>
/// <param name="Url">URL, открываемый при нажатии.</param>
/// <param name="CallbackData">
/// Данные, приходящие обратно в <c>callback_query</c> при нажатии, 1–64 байта UTF-8.
/// </param>
public sealed record InlineKeyboardButton(
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("url")] string? Url = null,
    [property: JsonPropertyName("callback_data")] string? CallbackData = null
);

/// <summary>
/// Инлайн-клавиатура под сообщением
/// (<see href="https://core.telegram.org/bots/api#inlinekeyboardmarkup"/>).
/// </summary>
/// <param name="InlineKeyboard">Ряды кнопок: внешний уровень — ряды, внутренний — кнопки в ряду.</param>
public sealed record InlineKeyboardMarkup(
    [property: JsonPropertyName("inline_keyboard")]
    IReadOnlyList<IReadOnlyList<InlineKeyboardButton>> InlineKeyboard
) : IReplyMarkup
{
    /// <inheritdoc />
    [UnconditionalSuppressMessage("AOT",
        "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.",
        Justification = "Десериализация происходит используя дружественный к AOT контекст который должен иметь все типы зарегистрированными")]
    public string ToJson() => JsonSerializer.Serialize(this, TelebotJson.Options);
}