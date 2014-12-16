using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Mupen64PlusRomDataManager
{
    public partial class FormMain : Form
    {
        private const string SECTIONLESS = "SECTIONLESS";
        private const string OUT_FOLDER = "Pages";

        public FormMain()
        {
            InitializeComponent();
        }

        private void buttonWiki_Click(object sender, EventArgs e)
        {
            Dictionary<string, RomDetail> roms = LoadRomData("mupen64plus.ini");
            Dictionary<string, GamePage> pages = AggregateGamePages(roms);
            GenerateWiki(pages);
        }

        private void buttonValidate_Click(object sender, EventArgs e)
        {
            Dictionary<string, RomDetail> roms = LoadRomData("mupen64plus.ini");
            Dictionary<string, GamePage> pages = AggregateGamePages(roms);
            ValidateDatabase(roms, pages);
        }

        private static Dictionary<string, RomDetail> LoadRomData(string filename)
        {
            Regex md5 = new Regex(@"^\[([A-Fa-f0-9]+)\]$");
            Regex crc = new Regex("CRC=(.+)");
            Regex goodname = new Regex("GoodName=(.+)");
            Regex refmd5 = new Regex("RefMD5=(.+)");
            Regex savetype = new Regex("SaveType=(.+)");
            Regex status = new Regex("Status=(.+)");
            Regex players = new Regex("Players=(.+)");
            Regex rumble = new Regex("Rumble=(.+)");

            Dictionary<string, RomDetail> roms = new Dictionary<string, RomDetail>();
            RomDetail rom = new RomDetail(SECTIONLESS);
            StreamReader reader = new StreamReader(filename);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (md5.IsMatch(line))
                {
                    string value = md5.Match(line).Groups[1].Value;
                    roms.Add(rom.md5, rom);
                    rom = new RomDetail(value);
                }

                if (crc.IsMatch(line))
                    rom.crc = crc.Match(line).Groups[1].Value;

                if (goodname.IsMatch(line))
                    rom.goodName = goodname.Match(line).Groups[1].Value;

                if (refmd5.IsMatch(line))
                    rom.refmd5 = refmd5.Match(line).Groups[1].Value;

                if (savetype.IsMatch(line))
                    rom.saveType = savetype.Match(line).Groups[1].Value;

                if (status.IsMatch(line))
                    rom.status = status.Match(line).Groups[1].Value;

                if (players.IsMatch(line))
                    rom.players = players.Match(line).Groups[1].Value;

                if (rumble.IsMatch(line))
                    rom.rumble = rumble.Match(line).Groups[1].Value;
            }
            roms.Remove(SECTIONLESS);
            reader.Close();

            foreach (RomDetail entry in roms.Values)
            {
                RomDetail master;
                if (entry.refmd5 != null && (master = roms[entry.refmd5]) != null)
                {
                    entry.status = master.status;
                    entry.players = master.players;
                    entry.saveType = master.saveType;
                    entry.rumble = master.rumble;
                }
            }
            return roms;
        }

        private static Dictionary<string, GamePage> AggregateGamePages(Dictionary<string, RomDetail> roms)
        {
            Dictionary<string, GamePage> pages = new Dictionary<string, GamePage>();
            foreach (var rom in roms.Values)
            {
                var title = rom.WikiTitle;
                if (!pages.ContainsKey(title))
                {
                    pages.Add(title, new GamePage(title));
                }
                pages[title].roms.Add(rom);
            }
            return pages;
        }

        private void GenerateWiki(Dictionary<string, GamePage> games)
        {
            richTextBox.Clear();
            Directory.CreateDirectory(OUT_FOLDER);
            GenerateGamePages(games);
            GenerateHome(games);
            richTextBox.AppendText("\nFinished generating " + games.Count + " games.");
        }

        private void GenerateGamePages(Dictionary<string, GamePage> games)
        {
            foreach (var page in games.Values)
            {
                richTextBox.AppendText(page.title + "\n");
                Application.DoEvents();

                using (StreamWriter writer = new StreamWriter(OUT_FOLDER + "\\" + page.title + ".md"))
                {
                    writer.Write(page.BodyText);
                }
            }
        }

        private void GenerateHome(Dictionary<string, GamePage> games)
        {
            List<string> titles = games.Keys.ToList();
            titles.Sort();

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("## Games\n");
            builder.AppendLine("Name | Status | Players | Rumble | SaveType");
            builder.AppendLine("-----|--------|---------|--------|---------");
            for (int i = 0; i < titles.Count; i++)
            {
                var page = games[titles[i]];
                string wikiLink = "";
                string status = "Unknown";
                string players = "Unknown";
                string rumble = "Unknown";
                string savetype = "Unknown";
                if (page.roms.Count > 0)
                {
                    RomDetail firstRom = page.roms[0];
                    wikiLink = firstRom.WikiLink;
                    status = firstRom.status;
                    players = firstRom.players;
                    rumble = firstRom.rumble;
                    savetype = firstRom.saveType;
                }
                //builder.AppendFormat("[[{0}]] | {1} | {2} | {3} | {4}\n", page.title, status, players, rumble, saveType);
                builder.AppendFormat("[{0}]({1}) | {2} | {3} | {4} | {5}\n", page.title, wikiLink, status, players, rumble, savetype);
            }
 
            using (StreamWriter writer = new StreamWriter(OUT_FOLDER + "\\Home.md"))
            {
                writer.Write(builder.ToString());
            }
        }

        private void ValidateDatabase(Dictionary<string, RomDetail> roms, Dictionary<string, GamePage> pages)
        {
            richTextBox.Clear();
            foreach (RomDetail rom in roms.Values)
            {
                if (rom.md5 == rom.refmd5)
                    richTextBox.AppendText(rom.goodName + "  self reference\n");

                if (rom.refmd5 != null)
                {
                    if (!roms.ContainsKey(rom.refmd5))
                        richTextBox.AppendText(rom.goodName + "  invalid reference\n");
                    else
                    {
                        var parent = roms[rom.refmd5];
                        if (parent.refmd5 != null)
                            richTextBox.AppendText(rom.goodName + "  double-linked reference\n");
                        if (parent.BaseName != rom.BaseName)
                            richTextBox.AppendText(rom.goodName + "  incorrect reference\n");
                    }
                }
                Application.DoEvents();
            }

            foreach (GamePage page in pages.Values)
            {
                String status = null;
                String players = null;
                String savetype = null;
                String rumble = null;
                foreach (RomDetail rom in page.roms)
                {
                    if (status != null && status != rom.status)
                        richTextBox.AppendText(rom.goodName + "  status mismatch" + "\n");
                    status = rom.status;

                    if (players != null && players != rom.players)
                        richTextBox.AppendText(rom.goodName + "  players mismatch" + "\n");
                    players = rom.players;

                    if (savetype != null && savetype != rom.saveType)
                        richTextBox.AppendText(rom.goodName + "  saveType mismatch" + "\n");
                    savetype = rom.saveType;

                    if (rumble != null && rumble != rom.rumble)
                        richTextBox.AppendText(rom.goodName + "  rumble mismatch" + "\n");
                    rumble = rom.rumble;

                    Application.DoEvents();
                }
            }
            richTextBox.AppendText("\nFinished checking database.");
        }


        public class GamePage
        {
            public readonly string title;
            public readonly List<RomDetail> roms;

            private const string ART_FORMAT = @"![]({0})";

            public GamePage(string title)
            {
                this.title = title;
                roms = new List<RomDetail>();
            }

            public string BodyText
            {
                get
                {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine(string.Format(ART_FORMAT, roms.First().ArtUrl));
                    builder.AppendLine();
                    builder.AppendLine("## Recommended Settings");
                    builder.AppendLine();
                    builder.AppendLine("## Known Issues");
                    builder.AppendLine();
                    builder.AppendLine("## Known Versions");
                    builder.AppendLine();
                    foreach (var rom in roms.OrderBy(r => r.goodName))
                    {
                        builder.AppendLine("- `" + rom.goodName + "  " + rom.md5 + "`");
                    }
                    return builder.ToString();
                }
            }
        }
    }
}
