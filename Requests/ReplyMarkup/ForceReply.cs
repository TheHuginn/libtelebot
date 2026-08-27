using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Telebot;

/// <summary>
/// Сразу открыть у пользователя интерфейс ответа на сообщение бота
/// (<see href="https://core.telegram.org/bots/api#forcereply"/>).
/// </summary>
/// <param name="InputFieldPlaceholder">Placeholder поля ввода, 1–64 символа.</param>
/// <param name="Selective">Сработать только для выделенных пользователей.</param>
public sealed record ForceReply(
    string? InputFieldPlaceholder = null,
    bool? Selective = null
) : IReplyMarkup
{
    /// <inheritdoc />
    [UnconditionalSuppressMessage("AOT",
        "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.",
        Justification = "Десериализация происходит используя дружественный к AOT контекст который должен иметь все типы зарегистрированными")]
    public string ToJson() => JsonSerializer.Serialize(new
    {
        force_reply = true,
        input_field_placeholder = InputFieldPlaceholder,
        selective = Selective,
    }, TelebotJson.Options);
}