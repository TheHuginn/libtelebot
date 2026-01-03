using System.Text.Json.Serialization;

namespace Telebot.Models;

public record Birthdate(
    [property: JsonPropertyName("day")]
    int Day,

    [property: JsonPropertyName("month")]
    int Month,

    [property: JsonPropertyName("year")]
    int? Year
);