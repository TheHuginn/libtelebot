# Telebot

A simple **Telegram Bot API client for .NET**, built as a learning project for practicing C# and HTTP-based APIs.

---

## Example

```csharp
using Telebot;

var tg = new Telegram("<YOUR_BOT_TOKEN>");

// Get bot info
var me = await tg.GetMeAsync(new GetMeRequestParams(), CancellationToken.None);
Console.WriteLine(me);

// Long polling
var updates = await tg.GetUpdatesAsync(
    new GetUpdatesRequestParams(Timeout: 10), CancellationToken.None
);

foreach (var update in updates)
{
    if (update.Message?.Text is not null)
    {
        Console.WriteLine("Echoing: " + update.Message.Text);

        await tg.SendMessageAync(
            new SendMessageRequestParams(
                update.Message.Chat.Id,
                update.Message.Text
            ), CancellationToken.None
        );
    }
}
```

## Contributing

This is a learning-oriented project.

Contributions are organized via **Issues and Milestones**
