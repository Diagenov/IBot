using System.Collections.Generic;
using System.IO;
using IBotProxy;

namespace IBot
{
    public class Proxy
    {
        public readonly ushort Port;
        public readonly string IP;
        public readonly ProxyType Type;

        public Proxy(string ip, ushort port, ProxyType type = ProxyType.Socks5)
        {
            if (string.IsNullOrWhiteSpace(ip))
                ip = "127.0.0.1";

            IP = ip;
            Port = port;
            Type = type;
        }

        public static string DefaultProxyListPath
        {
            get => Path.Combine("Proxy.txt");
        }
        /// <summary>
        /// ip:port -> { ip, port }
        /// </summary>
        /// <param name="path">If equal to null, the default path is used.</param>
        public static List<Proxy> LoadProxyList(ProxyType type, string path = null)
        {
            if (string.IsNullOrWhiteSpace(path))
                path = DefaultProxyListPath;

            var list = new List<Proxy>();
            if (!File.Exists(path))
                return list;

            try
            {
                using (var r = new StreamReader(path))
                    while (!r.EndOfStream)
                    {
                        var line = r.ReadLine();
                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        var proxy = line.Split(':');
                        if (proxy.Length < 2 || !ushort.TryParse(proxy[1], out ushort port))
                            continue;

                        list.Add(new Proxy(proxy[0], port, type));
                    }
            }
            catch (IOException ex)
            {
                Hooks.HandleExceptions(ex);
            }
            return list;
        }

        public override string ToString()
        {
            return $"{IP}:{Port}";
        }
    }
}
