using System.Text.Json.Serialization;

namespace Telebot.Models;

public record TextQuote(
    [property: JsonPropertyName("text")]
    string Text
);