using System.Text.Json.Serialization;

namespace Telebot.Models;

public record PhotoSize(
    [property: JsonPropertyName("file_id")]
    string FileId,

    [property: JsonPropertyName("file_unique_id")]
    string FileUniqueId,

    [property: JsonPropertyName("width")]
    int Width,

    [property: JsonPropertyName("height")]
    int Height,

    [property: JsonPropertyName("file_size")]
    int? FileSize
);