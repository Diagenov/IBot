using System.Drawing;

namespace IBot
{
    public class Server
    {
        public const ClientVersion CurrentVersion = ClientVersion.Version1449;

        public readonly ClientVersion Version;
        public readonly ushort Port;
        public readonly string IP;
        public string Password;
        public Rectangle Area;
        public Point Spawn;
        public string Name;

        public Server(string ip = "127.0.0.1", ushort port = 7777, ClientVersion version = CurrentVersion)
        {
            if (string.IsNullOrWhiteSpace(ip))
                ip = "127.0.0.1";

            IP = ip;
            Port = port;
            Version = version;
        }

        public override string ToString()
        {
            return Name ?? $"{IP}:{Port}";
        }
    }
}
