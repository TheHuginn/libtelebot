using System.Text.Json.Serialization;

namespace Telebot.Models;

/// <summary>
/// Представляет одно входящее обновление от Telegram Bot API
/// (см. <see href="https://core.telegram.org/bots/api#update"/>).
/// </summary>
/// <remarks>
/// За одно обновление приходит ровно одно из необязательных полей —
/// именно по тому, какое поле не равно <c>null</c>, определяется тип события
/// (новое сообщение, отредактированное сообщение, изменение членства в чате и т. п.).
/// Идентификатор <see cref="UpdateId"/> монотонно возрастает и используется
/// для подтверждения получения через параметр <c>offset</c> в <c>getUpdates</c>.
/// </remarks>
/// <param name="UpdateId">
/// Уникальный возрастающий идентификатор обновления.
/// Используется как <c>offset+1</c> при следующем вызове <c>getUpdates</c>,
/// чтобы пометить это обновление как обработанное.
/// </param>
/// <param name="Message">
/// Новое входящее сообщение любого типа — текст, фото, стикер и т. д.
/// </param>
/// <param name="EditedMessage">
/// Новая версия ранее отправленного сообщения, известного боту,
/// после редактирования пользователем.
/// </param>
/// <param name="BusinessConnection">
/// Изменение состояния бизнес-подключения бота к аккаунту пользователя
/// (включение, отключение, смена прав).
/// </param>
/// <param name="BusinessMessage">
/// Новое сообщение, полученное в рамках бизнес-аккаунта, подключённого к боту.
/// </param>
/// <param name="EditedBusinessMessage">
/// Отредактированная версия сообщения в рамках бизнес-аккаунта.
/// </param>
/// <param name="DeletedBusinessMessages">
/// Уведомление об удалении сообщений в бизнес-аккаунте,
/// подключённом к боту.
/// </param>
/// <param name="MyChatMember">
/// Изменение статуса самого бота как участника чата
/// (например, бот добавлен в группу или сделан администратором).
/// </param>
/// <param name="ChatMember">
/// Изменение статуса другого участника чата.
/// Приходит только если бот явно подписан на <c>chat_member</c>
/// через <c>allowed_updates</c> в <c>getUpdates</c>.
/// </param>
public record Update(
    [property: JsonPropertyName("update_id")]
    long UpdateId,

    [property: JsonPropertyName("message")]
    Message? Message = null,

    [property: JsonPropertyName("edited_message")]
    Message? EditedMessage = null,

    [property: JsonPropertyName("business_connection")]
    BusinessConnection? BusinessConnection = null,

    [property: JsonPropertyName("business_message")]
    Message? BusinessMessage = null,

    [property: JsonPropertyName("edited_business_message")]
    Message? EditedBusinessMessage = null,

    [property: JsonPropertyName("deleted_business_messages")]
    BusinessMessagesDeleted? DeletedBusinessMessages = null,

    [property: JsonPropertyName("my_chat_member")]
    ChatMemberUpdated? MyChatMember = null,

    [property: JsonPropertyName("chat_member")]
    ChatMemberUpdated? ChatMember = null
);