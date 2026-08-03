using System.Text.Json;
using System.Text.Json.Serialization;

namespace Telebot;

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