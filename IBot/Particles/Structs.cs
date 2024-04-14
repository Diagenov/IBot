using System;
using System.IO;

namespace IBot
{
    public struct Packet
    {
        public byte Type;
        public byte[] Data;

        public ushort Length => (ushort)(Data.Length + 3);
        public BinaryReader Reader => new BinaryReader(new MemoryStream(Data, 0, Data.Length));

        public byte[] GetBytes()
        {
            using (var s = new MemoryStream())
            {
                using (var w = new BinaryWriter(s))
                {
                    w.Write(Length);
                    w.Write(Type);
                    w.Write(Data);
                }
                return s.ToArray();
            }
        }

        public static byte[] GetData(params object[] data)
        {
            using (var s = new MemoryStream())
            {
                using (var w = new BinaryWriter(s))
                    w.Write(data);
                return s.ToArray();
            }
        }
    }

    public struct Buff
    {
        public ushort Type;
        public int Time;

        public const ushort MaxType = 326;

        public static Buff Random
        {
            get => new Buff()
            {
                Type = (ushort)Utils.Rand.Next(MaxType + 1),
                Time = Utils.Rand.Next(int.MaxValue)
            };
        }

        /// <param name="time">milliseconds</param>
        public Buff(ushort type, int time)
        {
            Type = Math.Min(type, MaxType);
            Time = time;
        }

        /// <param name="milliseconds">Нужно как время, прошедшее с последнего вызова метода.</param>
        /// <returns>Указывает, был ли бафф обновлен на нуль (очищен).</returns>
        public bool Update(int milliseconds)
        {
            if (Type == 0)
                return false;

            if (Time <= milliseconds)
                Type = 0;
            else
                Time -= milliseconds;

            return Type == 0;
        }
    }

    public struct Item
    {
        public short Stack;
        public byte Prefix;
        public short ItemID;

        public const short MaxItemID = 5087;
        public const byte MaxPrefix = 83;

        public static Item Random
        {
            get => new Item()
            {
                Stack = 1,
                Prefix = (byte)Utils.Rand.Next(MaxPrefix + 1),
                ItemID = (short)Utils.Rand.Next(MaxItemID + 1)
            };
        }

        public Item(short itemID, short stack = 1, byte prefix = 0)
        {
            ItemID = Math.Max((short)0, Math.Min(itemID, MaxItemID));
            Stack = Math.Max((short)0, Math.Min(stack, (short)9999));
            Prefix = Math.Min(prefix, MaxPrefix);
        }
    }
}
