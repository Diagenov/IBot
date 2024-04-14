using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;

namespace IBot
{
    public class Player
    {
        string name = "default";
        public string Name
        {
            get => name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    return;
                name = value;
            }
        }

        public const byte MaxSkin = 9;
        public const byte MaxHair = 162;

        byte skin;
        public byte Skin
        {
            get => skin;
            set => skin = Math.Min(MaxSkin, value);
        }

        byte hair;
        public byte Hair
        {
            get => hair;
            set => hair = Math.Min(MaxHair, value);
        }

        public byte HairDye;
        public short HideVisuals;
        public byte HideMisc;
        public Color HairC;
        public Color SkinC;
        public Color EyeC;
        public Color ShirtC;
        public Color UnderShirtC;
        public Color PantsC;
        public Color ShoeC;
        public Difficulty Difficulty;
        public TorchFlag TorchFlag;
        public byte UsedThings;

        public short HP = 500;
        public short FullHP = 500;
        public short Mana = 200;
        public short FullMana = 200;

        public bool PvP;
        public Team Team;

        public readonly Buff[] Buffs = new Buff[44];
        /// <summary>
        /// 0 - 58 = Inventory, 59 - 78 = Armor, 79 - 88 = Dye, 89 - 93 MiscEquips, 94 - 98 = MiscDyes, 
        /// 99 - 138 = Piggy bank, 139 - 178 = Safe, 179 = Trash, 180 - 219 = Defender's Forge, 220 - 259 = Void Vault
        /// 260 - 289 = Loadout1, 290 - 319 = Loadout2, 320 - 359 = Loadout3
        /// </summary>
        public readonly Item[] Inventory = new Item[350];
        public IEnumerable<Item> Main => Inventory.Take(50);
        public IEnumerable<Item> Coins => Inventory.Skip(50).Take(4);
        public IEnumerable<Item> Ammo => Inventory.Skip(54).Take(4);
        public Item CursorItem => Inventory[58];
        public IEnumerable<Item> Armor => Inventory.Skip(59).Take(20);
        public IEnumerable<Item> Dye => Inventory.Skip(79).Take(10);
        public IEnumerable<Item> Misc => Inventory.Skip(89).Take(5);
        public IEnumerable<Item> MiscDye => Inventory.Skip(94).Take(5);
        public IEnumerable<Item> Piggy => Inventory.Skip(99).Take(40);
        public IEnumerable<Item> Safe => Inventory.Skip(139).Take(40);
        public Item TrashItem => Inventory[179];
        public IEnumerable<Item> Forge => Inventory.Skip(180).Take(40);
        public IEnumerable<Item> VoidBag => Inventory.Skip(220).Take(40);

        byte currentLoadout = 0;
        public byte CurrentLoadout
        {
            get => currentLoadout;
            set => currentLoadout = Math.Min((byte)2, value);
        }
        public Tuple<IEnumerable<Item>, IEnumerable<Item>> Loadout1 => 
            new Tuple<IEnumerable<Item>, IEnumerable<Item>>(Inventory.Skip(260).Take(20), Inventory.Skip(280).Take(10));
        public Tuple<IEnumerable<Item>, IEnumerable<Item>> Loadout2 => 
            new Tuple<IEnumerable<Item>, IEnumerable<Item>>(Inventory.Skip(290).Take(20), Inventory.Skip(310).Take(10));
        public Tuple<IEnumerable<Item>, IEnumerable<Item>> Loadout3 =>
            new Tuple<IEnumerable<Item>, IEnumerable<Item>>(Inventory.Skip(320).Take(20), Inventory.Skip(340).Take(10));

        byte selectedSlot;
        public byte SelectedSlot
        {
            get => selectedSlot;
            set => selectedSlot = Math.Min((byte)58, value);
        }

        public Control Control = Control.Direction;
        public Pulley Pulley;
        public Miscs Miscs;
        public bool Sleeping;
        public PointF Position;
        public PointF Velocity;
        public PointF OriginalPos;
        public PointF HomePos;

        public Point TilesPosition
        {
            get => new Point((int)(Position.X / 16f), (int)(Position.Y / 16f));
            set => Position = new PointF(value.X * 16f, value.Y * 16f);
        }
        public static string DefaultPath
        {
            get => Path.Combine("Players");
        }

        /// <param name="name">If equal to null, it's created randomly.</param>
        public Player(string name = null)
        {
            if (string.IsNullOrWhiteSpace(this.name = name))
                this.name = Utils.CreateName();
        }

        /// <summary>
        /// Sets random skin, inventory, buffs and direction.
        /// </summary>
        public void Random()
        {
            Skin = (byte)Utils.Rand.Next(MaxSkin + 1);
            Hair = (byte)Utils.Rand.Next(MaxHair + 1);
            HideVisuals = (short)Utils.Rand.Next(ushort.MaxValue);

            HairC = Utils.RandomColor();
            SkinC = Utils.RandomColor();
            EyeC = Utils.RandomColor();
            ShirtC = Utils.RandomColor();
            UnderShirtC = Utils.RandomColor();
            PantsC = Utils.RandomColor();
            ShoeC = Utils.RandomColor();

            for (int i = 0; i < 59; i++)
                Inventory[i] = Item.Random;
            SelectedSlot = 0;

            int count = Utils.Rand.Next(6);
            for (int i = 0; i < count; i++)
                if (Buffs[i].Type == 0)
                    Buffs[i] = Buff.Random;

            if (Utils.Rand.Next(2) == 0)
                Control |= Control.ControlLeft;
            else
                Control |= Control.ControlRight;
        }
    }
}
