using System;
using System.IO;
using System.Drawing;

namespace IBot
{
    public delegate void ConsoleHandler(Message message);
    public delegate void ServerChatHandler(Message message, Bot bot);
    public delegate void DisconnectHandler(Bot bot);
    public delegate void ExceptionsHandler(Exception exception, Bot bot = null);
    public delegate Hooks.Result SendDataHandler(Packet packet, Bot bot);
    public delegate Hooks.Result ReadDataHandler(Packet packet, Bot bot);

    public static class Hooks
    {
        static bool crashLogs = false;
        public static bool CrashLogs
        {
            get => crashLogs;
            set 
            {
                if (crashLogs == value)
                    return;

                if (crashLogs = value)
                    AppDomain.CurrentDomain.UnhandledException += HandleException;
                else
                    AppDomain.CurrentDomain.UnhandledException -= HandleException;
            }
        }

        static void HandleException(object o, UnhandledExceptionEventArgs args)
        {
            var message = string.Format("[{0} - {1}] {2}\n\n\n",
                DateTime.Now.ToLongDateString(),
                DateTime.Now.ToLongTimeString(),
                args.ExceptionObject);

            var path = Path.Combine($"crash-logs.txt");
            var encoding = System.Text.Encoding.UTF8;

            using (var w = new StreamWriter(path, true, encoding))
                w.Write(message);
        }

        /// <summary>
        /// Called when IBot need to report you something.
        /// </summary>
        public static event ConsoleHandler Console;
        internal static void HandleConsole(Message message)
        {
            Console?.Invoke(message);
        }

        public static event ServerChatHandler ServerChat;
        internal static void HandleServerChat(string message, Color color, Bot bot)
        {
            ServerChat?.Invoke(new Message(message, color), bot);
        }

        /// <summary>
        /// Called when the bot disconnects.
        /// </summary>
        public static event DisconnectHandler Disconnect;
        internal static void HandleDisconnect(Bot bot)
        {
            Disconnect?.Invoke(bot);
        }

        public static event ExceptionsHandler Exceptions;
        internal static void HandleExceptions(Exception ex, Bot bot = null)
        {
            Exceptions?.Invoke(ex, bot);
        }

        /// <summary>
        /// Called when the bot sends a packet to the server. It is advisable not to block it when connecting.
        /// </summary>
        public static event SendDataHandler SendData;
        internal static Result HandleSendData(Packet packet, Bot bot)
        {
            return SendData == null ? Result.Continue : SendData.Invoke(packet, bot);
        }

        /// <summary>
        /// Called when the bot receives a packet from the server. It is advisable not to block it when connecting.
        /// </summary>
        public static event ReadDataHandler ReadData;
        internal static Result HandleGetData(Packet packet, Bot bot)
        {
            return ReadData == null ? Result.Continue : ReadData.Invoke(packet, bot);
        }

        public enum Result : byte
        {
            Cancel, Continue
        }
    }
}
