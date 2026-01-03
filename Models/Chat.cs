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

public record Location(
    [property: JsonPropertyName("latitude")]
    double Latitude,

    [property: JsonPropertyName("longitude")]
    double Longitude,

    [property: JsonPropertyName("horizontal_accuracy")]
    double? HorizontalAccuracy,

    [property: JsonPropertyName("live_period")]
    int? LivePeriod,

    [property: JsonPropertyName("heading")]
    int? Heading,

    [property: JsonPropertyName("proximity_alert_radius")]
    int? ProximityAlertRadius
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

public record ChatPermissions(
    [property: JsonPropertyName("can_send_messages")]
    bool? CanSendMessages,

    [property: JsonPropertyName("can_send_audios")]
    bool? CanSendAudios,

    [property: JsonPropertyName("can_send_documents")]
    bool? CanSendDocuments,

    [property: JsonPropertyName("can_send_photos")]
    bool? CanSendPhotos,

    [property: JsonPropertyName("can_send_videos")]
    bool? CanSendVideos,

    [property: JsonPropertyName("can_send_video_notes")]
    bool? CanSendVideoNotes,

    [property: JsonPropertyName("can_send_voice_notes")]
    bool? CanSendVoiceNotes,

    [property: JsonPropertyName("can_send_polls")]
    bool? CanSendPolls,

    [property: JsonPropertyName("can_send_other_messages")]
    bool? CanSendOtherMessages,

    [property: JsonPropertyName("can_add_web_page_previews")]
    bool? CanAddWebPagePreviews,

    [property: JsonPropertyName("can_change_info")]
    bool? CanChangeInfo,

    [property: JsonPropertyName("can_invite_users")]
    bool? CanInviteUsers,

    [property: JsonPropertyName("can_pin_messages")]
    bool? CanPinMessages,

    [property: JsonPropertyName("can_manage_topics")]
    bool? CanManageTopics
);