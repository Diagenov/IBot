using IBot;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;

namespace Musician
{
    public static class Tools
    {
        public static readonly List<FileInfo> Midies = new List<FileInfo>()
        {
            new FileInfo(Path.Combine("All.mid")),
        };
        public const int Count = 3;
        public const float HalfTone = 0.0833333358f;

        public static float GetNote(this SevenBitNumber noteID)
        {
            var id = (int)noteID;

            while (id < 60)
                id += 12;
            while (id > 84)
                id -= 12;

            switch (id)
            {
                case 60:
                    return -1f;
                case 61:
                    return -1f + HalfTone;
                case 62:
                    return -1f + 2f * HalfTone;
                case 63:
                    return -1f + 3f * HalfTone;
                case 64:
                    return -1f + 4f * HalfTone;
                case 65:
                    return -1f + 5f * HalfTone;
                case 66:
                    return -1f + 6f * HalfTone;
                case 67:
                    return -1f + 7f * HalfTone;
                case 68:
                    return -1f + 8f * HalfTone;
                case 69:
                    return -1f + 9f * HalfTone;
                case 70:
                    return -1f + 10f * HalfTone;
                case 71:
                    return -1f + 11f * HalfTone;
                case 72:
                    return 0f;
                case 73:
                    return HalfTone;
                case 74:
                    return HalfTone * 2f;
                case 75:
                    return HalfTone * 3f;
                case 76:
                    return HalfTone * 4f;
                case 77:
                    return HalfTone * 5f;
                case 78:
                    return HalfTone * 6f;
                case 79:
                    return HalfTone * 7f;
                case 80:
                    return HalfTone * 8f;
                case 81:
                    return HalfTone * 9f;
                case 82:
                    return HalfTone * 10f;
                case 83:
                    return HalfTone * 11f;
                case 84:
                    return 1f;
                default:
                    return float.MaxValue;
            }
        }

        public static bool LoadMidies()
        {
            var path = Path.Combine("midies");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return false;
            }

            Midies.AddRange(new DirectoryInfo(path)
                .GetFiles()
                .Where(i => i.Extension == ".midi" || i.Extension == ".mid"));

            if (Midies.Count == 1)
                return false;

            Console.WriteLine(new string('—', 30));
            for (int i = 0; i < Midies.Count; i++)
                Console.WriteLine($" [{i}] {Midies[i].Name}");
            Console.Write(new string('—', 30));
            return true;
        }

        public static async void PreparePlaying(Server server, params Midi[] midies)
        {
            var items = new int[Count]
            {
                494, 507, 508//, 1305, 4673
            };
            foreach (var midi in midies)
            {
                await Task.Delay(15000);
                var bots = Bot.Bots(server);
                if (bots.Count == 0)
                {
                    break;
                }
                for (int i = 0; i < bots.Count; i++)
                {
                    bots[i].Player.Inventory[0] = new Item(items[i], 1, 7);
                    bots[i].Player.SelectedSlot = 0;
                    await bots[i].SendSlot(0);
                    await bots[i].SendUpdatePlayer();
                }
                await StartPlaying(bots, midi.MidiFile);
            }
        }

        static async Task StartPlaying(List<Bot> bots, MidiFile midi)
        {
            var dictionary = new Dictionary<int, int>();
            var channels = new Dictionary<int, MidiBot>();
            int index = -1;

            foreach (var i in midi.GetNotes())
            {
                if (!dictionary.ContainsKey(i.Channel))
                {
                    dictionary.Add(i.Channel, 0);
                }
                dictionary[i.Channel]++;
            }
            foreach (var i in dictionary/*.OrderByDescending(x => x.Value)*/)
            {
                channels.Add(i.Key, new MidiBot
                {
                    Bot = bots[++index],
                    LastTime = -1
                });
                //Console.WriteLine($" + + + [{i.Key} channel] = {i.Value} notes ({bots[index].Name})");
            }

            using (var playback = midi.GetPlayback())
            {
                playback.NoteCallback = (note, time, length, playbackTime) =>
                {
                    if (!channels.TryGetValue(note.Channel, out var i))
                    {
                        return note;
                    }
                    if (i.LastTime != time)
                    {
                        i.Bot.Send(58, i.Bot.ID, note.NoteNumber.GetNote());
                    }
                    i.LastTime = time;
                    return note;
                };

                playback.Start();
                while (playback.IsRunning)
                {
                    await Task.Delay(100);
                }
            }
        }

        public struct Midi
        {
            public MidiFile MidiFile;
            public FileInfo FileInfo;
        }
    
        public class MidiBot
        {
            public Bot Bot;
            public long LastTime;
        }
    }
}
