using IBot;
using Melanchall.DryWetMidi.Core;
using static Musician.Tools;


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

if (!LoadMidies())
{
    Console.Write("Where're the midies? :P");
    Console.ReadLine();
    return;
}

int index = 0;
ushort port = 7777;
string ip = "terraria.tk";
string password = "";

while (true)
{
    Console.Write("\n\n\nIP: ");
    var _ip = Console.ReadLine();

    Console.Write("Port: ");
    if (ushort.TryParse(Console.ReadLine(), out ushort _port))
    {
        port = _port;
    }

    Console.Write("Password: ");
    password = Console.ReadLine() ?? "";

    Console.Write("Track number [#]: ");
    var success = int.TryParse(Console.ReadLine(), out int _index);

    if (!string.IsNullOrWhiteSpace(_ip))
    {
        ip = _ip;
    }
    if (success && index >= 0 && index < Midies.Count)
    {
        index = _index;
    }
    MainAsync(ip, port, index, password).Wait();
}


static async Task MainAsync(string ip, ushort port, int index, string password)
{
    var names = new string[Count] 
    {
        "серебряная арфа", "колокольчик", "золотая арфа"//, "гитара", "барабаны" 
    };
    var server = new Server(ip, port)
    {
        Password = password
    };

    for (int i = 0; i < Count; i++)
    {
        var bot = new Bot(server, names[i]);
        await bot.Connect();
    }
    if (index == 0)
    {
        PreparePlaying(server, Midies.Skip(1).Select(x => new Midi
        {
            MidiFile = MidiFile.Read(x.FullName),
            FileInfo = x
        }).ToArray());
    }
    else
    {
        PreparePlaying(server, new Midi
        {
            MidiFile = MidiFile.Read(Midies[index].FullName),
            FileInfo = Midies[index]
        });
    }

    List<Bot> bots;
    while ((bots = Bot.Bots(server)).Count > 0)
    {
        var message = Console.ReadLine() ?? ":D";
        if (message.StartsWith(".") || message.StartsWith("/"))
        {
            bots.ForEach(x => x.SendMessage(message));
            continue;
        }
        await bots[0].SendMessage(message);
    }
    Bot.Disconnect(server);
    Console.Write(":P");
}