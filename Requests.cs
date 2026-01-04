using System.Text.Json;

namespace Telebot;

//Allows request encoding to Form/Multipart
public interface ITelegramEncodable
{
    IEnumerable<TelegramRequestField> GetRequestFields();
    IEnumerable<TelegramRequestFile> GetRequestFiles();
}

//Base for every request
public record TelegramRequest(string Endpoint) : ITelegramEncodable
{
    public virtual IEnumerable<TelegramRequestField> GetRequestFields()
    {
        return Enumerable.Empty<TelegramRequestField>();
    }

    public virtual IEnumerable<TelegramRequestFile> GetRequestFiles()
    {
        return Enumerable.Empty<TelegramRequestFile>();
    }
}

//Request for GetMe
public sealed record GetMeRequestParams() : TelegramRequest("GetMe"), ITelegramEncodable;

//Request for GetUpdates
public sealed record GetUpdatesRequestParams(
    int? Offset = null,
    int? Limit = null,
    int? Timeout = null,
    IReadOnlyList<string>? AllowedUpdates = null
) : TelegramRequest("GetUpdates"), ITelegramEncodable
{
    public override IEnumerable<TelegramRequestField> GetRequestFields()
    {
        if (Offset is not null)
            yield return new TelegramRequestField("offset", Offset.Value.ToString());
        if (Limit is not null)
            yield return new TelegramRequestField("limit", Limit.Value.ToString());
        if (Timeout is not null)
            yield return new TelegramRequestField("timeout", Timeout.Value.ToString());
        if (AllowedUpdates is not null)
            yield return new TelegramRequestField("allowed_updates", JsonSerializer.Serialize(AllowedUpdates));
    }
}

//Request for sendMessage
public sealed record SendMessageRequestParams(
    long ChatId,
    string Text,
    string? ParseMode = null,
    bool? DisableNotification = null,
    bool? ProtectContent = null,
    int? ReplyToMessageId = null,
    bool? AllowSendingWithoutReply = null
) : TelegramRequest("sendMessage")
{
    public override IEnumerable<TelegramRequestField> GetRequestFields()
    {
        yield return new TelegramRequestField("chat_id", ChatId.ToString());
        yield return new TelegramRequestField("text", Text);

        if (ParseMode is not null)
            yield return new TelegramRequestField("parse_mode", ParseMode);
        if (DisableNotification is not null)
            yield return new TelegramRequestField("disable_notification", 
                DisableNotification.Value ? "true" : "false");
        if (ProtectContent is not null)
            yield return new TelegramRequestField("protect_content", 
                ProtectContent.Value ? "true" : "false");
        if (ReplyToMessageId is not null)
            yield return new TelegramRequestField("reply_to_message_id", ReplyToMessageId.Value.ToString());
        if (AllowSendingWithoutReply is not null)
            yield return new TelegramRequestField("allow_sending_without_reply",
                AllowSendingWithoutReply.Value ? "true" : "false");
        
    }
}

//Request for sendPhoto
public sealed record SendPhotoRequestParams(
    long ChatId,
    InputFile Photo,
    string? Caption = null,
    string? ParseMode = null,
    bool? HasSpoiler = null,
    bool? DisableNotification = null,
    bool? ProtectContent = null,
    int? ReplyToMessageId = null,
    bool? AllowSendingWithoutReply = null
) : TelegramRequest("sendPhoto")
{
    public override IEnumerable<TelegramRequestField> GetRequestFields()
    {
        yield return new TelegramRequestField("chat_id", ChatId.ToString());

        if (Caption is not null)
            yield return new TelegramRequestField("caption", Caption);

        if (ParseMode is not null)
            yield return new TelegramRequestField("parse_mode", ParseMode);

        if (HasSpoiler is not null)
            yield return new TelegramRequestField(
                "has_spoiler",
                HasSpoiler.Value ? "true" : "false"
            );

        if (DisableNotification is not null)
            yield return new TelegramRequestField(
                "disable_notification",
                DisableNotification.Value ? "true" : "false"
            );

        if (ProtectContent is not null)
            yield return new TelegramRequestField(
                "protect_content",
                ProtectContent.Value ? "true" : "false"
            );

        if (ReplyToMessageId is not null)
            yield return new TelegramRequestField(
                "reply_to_message_id",
                ReplyToMessageId.Value.ToString()
            );

        if (AllowSendingWithoutReply is not null)
            yield return new TelegramRequestField(
                "allow_sending_without_reply",
                AllowSendingWithoutReply.Value ? "true" : "false"
            );
    }

    public override IEnumerable<TelegramRequestFile> GetRequestFiles()
    {
        yield return new TelegramRequestFile("photo", Photo);
    }
}

public sealed record SetWebhookRequestParams(
    string Url,
    InputFileWithStream? Certificate = null,
    string? IpAddress = null,
    int? MaxConnections = null,
    IReadOnlyList<string>? AllowedUpdates = null,
    bool? DropPendingUpdates = null,
    string? SecretToken = null
) : TelegramRequest("setWebhook")
{
    public override IEnumerable<TelegramRequestField> GetRequestFields()
    {
        yield return new TelegramRequestField("url", Url);

        if (IpAddress is not null)
            yield return new TelegramRequestField("ip_address", IpAddress);

        if (MaxConnections is not null)
            yield return new TelegramRequestField(
                "max_connections",
                MaxConnections.Value.ToString()
            );

        if (AllowedUpdates is not null)
            yield return new TelegramRequestField(
                "allowed_updates",
                JsonSerializer.Serialize(AllowedUpdates)
            );

        if (DropPendingUpdates is not null)
            yield return new TelegramRequestField(
                "drop_pending_updates",
                DropPendingUpdates.Value ? "true" : "false"
            );

        if (SecretToken is not null)
            yield return new TelegramRequestField("secret_token", SecretToken);
    }

    public override IEnumerable<TelegramRequestFile> GetRequestFiles()
    {
        if (Certificate is not null)
            yield return new TelegramRequestFile("certificate", Certificate);
    }
}
