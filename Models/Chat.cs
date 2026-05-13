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

public record ChatFullInfo(
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
    bool? IsDirectMessages,

    [property: JsonPropertyName("accent_color_id")]
    int AccentColorId,

    [property: JsonPropertyName("max_reaction_count")]
    int MaxReactionCount,

    [property: JsonPropertyName("photo")]
    ChatPhoto? Photo,

    [property: JsonPropertyName("active_usernames")]
    string[]? ActiveUsernames,

    [property: JsonPropertyName("birthdate")]
    Birthdate? Birthdate,

    [property: JsonPropertyName("business_intro")]
    BusinessIntro? BusinessIntro,

    [property: JsonPropertyName("business_location")]
    BusinessLocation? BusinessLocation,
    
    /* IMPLEMENT TYPES
    [property: JsonPropertyName("business_opening_hours")]
    BusinessOpeningHours? BusinessOpeningHours,
    */

    [property: JsonPropertyName("personal_chat")]
    Chat? PersonalChat,

    [property: JsonPropertyName("parent_chat")]
    Chat? ParentChat,

    [property: JsonPropertyName("available_reactions")]
    ReactionType[]? AvailableReactions,

    [property: JsonPropertyName("background_custom_emoji_id")]
    string? BackgroundCustomEmojiId,

    [property: JsonPropertyName("profile_accent_color_id")]
    int? ProfileAccentColorId,

    [property: JsonPropertyName("profile_background_custom_emoji_id")]
    string? ProfileBackgroundCustomEmojiId,

    [property: JsonPropertyName("emoji_status_custom_emoji_id")]
    string? EmojiStatusCustomEmojiId,

    [property: JsonPropertyName("emoji_status_expiration_date")]
    int? EmojiStatusExpirationDate,

    [property: JsonPropertyName("bio")]
    string? Bio,

    [property: JsonPropertyName("has_private_forwards")]
    bool? HasPrivateForwards,

    [property: JsonPropertyName("has_restricted_voice_and_video_messages")]
    bool? HasRestrictedVoiceAndVideoMessages,

    [property: JsonPropertyName("join_to_send_messages")]
    bool? JoinToSendMessages,

    [property: JsonPropertyName("join_by_request")]
    bool? JoinByRequest,

    [property: JsonPropertyName("description")]
    string? Description,

    [property: JsonPropertyName("invite_link")]
    string? InviteLink,

    [property: JsonPropertyName("pinned_message")]
    Message? PinnedMessage,

    [property: JsonPropertyName("permissions")]
    ChatPermissions? Permissions,

    [property: JsonPropertyName("slow_mode_delay")]
    int? SlowModeDelay,

    [property: JsonPropertyName("unrestrict_boost_count")]
    int? UnrestrictBoostCount,

    [property: JsonPropertyName("message_auto_delete_time")]
    int? MessageAutoDeleteTime,

    [property: JsonPropertyName("has_aggressive_anti_spam_enabled")]
    bool? HasAggressiveAntiSpamEnabled,

    [property: JsonPropertyName("has_hidden_members")]
    bool? HasHiddenMembers,

    [property: JsonPropertyName("has_protected_content")]
    bool? HasProtectedContent,

    [property: JsonPropertyName("sticker_set_name")]
    string? StickerSetName,

    [property: JsonPropertyName("can_set_sticker_set")]
    bool? CanSetStickerSet,

    [property: JsonPropertyName("custom_emoji_sticker_set_name")]
    string? CustomEmojiStickerSetName,

    [property: JsonPropertyName("linked_chat_id")]
    long? LinkedChatId,
    
    /* IMPLEMENT TYPES
    [property: JsonPropertyName("location")]
    ChatLocation? Location,
    
    [property: JsonPropertyName("rating")]
    UserRating? Rating,
    */
    [property: JsonPropertyName("paid_message_star_count")]
    int? PaidMessageStarCount
    
    /* IMPLEMENT TYPES
    [property: JsonPropertyName("unique_gift_colors")]
    UniqueGiftColors? UniqueGiftColors
    */
);

