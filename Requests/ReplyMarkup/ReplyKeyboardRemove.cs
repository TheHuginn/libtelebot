using System.Text.Json;

namespace Telebot;

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
    }, TelebotJson.Options);
}
