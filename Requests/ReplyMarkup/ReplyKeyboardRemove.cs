using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Telebot;

/// <summary>
/// Убрать обычную (reply) клавиатуру
/// (<see href="https://core.telegram.org/bots/api#replykeyboardremove"/>).
/// </summary>
/// <param name="Selective">Убрать только у выделенных пользователей.</param>
public sealed record ReplyKeyboardRemove(
    bool? Selective = null
) : IReplyMarkup
{
    /// <inheritdoc />
    [UnconditionalSuppressMessage("AOT",
        "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.",
        Justification = "Десериализация происходит используя дружественный к AOT контекст который должен иметь все типы зарегистрированными")]
    public string ToJson() => JsonSerializer.Serialize(new
    {
        remove_keyboard = true,
        selective = Selective,
    }, TelebotJson.Options);
}