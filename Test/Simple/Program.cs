using IBot;

Console.Title = ">:3";
Hooks.Console += (Message x) =>
{
    Console.WriteLine(x.GetSingleColorText.Item1);
};
Hooks.ServerChat += (Message x, Bot y) =>
{
    Console.WriteLine(x.GetSingleColorText.Item1);
};
Hooks.CrashLogs = true;

while (true)
{
    Console.Write("IP: ");
    MainAsync(Console.ReadLine() ?? "terraria.tk").Wait();
}

static async Task MainAsync(string ip)
{
    var bot = new Bot(new Server(ip));

    bot.Player.Random();
    await bot.Connect();

    while (bot.State != ConnectionState.Disconnected)
    {
        await bot.SendMessage(Console.ReadLine() ?? ":D");
    }
    Console.WriteLine(":P\n\n");
}
