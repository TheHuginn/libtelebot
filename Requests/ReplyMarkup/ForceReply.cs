using System.Text.Json;

namespace Telebot;

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
    }, TelebotJson.Options);
}
