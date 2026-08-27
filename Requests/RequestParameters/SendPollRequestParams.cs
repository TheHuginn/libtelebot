using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Telebot;

/// <summary>
/// Вариант ответа для <c>sendPoll</c>. Здесь оставлено только текстовое поле;
/// форматирование и медиа-варианты API опущены.
/// </summary>
/// <param name="Text">Текст варианта, 1–100 символов.</param>
public sealed record InputPollOption(
    [property: JsonPropertyName("text")] string Text
);

/// <summary>
/// Параметры <c>sendPoll</c> — отправка опроса. Опциональные поля Bot API
/// (анонимность, викторина, таймеры и т. п.) не поддерживаются.
/// </summary>
/// <param name="ChatId">Идентификатор чата-получателя.</param>
/// <param name="Question">Текст вопроса, 1–300 символов.</param>
/// <param name="Options">Варианты ответа (2–10 элементов).</param>
public sealed record SendPollRequestParams(
    long ChatId,
    string Question,
    IReadOnlyList<InputPollOption> Options
) : TelegramRequest("sendPoll")
{
    /// <inheritdoc />
    [UnconditionalSuppressMessage("AOT",
        "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.",
        Justification = "Десериализация происходит используя дружественный к AOT контекст который должен иметь все типы зарегистрированными")]
    public override IEnumerable<TelegramRequestField> GetRequestFields()
    {
        yield return new TelegramRequestField("chat_id", ChatId.ToString());
        yield return new TelegramRequestField("question", Question);
        yield return new TelegramRequestField("options", JsonSerializer.Serialize(Options, TelebotJson.Options));
    }
}