
using Telebot;
using Telebot.Models;

var tg = new Telegram("8730320919:AAHRUY3JDTQRWIfmRpfJbae0Zm_rcSdkeyU");

// Get bot info
User me;
try
{
    me = await tg.GetMeAsync(new GetMeRequestParams(), CancellationToken.None);
    Console.WriteLine(me);
}
catch (TelebotException ex)
{
    Console.WriteLine(ex.Message);
    return;
}


// Long polling
var updates = await tg.GetUpdatesAsync(
    new GetUpdatesRequestParams(Timeout: 10), CancellationToken.None
);

foreach (var update in updates)
{
    if (update.Message?.Text is not null)
    {
        Console.WriteLine($"Message in: {update.Message.Chat.Id} " + update.Message.Text);
        
        //Testing mentions
        if (update.Message.Entities is not null)
        {
            foreach (var ent in update.Message.Entities)
            {
                Console.WriteLine("\tFound entity: " + ent.Type);
                Console.WriteLine("\t" + update.Message.Text.Substring(ent.Offset, ent.Length));
            }
        }
        
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
