using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace IBot
{
    public class Message
    {
        public string OriginalText { get; private set; }
        public Color DefaultColor { get; private set; }
        /// <summary>
        /// Formatted text which split into pieces.
        /// </summary>
        public List<string> Text { get; private set; }
        /// <summary>
        /// Colors of the corresponding parts from Message.Text.
        /// </summary>
        public List<Color> Colors { get; private set; }

        public static readonly Regex ItemConstruction = new Regex(@"\[[inag](?:\/[sp]\d+){0,2}:(\d+)\]", RegexOptions.IgnoreCase);
        public static readonly Regex ColorConstruction = new Regex(@"\[c\/([^:]*):([^\]]*)\]", RegexOptions.IgnoreCase);

        public Tuple<string, Color> GetSingleColorText
        {
            get => new Tuple<string, Color>(string.Join("", Text), DefaultColor);
        }

        /// <summary>
        /// Converts constructs like [i:xxxx] and [c/xxxxxx:text].
        /// </summary>
        /// <param name="text">Original text.</param>
        /// <param name="color">Default text color, where there are no constructions.</param>
        public Message(string text, Color color)
        {
            OriginalText = text;
            DefaultColor = color;
            Colors = new List<Color>();
            Text = new List<string>();

            string[] strArr = null;
            Color[] clrArr = null;

            text = ItemConstruction.Replace(text, "[$1]");

            if (ColorConstruction.IsMatch(text))
            {
                var matches = ColorConstruction.Matches(text).OfType<Match>().ToArray();
                strArr = new string[matches.Length];
                clrArr = new Color[matches.Length];

                int i = 0;
                string[] strs;
                foreach (var s in matches.Select(x => x.Value.Substring(3, x.Value.Length - 4)))
                {
                    strs = s.Split(':');
                    clrArr[i] = ColorTranslator.FromHtml('#' + strs[0].ToUpper());
                    strArr[i] = strs[1];
                    i++;
                }
                strs = ColorConstruction.Replace(text, "א").Split('א');
                int length = Math.Max(strArr.Length, strs.Length);

                for (i = 0; i < length; i++)
                {
                    if (!string.IsNullOrEmpty(strs[i]))
                    {
                        Colors.Add(color);
                        Text.Add(strs[i]);
                    }
                    if (i == length - 1 && strArr.Length != strs.Length)
                    {
                        break;
                    }
                    if (!string.IsNullOrEmpty(strArr[i]))
                    {
                        Colors.Add(clrArr[i]);
                        Text.Add(strArr[i]);
                    }
                }
                return;
            }
            Text.Add(text);
            Colors.Add(color);
        }
    }
}
