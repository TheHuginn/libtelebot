using System.Text.Json;

namespace Telebot;

public interface ITelegramEncodable
{
    IEnumerable<TelegramRequestField> GetRequestFields();
    IEnumerable<TelegramRequestFile> GetRequestFiles();
}

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