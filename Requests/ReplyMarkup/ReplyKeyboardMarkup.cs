using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Telebot;

/// <summary>
/// Кнопка обычной (reply) клавиатуры
/// (<see href="https://core.telegram.org/bots/api#keyboardbutton"/>).
/// Покрыт минимум — только текстовая кнопка.
/// </summary>
/// <param name="Text">Текст на кнопке. При нажатии отправляется как сообщение.</param>
public sealed record KeyboardButton(
    [property: JsonPropertyName("text")] string Text
);

/// <summary>
/// Обычная (не инлайновая) клавиатура, заменяющая клавиатуру устройства
/// (<see href="https://core.telegram.org/bots/api#replykeyboardmarkup"/>).
/// </summary>
/// <param name="Keyboard">Ряды кнопок: внешний уровень — ряды, внутренний — кнопки в ряду.</param>
/// <param name="IsPersistent">Держать клавиатуру видимой всегда.</param>
/// <param name="ResizeKeyboard">Уменьшить высоту клавиатуры до необходимой.</param>
/// <param name="OneTimeKeyboard">Скрыть клавиатуру после первого нажатия.</param>
/// <param name="InputFieldPlaceholder">Placeholder поля ввода, 1–64 символа.</param>
/// <param name="Selective">Показать клавиатуру только упомянутым пользователям / автору reply-цели.</param>
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
    [UnconditionalSuppressMessage("AOT",
        "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.",
        Justification = "Десериализация происходит используя дружественный к AOT контекст который должен иметь все типы зарегистрированными")]
    public string ToJson() => JsonSerializer.Serialize(this, TelebotJson.Options);
}