using System.Text.Json.Serialization;

namespace Telebot.Models;

public record User(
    [property: JsonPropertyName("id")]
    long Id,

    [property: JsonPropertyName("is_bot")]
    bool IsBot,

    [property: JsonPropertyName("first_name")]
    string FirstName,

    [property: JsonPropertyName("last_name")]
    string? LastName = null,

    [property: JsonPropertyName("username")]
    string? Username = null,

    [property: JsonPropertyName("language_code")]
    string? LanguageCode = null,

    [property: JsonPropertyName("is_premium")]
    bool? IsPremium = null
);