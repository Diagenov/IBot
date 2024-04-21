using Discord;
using Discord.WebSocket;
using IBot;
using static DiscordBot.Tools;


Console.Title = ">:3";
Hooks.Console += (Message x) =>
{
    Console.WriteLine(x.GetSingleColorText.Item1);
};
Hooks.ServerChat += (Message x, Bot y) =>
{
    y.Broadcast(x.GetSingleColorText.Item1);
    Console.WriteLine(x.GetSingleColorText.Item1);
};
Hooks.ReadData += (Packet x, Bot y) =>
{
    x.Handle();
    return Hooks.Result.Continue;
};
Hooks.CrashLogs = true;
DiscordConnect().Wait();


static async Task DiscordConnect()
{
    DBot.Log += async (LogMessage x) =>
    {
        Console.WriteLine(x.ToString());
    };
    DBot.MessageReceived += DiscordMessage;

    await DBot.LoginAsync(TokenType.Bot, Token);
    await DBot.StartAsync();
    await DBot.SetCustomStatusAsync(">:3");
    await DBot.SetStatusAsync(UserStatus.Online);
    await Task.Delay(-1);
}

static async Task DiscordMessage(SocketMessage x)
{
    if (Channel != x.Channel.Id || x.Author.IsBot || x.Author.Id == DBot.CurrentUser.Id)
        return;

    var message = x.Content;
    if (string.IsNullOrWhiteSpace(message) || message.Length > 250 || message.Contains("\n"))
        return;

    if (x.Author.Id == God)
        switch (message)
        {
            case "заходи":
                Names.Clear();
                await TBot.Connect();
                return;

            case "выходи":
                Names.Clear();
                TBot.Disconnect();
                return;

            case "список":
                if (Names.Count == 0)
                    await x.Channel.SendMessageAsync(@"***пусто***");
                else
                    await x.Channel.SendMessageAsync($"***Список ({Names.Values.Count})***\n```\n{string.Join(", ", Names.Values)}\n```");
                return;

            case "спать":
                await DBot.StopAsync();
                TBot.Disconnect();
                Environment.Exit(0);
                return;
        }

    if (TBot.State != IBot.ConnectionState.Connected)
    {
        return;
    }
    if (message.StartsWith('.') || message.StartsWith('/'))
    {
        if (x.Author.Id == God)
            await TBot.SendMessage(message);
        return;
    }
    await TBot.SendMessage($"«{message}» {{by}} {x.Author.Username}");
}