using System.Text.Json.Serialization;

namespace Telebot.Models;

/// <summary>
/// Один вариант ответа в опросе
/// (см. <see href="https://core.telegram.org/bots/api#polloption"/>).
/// </summary>
/// <param name="Text">
/// Текст варианта ответа, 1–100 символов.
/// </param>
/// <param name="VoterCount">
/// Количество пользователей, проголосовавших именно за этот вариант.
/// В анонимных опросах — общее число голосов; в неанонимных — то же самое,
/// но список голосовавших доступен отдельно через <c>poll_answer</c>-апдейты.
/// </param>
/// <param name="TextEntities">
/// Специальные сущности в тексте варианта (форматирование, кастомные эмодзи и т. п.).
/// Заполняется, только если в <see cref="Text"/> действительно есть такие сущности.
/// </param>
public record PollOption(
    [property: JsonPropertyName("text")]
    string Text,

    [property: JsonPropertyName("voter_count")]
    int VoterCount,

    [property: JsonPropertyName("text_entities")]
    MessageEntity[]? TextEntities = null
);

/// <summary>
/// Опрос в чате
/// (см. <see href="https://core.telegram.org/bots/api#poll"/>).
/// </summary>
/// <remarks>
/// Одна и та же модель описывает и обычный опрос (<see cref="Type"/> = <c>"regular"</c>),
/// и опрос-викторину (<see cref="Type"/> = <c>"quiz"</c>). Поля <see cref="CorrectOptionId"/>,
/// <see cref="Explanation"/> и <see cref="ExplanationEntities"/> относятся только к
/// викторинам и в обычных опросах будут <c>null</c>.
/// <para/>
/// <see cref="CorrectOptionId"/> возвращается **только** в двух случаях: боту-автору
/// опроса и всем клиентам после того, как опрос закрыт (<see cref="IsClosed"/> = <c>true</c>).
/// Пока идущая викторина «в глухую» для всех, кроме её автора-бота.
/// </remarks>
/// <param name="Id">
/// Уникальный идентификатор опроса. Используется в <c>stopPoll</c> — впрочем,
/// на стороне бота обычно проще держать пару <c>chat_id + message_id</c>, потому что
/// именно они требуются для остановки.
/// </param>
/// <param name="Question">
/// Текст вопроса, 1–300 символов.
/// </param>
/// <param name="Options">
/// Список вариантов ответа (2–10 элементов).
/// </param>
/// <param name="TotalVoterCount">
/// Общее число уникальных пользователей, проголосовавших в опросе.
/// </param>
/// <param name="IsClosed">
/// <c>true</c>, если опрос уже закрыт и в него больше нельзя проголосовать.
/// </param>
/// <param name="IsAnonymous">
/// <c>true</c>, если опрос анонимный. В анонимных опросах Telegram не присылает
/// апдейты <c>poll_answer</c> с идентификатором проголосовавшего.
/// </param>
/// <param name="Type">
/// Тип опроса: <c>"regular"</c> — обычный, <c>"quiz"</c> — викторина с правильным ответом.
/// </param>
/// <param name="AllowsMultipleAnswers">
/// <c>true</c>, если пользователь может выбрать несколько вариантов сразу.
/// Всегда <c>false</c> для викторин (<see cref="Type"/> = <c>"quiz"</c>).
/// </param>
/// <param name="QuestionEntities">
/// Специальные сущности в тексте вопроса. Заполняется, только если они есть.
/// </param>
/// <param name="CorrectOptionId">
/// Индекс правильного варианта (0-based) — только для викторин. Возвращается
/// боту-автору всегда, а всем остальным клиентам — только после закрытия опроса.
/// </param>
/// <param name="Explanation">
/// Текст пояснения, который показывается после ответа в викторине,
/// 0–200 символов. Только для викторин.
/// </param>
/// <param name="ExplanationEntities">
/// Специальные сущности в тексте <see cref="Explanation"/>.
/// </param>
/// <param name="OpenPeriod">
/// Время в секундах, в течение которого опрос будет активен после отправки.
/// </param>
/// <param name="CloseDate">
/// Абсолютное время автозакрытия опроса, Unix-timestamp.
/// Взаимоисключающе с <see cref="OpenPeriod"/>.
/// </param>
public record Poll(
    [property: JsonPropertyName("id")]
    string Id,

    [property: JsonPropertyName("question")]
    string Question,

    [property: JsonPropertyName("options")]
    IReadOnlyList<PollOption> Options,

    [property: JsonPropertyName("total_voter_count")]
    int TotalVoterCount,

    [property: JsonPropertyName("is_closed")]
    bool IsClosed,

    [property: JsonPropertyName("is_anonymous")]
    bool IsAnonymous,

    [property: JsonPropertyName("type")]
    string Type,

    [property: JsonPropertyName("allows_multiple_answers")]
    bool AllowsMultipleAnswers,

    [property: JsonPropertyName("question_entities")]
    MessageEntity[]? QuestionEntities = null,

    [property: JsonPropertyName("correct_option_id")]
    int? CorrectOptionId = null,

    [property: JsonPropertyName("explanation")]
    string? Explanation = null,

    [property: JsonPropertyName("explanation_entities")]
    MessageEntity[]? ExplanationEntities = null,

    [property: JsonPropertyName("open_period")]
    int? OpenPeriod = null,

    [property: JsonPropertyName("close_date")]
    long? CloseDate = null
);

/// <summary>
/// Изменение голоса пользователя в неанонимном опросе
/// (см. <see href="https://core.telegram.org/bots/api#pollanswer"/>).
/// </summary>
/// <remarks>
/// Приходит в поле <see cref="Update.PollAnswer"/>. Присылается только для
/// <b>неанонимных</b> опросов — в анонимных Telegram сознательно не раскрывает,
/// кто и как проголосовал (см. <see cref="Poll.IsAnonymous"/>).
/// <para/>
/// Пустой массив <see cref="OptionIds"/> означает, что пользователь отозвал
/// свой голос — это отдельное событие, не путать с «не голосовал».
/// <para/>
/// Голосующим может быть либо обычный пользователь (<see cref="User"/>), либо
/// анонимный администратор канала / группы, голосующий от имени чата
/// (<see cref="VoterChat"/>) — заполняется ровно одно из полей.
/// </remarks>
/// <param name="PollId">
/// Идентификатор опроса, к которому относится голос — совпадает с <see cref="Poll.Id"/>
/// того сообщения-опроса, за которым бот следит.
/// </param>
/// <param name="OptionIds">
/// Индексы выбранных вариантов ответа (0-based) в порядке из <see cref="Poll.Options"/>.
/// Пустой массив — пользователь отменил свой голос.
/// </param>
/// <param name="User">
/// Пользователь, изменивший голос. Заполняется для неанонимного голосующего;
/// <c>null</c>, если голос был отдан от имени чата (см. <see cref="VoterChat"/>).
/// </param>
/// <param name="VoterChat">
/// Чат, от имени которого был изменён голос (анонимный админ канала / группы).
/// <c>null</c>, если голосовал обычный пользователь.
/// </param>
public record PollAnswer(
    [property: JsonPropertyName("poll_id")]
    string PollId,

    [property: JsonPropertyName("option_ids")]
    IReadOnlyList<int> OptionIds,

    [property: JsonPropertyName("user")]
    User? User = null,

    [property: JsonPropertyName("voter_chat")]
    Chat? VoterChat = null
);