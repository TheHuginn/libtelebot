using System.Text.Json.Serialization;

namespace Telebot.Models;

public record Chat(
    [property: JsonPropertyName("id")]
    long Id,

    [property: JsonPropertyName("type")]
    string Type,

    [property: JsonPropertyName("title")]
    string? Title,
    

    [property: JsonPropertyName("username")]
    string? Username,

    [property: JsonPropertyName("first_name")]
    string? FirstName,

    [property: JsonPropertyName("last_name")]
    string? LastName,

    [property: JsonPropertyName("is_forum")]
    bool? IsForum,

    [property: JsonPropertyName("is_direct_messages")]
    bool? IsDirectMessages
);

public record ChatMemberUpdated(
    [property: JsonPropertyName("chat")]
    Chat Chat,

    [property: JsonPropertyName("from")]
    User From
);

public record ChatPhoto(
    [property: JsonPropertyName("small_file_id")]
    string SmallFileId,

    [property: JsonPropertyName("small_file_unique_id")]
    string SmallFileUniqueId,

    [property: JsonPropertyName("big_file_id")]
    string BigFileId,

    [property: JsonPropertyName("big_file_unique_id")]
    string BigFileUniqueId
);