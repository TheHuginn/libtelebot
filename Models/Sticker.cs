using System.Text.Json.Serialization;

namespace Telebot.Models;

public record Sticker(
    [property: JsonPropertyName("file_id")]
    string FileId,

    [property: JsonPropertyName("file_unique_id")]
    string FileUniqueId,

    [property: JsonPropertyName("type")]
    string Type,

    [property: JsonPropertyName("width")]
    int Width,

    [property: JsonPropertyName("height")]
    int Height,

    [property: JsonPropertyName("is_animated")]
    bool IsAnimated,

    [property: JsonPropertyName("is_video")]
    bool IsVideo,

    [property: JsonPropertyName("thumbnail")]
    PhotoSize? Thumbnail,

    [property: JsonPropertyName("emoji")]
    string? Emoji,

    [property: JsonPropertyName("set_name")]
    string? SetName,

    [property: JsonPropertyName("mask_position")]
    MaskPosition? MaskPosition,

    [property: JsonPropertyName("custom_emoji_id")]
    string? CustomEmojiId,

    [property: JsonPropertyName("needs_repainting")]
    bool? NeedsRepainting,

    [property: JsonPropertyName("file_size")]
    int? FileSize
);