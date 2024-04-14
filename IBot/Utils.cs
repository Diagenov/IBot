using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

namespace IBot
{
    public static class Utils
    {
        public static Random Rand = new Random();

        /// <summary>
        /// "[head] topic: message with args"
        /// </summary>
        /// <typeparam name="T">The class calling this method.</typeparam>
        public static void ConsoleError(string head, string topic, string message, params object[] args)
        {
            ConsoleMessage(head, topic, message, Color.IndianRed, args);
        }

        /// <summary>
        /// "[head] topic: message with args"
        /// </summary>
        /// <typeparam name="T">The class calling this method.</typeparam>
        public static void ConsoleInfo(string head, string topic, string message, params object[] args)
        {
            ConsoleMessage(head, topic, message, Color.Yellow, args);
        }

        /// <summary>
        /// "[head] topic: message with args"
        /// </summary>
        /// <typeparam name="T">The class calling this method.</typeparam>
        public static void ConsoleSuccess(string head, string topic, string message, params object[] args)
        {
            ConsoleMessage(head, topic, message, Color.LightGreen, args);
        }

        /// <summary>
        /// "[head] topic: message with args"
        /// </summary>
        /// <typeparam name="T">The class calling this method.</typeparam>
        public static void ConsoleMessage(string head, string topic, string message, Color color, params object[] args)
        {
            Hooks.HandleConsole(new Message($"[{head}] {topic}: {string.Format(message, args)}", color));
        }

        public static void Write(this BinaryWriter w, Color color)
        {
            w.Write(color.R);
            w.Write(color.G);
            w.Write(color.B);
        }

        public static void Write(this BinaryWriter w, PointF vector)
        {
            w.Write(vector.X);
            w.Write(vector.Y);
        }

        public static void Write(this BinaryWriter w, Point point)
        {
            w.Write(point.X);
            w.Write(point.Y);
        }

        public static void Write(this BinaryWriter w, Item item)
        {
            w.Write(item.Stack);
            w.Write(item.Prefix);
            w.Write(item.ItemID);
        }

        public static PointF ReadVector(this BinaryReader r)
        {
            return new PointF(r.ReadSingle(), r.ReadSingle());
        }

        public static Color ReadColor(this BinaryReader r)
        {
            return Color.FromArgb(r.ReadByte(), r.ReadByte(), r.ReadByte());
        }

        public static Point ReadPoint16(this BinaryReader r)
        {
            return new Point(r.ReadInt16(), r.ReadInt16());
        }

        public static Item ReadItem(this BinaryReader r, ReadItemOrder order)
        {
            var i = new Item();
            if (order == ReadItemOrder.IdPrefixStack)
            {
                i.ItemID = r.ReadInt16();
                i.Prefix = r.ReadByte();
                i.Stack = r.ReadInt16();
            }
            else
            {
                i.Stack = r.ReadInt16();
                i.Prefix = r.ReadByte();
                i.ItemID = r.ReadInt16();
            }
            return i;
        }

        public static Buff ReadBuff(this BinaryReader r)
        {
            return new Buff(r.ReadUInt16(), r.ReadInt32() * 1000 / 60);
        }

        public static string ReadNetworkText(this BinaryReader r)
        {
            return r.ReadNetworkText(0);
        }

        static string ReadNetworkText(this BinaryReader r, int count)
        {
            var mode = r.ReadByte();
            var text = r.ReadString();

            if (mode != 0 && count < 3)
            {
                var strs = new string[r.ReadByte()];
                for (byte i = 0; i < strs.Length; i++)
                    strs[i] = r.ReadNetworkText(count + 1);
                return string.Format(text, strs);
            }
            return text;
        }

        /// <param name="index">Bit number.</param>
        public static bool Bit(this byte x, int index)
        {
            if (index > 7)
                index = 7;
            byte bit = 1;
            for (byte i = 0; i < index; i++)
                bit *= 2;
            return (x & bit) == bit;
        }

        public static IEnumerable<byte> GetBytes(this PointF x)
        {
            return BitConverter.GetBytes(x.X).Concat(BitConverter.GetBytes(x.Y));
        }

        public static Point RandomPoint(this Rectangle x)
        {
            return new Point(Rand.Next(x.X, x.X + x.Width), Rand.Next(x.Y, x.Y + x.Height));
        }

        public static char RandomChar(this string x)
        {
            return x[Rand.Next(x.Length)];
        }

        public static Color RandomColor()
        {
            var bytes = new byte[3];
            Rand.NextBytes(bytes);
            return Color.FromArgb(bytes[0], bytes[1], bytes[2]);
        }

        public static string RandomString(int length)
        {
            var bytes = new byte[length];
            Rand.NextBytes(bytes);
            return Encoding.UTF8.GetString(bytes);
        }

        public static string CreateName(int minLength = 2, int maxLength = 5)
        {
            var name = "";
            var i = "aeiouy";
            var j = "bdfghjklmnprstvxzc";

            string[] k = { "ee", "oo", "ea", "yo", "ui", "ou", "ya", "ue", "oe", "ae" };
            string[] l = { "th", "sh", "cl", "ll", "str", "fl", "ch" };

            var flag = Rand.Next(2) == 1;
            var count = Rand.Next(minLength, maxLength);

            if (flag = !flag)
                name += char.ToUpper(i.RandomChar());
            else
                name += char.ToUpper(j.RandomChar());

            while (name.Length <= count)
            {
                if (flag = !flag)
                {
                    if (Rand.Next(13) == 7)
                        name += k[Rand.Next(k.Length)];
                    else
                        name += i.RandomChar(); 
                }
                else
                {
                    if (Rand.Next(13) == 7)
                        name += l[Rand.Next(l.Length)];
                    else
                        name += j.RandomChar(); 
                }
            }
            return name;
        }

        public static void Write(this BinaryWriter w, params object[] args)
        {
            if (args == null)
                return;

            foreach (var x in args)
            {
                if (x is bool)
                    w.Write((bool)x);
                else if (x is byte)
                    w.Write((byte)x);
                else if (x is byte[])
                    w.Write((byte[])x);
                else if (x is short)
                    w.Write((short)x);
                else if (x is ushort)
                    w.Write((ushort)x);
                else if (x is ushort[])
                    foreach (var j in (ushort[])x)
                        w.Write(j);
                else if (x is int)
                    w.Write((int)x);
                else if (x is long)
                    w.Write((long)x);
                else if (x is float)
                    w.Write((float)x);
                else if (x is string)
                    w.Write((string)x);
                else if (x is Color)
                    w.Write((Color)x);
                else if (x is PointF)
                    w.Write((PointF)x);
                else if (x is Item)
                    w.Write((Item)x);
                else if (x is Point)
                    w.Write((Point)x);
            }

            for (int i = 0; i < args.Length; i++)
            {
                
            }
        }
    }
}
