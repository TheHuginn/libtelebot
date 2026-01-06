
using Telebot;

var tg = new Telegram("<YOUR TOKEN>");

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
        Console.WriteLine($"Message in: {update.Message.Chat.Id}" + update.Message.Text);
        
        try
        {
            await tg.SendMessageAsync(
                new SendMessageRequestParams(
                    update.Message.Chat.Id,
                    update.Message.Text,
                    update.Message.MessageThreadId
                ), CancellationToken.None
            );
        }catch (TelebotException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
