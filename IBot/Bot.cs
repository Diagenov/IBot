using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace IBot
{
    public class Bot
    {
        static Dictionary<Server, Thread> Threads = new Dictionary<Server, Thread>();
        static Dictionary<Server, List<Bot>> List = new Dictionary<Server, List<Bot>>();

        #region Поля, свойства
        /// <summary>
        /// Set the max time to wait for response from the server during the connection. Default is 1 minute (60000ms).
        /// </summary>
        public static uint ResponseWaitingTime = 3 * 60 * 1000; // 3 minutes

        /// <summary>
        /// Needed to send 36 packets, which are sent every 15 seconds to keeping connection.
        /// You will have to adjust its values ​​yourself to make the bot look like a regular player.
        /// </summary>
        public readonly byte[] Zones = new byte[4]; // ?

        public string UUID;
        public byte ID { get; private set; }
        public Server Server { get; private set; }
        public Proxy Proxy { get; private set; }
        public Player Player { get; private set; }
        public ConnectionState State { get; private set; }
        public ClientVersion Version
        {
            get => Server.Version;
        }
        public string Name
        {
            get => Player.Name;
            set => Player.Name = value;
        }

        byte lastSending = 0;
        DateTime lastResponseTime;
        NetworkStream stream;
        TcpClient client;
        #endregion

        #region Конструкторы
        /// <param name="server">Don't equate it to null.</param>
        /// <param name="name">If equal to null, it's created randomly.</param>
        public Bot(Server server, string name = null)
        {
            Server = server ?? throw new NullReferenceException("The parameter \"server\" must not be null.");
            Player = new Player(name);
        }
        #endregion

        #region Connecting
        public async Task<bool> Connect()
        {
            if (State != ConnectionState.Disconnected)
                Disconnect();

            Utils.ConsoleInfo("Bot", Name, $"Connect to the server {Server}. . . ");
            return await Connect(Server.IP, Server.Port) && await ContinueConnection();
        }

        public async Task<bool> Connect(Proxy proxy)
        {
            if (State != ConnectionState.Disconnected)
                Disconnect();

            Utils.ConsoleMessage("Bot", Name, $"Connect to the server {Server} with proxy {proxy}. . . ", Color.PaleVioletRed);

            Proxy = proxy;
            if (!await Connect(proxy.IP, proxy.Port))
                return false;

            try
            {
                if (!await IBotProxy.Helper.ConnectProxyToServer(client, proxy.Type, Server.IP, Server.Port, ResponseWaitingTime))
                {
                    Utils.ConsoleError("Bot", Name, $"Connection timeout expired ({ResponseWaitingTime}ms).");
                    Disconnect();
                    return false;
                }
            }
            catch (Exception ex)
            {
                Hooks.HandleExceptions(ex, this);
                Disconnect();
                return false;
            }
            return await ContinueConnection();
        }

        async Task<bool> Connect(string ip, ushort port)
        {
            State = ConnectionState.Connecting;

            lock (new object())
            {
                if (!List.ContainsKey(Server))
                    List.Add(Server, new List<Bot>());
            }
            List[Server].Add(this);

            try
            {                
                client = new TcpClient();
                await client.ConnectAsync(ip, port);
                stream = client.GetStream();
                return true;
            }
            catch (SocketException ex)
            {
                Hooks.HandleExceptions(ex, this);
                Disconnect();
                return false;
            }
        }

        async Task<bool> ContinueConnection()
        {
            lastResponseTime = DateTime.Now;
            State = ConnectionState.ConnectRequested;
            if (!await Send(1, "Terraria" + (int)Version))
            {
                return false;
            }
            lock (new object())
            {
                if (!Threads.ContainsKey(Server))
                {
                    Threads.Add(Server, new Thread(Updater));
                    Threads[Server].Start(Server);
                }
            }
            return true;
        }

        public void Disconnect()
        {
            if (State == ConnectionState.Disconnected)
                return;
            
            lock (new object())
            {
                List[Server].Remove(this);
                if (List[Server].Count == 0)
                    List.Remove(Server);
            }

            if (stream != null)
            {
                stream.Dispose();
                stream.Close();
            }
            if (client.Connected)
            {
                client.Close();
            }
            State = ConnectionState.Disconnected;
            Utils.ConsoleInfo("Bot", Name, "Disconnected.");
            Hooks.HandleDisconnect(this);
        }

        /// <returns>Disconnected bots count.</returns>
        public static int Disconnect(string ip, ushort port, Func<Bot, bool> selector = null)
        {
            var server = List.Keys.FirstOrDefault(i => i.IP == ip && i.Port == port);
            if (server == null)
                return 0;

            int count = 0;
            foreach (var x in List[server].FindAll(x => x.State != ConnectionState.Disconnected))
                if (selector == null || selector(x))
                {
                    x.Disconnect();
                    count++;
                }
            return count;
        }

        public static int Disconnect(Server server, Func<Bot, bool> selector = null)
        {
            if (server == null)
                return 0;
            return Disconnect(server.IP, server.Port, selector);
        }
        #endregion

        #region Updaters
        static async void Updater(object obj)
        {
            Thread.CurrentThread.IsBackground = true;

            var timer1 = DateTime.MinValue;
            var timer2 = DateTime.MinValue;
            var timer3 = DateTime.Now;
            var server = (Server)obj;

            while (List.ContainsKey(server))
            {
                var list = List[server].ToArray();
                if (list.Length == 0)
                {
                    if (timer3 == DateTime.MinValue)
                    {
                        timer1 = DateTime.MinValue;
                        timer2 = DateTime.MinValue;
                        timer3 = DateTime.Now;
                    }
                    if (DateTime.Now.Subtract(timer1).TotalSeconds > 60)
                    {
                        List.Remove(server);
                        break;
                    }
                    await Task.Delay(10);
                    continue;
                }
                timer3 = DateTime.MinValue;

                var any = false;
                foreach (var x in list)
                {
                    any = await x.ReadPacket() || any;
                }
                if (!any)
                {
                    await Task.Delay(10);
                    continue;
                }

                if (timer1 == DateTime.MinValue || timer2 == DateTime.MinValue)
                {
                    timer1 = DateTime.Now;
                    timer2 = DateTime.Now;
                    continue;
                }

                var interval = DateTime.Now.Subtract(timer1).TotalMilliseconds;
                if (interval > 100)
                {
                    foreach (var x in list)
                    {
                        if (x.State != ConnectionState.Connected)
                            continue;

                        if (x.UpdateBuffs((int)interval))
                            await x.SendBuffs();
                    }
                    timer1 = DateTime.Now;
                }

                if (DateTime.Now.Subtract(timer2).TotalMilliseconds < 15000)
                {
                    continue;
                }
                foreach (var x in list)
                {
                    if (x.State != ConnectionState.Connected)
                        continue;

                    if ((!x.lastSending.Bit(0) && !await x.SendUpdatePlayer()) ||
                        (!x.lastSending.Bit(1) && !await x.SendHP()) ||
                        (!x.lastSending.Bit(2) && !await x.SendZones()) ||
                        (!x.lastSending.Bit(3) && !await x.Send(40, x.ID, (short)-1)))
                        continue;

                    x.lastSending = 0;
                }
                timer2 = DateTime.Now;
            }
            Threads.Remove(server);
        }

        bool UpdateBuffs(int interval)
        {
            var updated = false;
            for (int i = 0; i < Player.Buffs.Length; i++)
            {
                ref var x = ref Player.Buffs[i];
                updated = x.Update(interval) || updated;
            }
            return updated;
        }
        #endregion

        #region Отправка пакетов (важное)
        /// <summary>
        /// Packet №4
        /// </summary>
        public async Task<bool> SendPlayerInfo()
        {
            return await Send(4,
                ID,
                Player.Skin,
                Player.Hair,
                Player.Name,
                Player.HairDye,
                Player.HideVisuals,
                Player.HideMisc,
                Player.HairC,
                Player.SkinC,
                Player.EyeC,
                Player.ShirtC,
                Player.UnderShirtC,
                Player.PantsC,
                Player.ShoeC,
                (byte)Player.Difficulty,
                (byte)Player.TorchFlag,
                Player.UsedThings);
        }

        /// <summary>
        /// Packet №5
        /// </summary>
        /// <param name="slot">Strictly not less than 0 and not more than 260.</param>
        public async Task<bool> SendSlot(int slot)
        {
            if (slot < 0 || slot >= Player.Inventory.Length)
                return false;
            return await Send(5, ID, (short)slot, Player.Inventory[slot]);
        }

        /// <summary>
        /// Packet №12
        /// </summary>
        /// <param name="context">0 = Death, 1 = Connection, 2 = Recall from item</param>
        /// <param name="X">Default is the world spawn point.</param>
        /// <param name="Y">Default is the world spawn point.</param>
        public async Task<bool> SendSpawn(byte context = 1, int X = -1, int Y = -1)
        {
            return await Send(12, ID, (short)X, (short)Y, 0, 0, context);
        }

        /// <summary>
        /// Packet №13
        /// </summary>
        public async Task<bool> SendUpdatePlayer()
        {
            var bytes = new List<byte>();
            if (Player.Velocity != PointF.Empty)
            {
                Player.Pulley |= Pulley.UpdateVelocity;
                bytes.AddRange(Player.Velocity.GetBytes());
            }
            else
            {
                Player.Pulley ^= Pulley.UpdateVelocity;
            }
            if (Player.OriginalPos != PointF.Empty)
            {
                Player.Miscs |= Miscs.UsedPotionofReturn;
                bytes.AddRange(Player.OriginalPos.GetBytes());
                bytes.AddRange(Player.HomePos.GetBytes());
            }
            else
            {
                Player.Miscs ^= Miscs.UsedPotionofReturn;
            }
            return await Send(13, 
                ID,
                (byte)Player.Control,
                (byte)Player.Pulley,
                (byte)Player.Miscs,
                Player.Sleeping,
                Player.SelectedSlot,
                Player.Position,
                bytes.ToArray());
        }

        /// <summary>
        /// Packet №16
        /// </summary>
        public async Task<bool> SendHP()
        {
            return await Send(16, ID, Player.HP, Player.FullHP);
        }

        /// <summary>
        /// Packet №68 
        /// </summary>
        public async Task<bool> SendUUID()
        {
            return await Send(68, UUID = UUID ?? Guid.NewGuid().ToString());
        }

        /// <summary>
        /// Packet №50
        /// </summary>
        public async Task<bool> SendBuffs()
        {
            return await Send(50, ID, Player.Buffs.Select(x => x.Type).ToArray());
        }

        /// <summary>
        /// Packet №42
        /// </summary>
        public async Task<bool> SendMana()
        {
            return await Send(42, ID, Player.Mana, Player.FullMana);
        }

        /// <summary>
        /// Packet №36
        /// </summary>
        public async Task<bool> SendZones()
        {
            return await Send(36, ID, Zones);
        }

        /// <summary>
        /// Packet №147
        /// </summary>
        public async Task<bool> SendLoadoutsSync()
        {
            return await Send(147, ID, Player.CurrentLoadout);
        }

        /// <param name="data">Packet data.</param>
        /// <param name="packet">Packet type (Id).</param>
        public async Task<bool> Send(byte packet, params object[] data)
        {
            return await Send(new Packet
            {
                Type = packet,
                Data = Packet.GetData(data)
            });
        }

        public async Task<bool> Send(Packet packet)
        {
            if (State < ConnectionState.ConnectRequested)
                return false;

            if (State != ConnectionState.Connected && packet.Type > 12 && !new byte[] { 93, 16, 42, 50, 147, 38, 68 }.Contains(packet.Type))
                return false;

            if (Hooks.HandleSendData(packet, this) == Hooks.Result.Cancel)
                return false;

            switch (packet.Type)
            {
                case 13:
                    lastSending |= 1;
                    break;
                case 16:
                    lastSending |= 2;
                    break;
                case 36:
                    lastSending |= 4;
                    break;
                case 40:
                    lastSending |= 8;
                    break;
            }

            try
            {
                await stream.WriteAsync(packet.GetBytes(), 0, packet.Length);
                return true;
            }
            catch (ObjectDisposedException ex)
            {
                Hooks.HandleExceptions(ex, this);
                return false;
            }
            catch (IOException ex)
            {
                Hooks.HandleExceptions(ex, this);
                return false;
            }
        }
        #endregion

        #region Чтение пакетов
        async void ReadData(Packet packet)
        {
            if (Hooks.HandleGetData(packet, this) == Hooks.Result.Cancel)
                return;

            try
            {
                switch (packet.Type)
                {
                    case 2:
                        using (var r = packet.Reader)
                            Utils.ConsoleError("Bot", Name, $"Kicked for '{r.ReadNetworkText()}'.");
                        Disconnect();
                        break;

                    case 3:
                        ID = packet.Data[0];
                        if (!await SendPlayerInfo() ||
                            !await SendUUID() ||
                            !await SendHP() ||
                            !await SendMana() ||
                            !await SendBuffs() ||
                            !await SendLoadoutsSync())
                            return;

                        for (short slot = 0; slot < Player.Inventory.Length; slot++)
                            if (!await SendSlot(slot))
                                return;

                        if (!await Send(6))
                            return;

                        if (State == ConnectionState.ConnectRequested)
                            State = ConnectionState.WorldInfoRequested;
                        break;

                    case 7:
                        if (State == ConnectionState.WorldInfoRequested)
                        {
                            State = ConnectionState.GetSectionRequested;
                            if (!await Send(8, -1, -1))
                                return;
                        }
                        using (var r = packet.Reader)
                        {
                            r.ReadInt32(); // Time
                            r.ReadByte(); // Moon
                            r.ReadByte(); // Moon Phase
                            Server.Area = new Rectangle(0, 0, r.ReadInt16(), r.ReadInt16());
                            Server.Spawn = r.ReadPoint16();
                            r.ReadInt16(); // Surface Layer
                            r.ReadInt16(); // Rock Layer
                            r.ReadInt32(); // ID
                            Server.Name = r.ReadString();
                        }
                        break;

                    case 10:
                        if (State == ConnectionState.GetSectionRequested)
                        {
                            State = ConnectionState.SpawnRequested;
                            await SendSpawn();
                        }
                        break;

                    case 36:
                        if (ID == packet.Data[0])
                        {
                            Zones[0] = packet.Data[1];
                            Zones[1] = packet.Data[2];
                            Zones[2] = packet.Data[3];
                            Zones[3] = packet.Data[4];
                        }
                        break;

                    case 37:
                        await Send(38, Server.Password);
                        break;

                    case 39:
                        using (var r = packet.Reader)
                        {
                            if (r.ReadInt16() == 400)
                                await Send(22, (short)400, (byte)255);
                        }
                        break;

                    case 49:
                    case 129:
                        if (State == ConnectionState.Connected)
                            break;

                        State = ConnectionState.Connected;
                        Utils.ConsoleSuccess("Bot", Name, "Connected.");

                        Player.Position = new PointF(Server.Spawn.X * 16f, (Server.Spawn.Y * 16f) - 48f);
                        await Send(22, (short)400, (byte)255);
                        await Send(82, (short)6, (ushort)14);
                        await SendZones();
                        await SendUpdatePlayer();
                        break;

                    case 82:
                        using (var r = packet.Reader)
                            if (r.ReadByte() == 1 && r.ReadByte() == 0 && r.ReadByte() == 255)
                            {
                                if (this == Reader(Server))
                                    Hooks.HandleServerChat(r.ReadNetworkText(), r.ReadColor(), this);
                            }
                        break;

                    case 118:
                        if (ID == packet.Data[0])
                            await SendSpawn();
                        break;

                    case 65:
                        if (packet.Data[0].Bit(0))
                        {
                            break;
                        }
                        using (var r = packet.Reader)
                        {
                            r.BaseStream.Position++;
                            if (r.ReadInt16() != ID)
                                break;

                            Player.Position.X = r.ReadSingle();
                            Player.Position.Y = r.ReadSingle();
                            await SendUpdatePlayer();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Hooks.HandleExceptions(ex, this);
                Utils.ConsoleError("Bot", Name, $"Packet {packet.Type} caused an error {ex.GetType().Name}.");
            }
        }
        
        async Task<bool> ReadPacket()
        {
            if (State < ConnectionState.ConnectRequested)
                return false;

            if (!stream.DataAvailable)
            {
                if ((DateTime.Now - lastResponseTime).TotalMilliseconds > ResponseWaitingTime)
                {
                    Utils.ConsoleError("Bot", Name, $"Connection timeout expired ({ResponseWaitingTime}ms).");
                    Disconnect();
                }
                return false;
            }
            lastResponseTime = DateTime.Now;

            int read;
            byte[] data = new byte[2];

            for (int i = 0; i < 2; i++)
            {
                read = stream.ReadByte();
                if (read == -1)
                    return false;
                else
                    data[i] = (byte)read;
            }

            int count = BitConverter.ToUInt16(data, 0);
            if (count < 3)
                return false;

            read = stream.ReadByte();
            if (read == -1)
                return false;

            count -= 3;
            data = new byte[count];
            while (count > 0)
            {
                if (stream.DataAvailable)
                {
                    count -= await stream.ReadAsync(data, data.Length - count, count);
                }
                else if ((DateTime.Now - lastResponseTime).TotalMilliseconds > ResponseWaitingTime)
                {
                    Utils.ConsoleError("Bot", Name, $"Connection timeout expired ({ResponseWaitingTime}ms).");
                    Disconnect();
                    return false;
                }
            }
            ReadData(new Packet()
            {
                Type = (byte)read,
                Data = data
            });
            return true;
        }
        #endregion

        #region Отправка пакетов (остальное)
        /// <summary>
        /// Packet №27
        /// </summary>
        public async Task<bool> SendProjectile(int projId, short type, PointF position, PointF velocity = new PointF(), short damage = 0, float knockback = 0f, float ai1 = 0, float ai2 = 0, short projUUID = 0)
        {
            byte flags = 0;
            List<byte> bytes = new List<byte>();
            if (ai1 != 0)
            {
                flags |= 1;
                bytes.AddRange(BitConverter.GetBytes(ai1));
            }
            if (ai2 != 0)
            {
                flags |= 2;
                bytes.AddRange(BitConverter.GetBytes(ai2));
            }
            if (damage != 0)
            {
                flags |= 16;
                bytes.AddRange(BitConverter.GetBytes(damage));
            }
            if (knockback != 0)
            {
                flags |= 32;
                bytes.AddRange(BitConverter.GetBytes(knockback));
            }
            if (projUUID != 0)
            {
                flags |= 128;
                bytes.AddRange(BitConverter.GetBytes(projUUID));
            }
            return await Send(27, (short)projId, position, velocity, ID, type, flags, bytes.ToArray());
        }

        /// <summary>
        /// Packet №82
        /// </summary>
        public async Task<bool> SendMessage(string text)
        {
            return await Send(82, (ushort)1, (byte)0, text ?? "Hey! I'm a stupid bot :D");
        }
        #endregion

        #region Bots и Reader
        public static Bot Reader(string ip, ushort port)
        {
            if (List.Count == 0)
                return null;

            foreach (var x in List)
            {
                if (x.Key.IP != ip || x.Key.Port != port)
                    continue;

                foreach (var y in x.Value)
                    if (y.State > ConnectionState.Connecting)
                        return y;
            }
            return null;
        }

        public static Bot Reader(Server server)
        {
            if (server == null)
                return null;
            return Reader(server.IP, server.Port);
        }

        public static List<Bot> Bots(string ip, ushort port, Func<Bot, bool> selector = null)
        {
            var list = new List<Bot>();
            if (List.Count == 0)
                return list;

            foreach (var x in List)
            {
                if (x.Key.IP != ip || x.Key.Port != port)
                    continue;

                foreach (var y in x.Value)
                    if (y.State != ConnectionState.Disconnected && 
                        selector == null ? true : selector(y))
                        list.Add(y);
            }
            return list;
        }

        public static List<Bot> Bots(Server server, Func<Bot, bool> selector = null)
        {
            if (server == null)
                return new List<Bot>();
            return Bots(server.IP, server.Port, selector);
        }
        #endregion
    }
}
