using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Mupen64PlusRomDataManager
{
    public class RomDetail
    {
        private const String ART_URL_TEMPLATE = "http://paulscode.com/downloads/Mupen64Plus-AE/CoverArt/{0}";

        public readonly string md5;
        public string crc = null;
        public string goodName = null;
        public string refmd5 = null;
        public string saveType = null;
        public string status = null;
        public string players = null;
        public string rumble = null;

        public RomDetail(string md5)
        {
            this.md5 = md5;
        }

        public string BaseName
        {
            get
            {
                var baseName = goodName;
                baseName = Regex.Split(baseName, @" \(")[0];
                baseName = Regex.Split(baseName, @" \[")[0];
                return baseName.Trim();
            }
        }

        public string ArtName
        {
            get
            {
                var artName = BaseName;
                artName = Regex.Replace(artName, @"['\.]", "");
                artName = Regex.Replace(artName, @"\W+", "_");
                return artName + ".png";
            }
        }

        public string ArtUrl
        {
            get
            {
                return String.Format(ART_URL_TEMPLATE, ArtName);
            }
        }

        public string WikiTitle
        {
            get
            {
                var wikiTitle = BaseName;
                if (goodName.Contains(" (Kiosk"))
                    wikiTitle += " (Kiosk Demo)";
                return wikiTitle;
            }
        }

        public string WikiLink
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                var wikiTitle = Regex.Replace(WikiTitle, @"\s", "-");
                for (int i = 0; i < wikiTitle.Length; i++)
                {
                    char cc = wikiTitle[i];
                    if (UnescapePredicate(cc))
                        builder.Append(cc);
                    else
                        builder.AppendFormat("%{0:X2}", (int)cc);
                }
                return builder.ToString();
            }
        }

        private static bool UnescapePredicate(char cc)
        {
            // a - z
            if (97 <= cc && cc <= 122)
                return true;
            // A - Z
            else if (65 <= cc && cc <= 90)
                return true;
            // 0 - 9
            else if (48 <= cc && cc <= 57)
                return true;
            // !
            else if (cc == 33)
                return true;
            // '()*
            else if (39 <= cc && cc <= 42)
                return true;
            // -.
            else if (45 <= cc && cc <= 46)
                return true;
            // _
            else if (cc == 95)
                return true;
            // ~
            else if (cc == 126)
                return true;
            else
                return false;
        }

        public string Description
        {
            get
            {
                StringBuilder builder = new StringBuilder();

                if (refmd5 == null)
                {
                    builder.AppendLine(";" + goodName);
                    builder.AppendLine(": MD5: " + md5);
                    builder.AppendLine(": CRC: " + crc);
                    builder.AppendLine(": Status: " + status);
                    builder.AppendLine(": Players: " + players);
                    builder.AppendLine(": Save Type: " + saveType);
                    builder.AppendLine(": Rumble: " + rumble);
                }
                else
                {
                    builder.AppendLine(";" + goodName);
                    builder.AppendLine(": MD5: " + md5);
                    builder.AppendLine(": CRC: " + crc);
                    builder.AppendLine(": See: " + refmd5);
                }

                return builder.ToString();
            }
        }
    }
}
