using Discord;
using Discord.WebSocket;
using IBot;
using System.Text.RegularExpressions;

namespace DiscordBot
{
    public static class Tools
    {
        public readonly static DiscordSocketClient DBot = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
        });
        public readonly static Bot TBot = new Bot(new Server("terraria.tk"));
        public readonly static Dictionary<byte, string> Names = new Dictionary<byte, string>();

        public const string Token = ;
        public const ulong Channel = ;
        public const ulong God = ;

        static IMessageChannel BroadcastChannel
        {
            get => DBot.GetChannel(Channel) as IMessageChannel;
        }

        static async Task SendMessage(string message)
        {
            await BroadcastChannel.SendMessageAsync(message);
        }

        public static bool IsMatch(this string input, string pattern)
        {
            return Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase);
        }

        public static string GetEmoji(this string prefix)
        {
            switch (prefix)
            {
                case "73": // sponsor
                    return ":moneybag:";
                case "1110": //guest
                    return ":rock:";
                case "367": //admin
                    return ":dancer:";
                case "851": //moderator
                    return ":fire_extinguisher:";
                case "2214": //builder
                case "407": //prebuilder
                    return ":bricks:";
                case "4952": //eventmaker 
                    return "accordion"; 
                case "177": //youtuber 
                    return "video_game"; 
                default: //default
                    return ":wood:";
            }
        }

        public static async void Broadcast(this Bot bot, string message)
        {
            message = Regex.Replace(message, @"<?@([^\s]*)", "$1");

            var match = new ChatMatch(message, @"\[25\] (.*) зашел\.");
            if (match.Success)
            {
                await SendMessage($"***:inbox_tray:  {match[1]} зашел.***");
                return;
            }

            match = new ChatMatch(message, @"\[25\] (.*) вышел\.");
            if (match.Success)
            {
                await SendMessage($"***:outbox_tray:  {match[1]} вышел.***"); 
                return;
            }

            match = new ChatMatch(message, @"\[(\d+)\] (.{1,20}) <\[\d+\]>: (.*)");
            if (match.Success)
            {
                var prefix = bot.Name == match[2] ? ":bee:" : match[1].GetEmoji();
                await SendMessage($"{prefix} **{match[2]}**: {Regex.Replace(match[3], @"\[\d+\]", " :leaves: ")}");
                return;
            }

            match = new ChatMatch(message, @"\[(\d+)\] (.{1,20}): (.*)");
            if (match.Success)
            {
                var prefix = bot.Name == match[2] ? ":bee:" : match[1].GetEmoji();
                await SendMessage($"{prefix} **{match[2]}**: {Regex.Replace(match[3], @"\[\d+\]", " :leaves: ")}");
                return;
            }

            match = new ChatMatch(message, @"<(?:To|From).*> .*");
            if (match.Success)
            {
                return;
            }
            await SendMessage($"***{Regex.Replace(message, @"\[\d+\]", ":mushroom:")}***");
        }

        public static void Handle(this Packet packet)
        {
            if (packet.Type == 4)
            {
                using (var r = packet.Reader)
                {
                    var id = r.ReadByte();
                    r.BaseStream.Position += 2;
                    var name = r.ReadString();

                    if (Names.ContainsKey(id))
                        Names[id] = Message.ColorConstruction.Replace(name, "$2");
                    else
                        Names.Add(id, Message.ColorConstruction.Replace(name, "$2"));
                }
            }
            else if (packet.Type == 14 && packet.Data[1] == 0)
            {
                Names.Remove(packet.Data[0]);
            }
        }
    }

    public struct ChatMatch
    {
        Match match;
        string message;

        public bool Success => match.Success && match.Length == message.Length;

        public string this[int i] => match.Groups[i].Value;

        public ChatMatch(string message, string pattern)
        {
            this.message = message;
            match = Regex.Match(message, pattern);
        }
    }
}
