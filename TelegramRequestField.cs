namespace Telebot;

public record TelegramRequestField(
    string Name,
    string Value
);

public abstract record InputFile;

public sealed record InputFileWithId(string Id) : InputFile;
public sealed record InputFileWithUrl(Uri Url) : InputFile;
public sealed record InputFileWithStream(Stream Stream, string ContentType, string FileName) : InputFile;

public record TelegramRequestFile(
    string Name,
    InputFile File
);