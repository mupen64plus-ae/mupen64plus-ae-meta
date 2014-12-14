﻿using System;
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

        public FormMain()
        {
            InitializeComponent();
        }

        private void buttonWiki_Click(object sender, EventArgs e)
        {
            Dictionary<string, RomEntry> roms = LoadRomData("mupen64plus.ini");
            Dictionary<string, GamePage> pages = AggregateGamePages(roms);
            GenerateWiki(pages);
        }

        private void buttonValidate_Click(object sender, EventArgs e)
        {
            Dictionary<string, RomEntry> roms = LoadRomData("mupen64plus.ini");
            Dictionary<string, GamePage> pages = AggregateGamePages(roms);
            ValidateDatabase(roms, pages);
        }

        private static Dictionary<string, RomEntry> LoadRomData(string filename)
        {
            Regex md5 = new Regex(@"^\[([A-Fa-f0-9]+)\]$");
            Regex goodname = new Regex("GoodName=(.+)");
            Regex crc = new Regex("CRC=(.+)");
            Regex refmd5 = new Regex("RefMD5=(.+)");
            Regex status = new Regex("Status=(.+)");
            Regex players = new Regex("Players=(.+)");
            Regex savetype = new Regex("SaveType=(.+)");
            Regex rumble = new Regex("Rumble=(.+)");

            Dictionary<string, RomEntry> roms = new Dictionary<string, RomEntry>();
            RomEntry rom = new RomEntry(SECTIONLESS);
            StreamReader reader = new StreamReader(filename);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (md5.IsMatch(line))
                {
                    string value = md5.Match(line).Groups[1].Value;
                    roms.Add(rom.md5, rom);
                    rom = new RomEntry(value);
                }

                if (goodname.IsMatch(line))
                    rom.goodname = goodname.Match(line).Groups[1].Value;

                if (crc.IsMatch(line))
                    rom.crc = crc.Match(line).Groups[1].Value;

                if (refmd5.IsMatch(line))
                    rom.refmd5 = refmd5.Match(line).Groups[1].Value;

                if (status.IsMatch(line))
                    rom.status = status.Match(line).Groups[1].Value;

                if (players.IsMatch(line))
                    rom.players = players.Match(line).Groups[1].Value;

                if (savetype.IsMatch(line))
                    rom.savetype = savetype.Match(line).Groups[1].Value;

                if (rumble.IsMatch(line))
                    rom.rumble = rumble.Match(line).Groups[1].Value;
            }
            roms.Remove(SECTIONLESS);
            reader.Close();

            foreach (RomEntry entry in roms.Values)
            {
                RomEntry master;
                if (entry.refmd5 != null && (master = roms[entry.refmd5]) != null)
                {
                    entry.status = master.status;
                    entry.players = master.players;
                    entry.savetype = master.savetype;
                    entry.rumble = master.rumble;
                }
            }
            return roms;
        }

        private static Dictionary<string, GamePage> AggregateGamePages(Dictionary<string, RomEntry> roms)
        {
            Dictionary<string, GamePage> pages = new Dictionary<string, GamePage>();
            foreach (RomEntry rom in roms.Values)
            {
                string title = rom.BaseName;
                GamePage page;
                if (pages.ContainsKey(title))
                {
                    page = pages[title];
                }
                else
                {
                    page = new GamePage(title);
                    pages.Add(title, page);
                }
                page.roms.Add(rom);
            }
            return pages;
        }

        private void GenerateWiki(Dictionary<string, GamePage> games)
        {
            DotNetWikiBot.Site site = new DotNetWikiBot.Site("http://littleguy77.wikia.com/");

            richTextBox1.Clear();
            GenerateGamePages(games, site);
            GenerateHome(games, site);
            richTextBox1.AppendText("\nFinished generating " + games.Count + " games.");
        }

        private void GenerateGamePages(Dictionary<string, GamePage> games, DotNetWikiBot.Site site)
        {
            //foreach (var page in games.Values)
            for (int i = 0; i < games.Count; i++)
            {
                GamePage page = games.ElementAt(i).Value;

                richTextBox1.AppendText(page.title + "\n");
                Application.DoEvents();

                //if (i < 10)
                {
                    DotNetWikiBot.Page p = new DotNetWikiBot.Page(site, page.title);
                    p.Load();
                    p.text = page.BodyText;
                    p.AddToCategory("Games");
                    p.Save();
                }
            }
        }

        private void GenerateHome(Dictionary<string, GamePage> games, DotNetWikiBot.Site site)
        {
            List<string> titles = games.Keys.ToList();
            titles.Sort();

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("==Games==\n");
            builder.AppendLine("{| class=\"wikitable sortable\"");
            builder.AppendLine("|-");
            builder.AppendLine("! Name !! Status !! Players !! Rumble !! SaveType");
            for (int i = 0; i < titles.Count; i++)
            {
                GamePage page = games[titles[i]];
                string status = "Unknown";
                string players = "Unknown";
                string rumble = "Unknown";
                string savetype = "Unknown";
                if (page.roms.Count > 0)
                {
                    RomEntry firstRom = page.roms[0];
                    status = firstRom.status;
                    players = firstRom.players;
                    rumble = firstRom.rumble;
                    savetype = firstRom.savetype;
                }
                builder.AppendLine("|-");
                builder.AppendFormat("| [[{0}]] || {1} || {2} || {3} || {4}\n", page.title, status, players, rumble, savetype);
            }
            builder.AppendLine("|}");

            DotNetWikiBot.Page index = new DotNetWikiBot.Page(site, "AllGames");
            index.Load();
            index.text = builder.ToString();
            index.Save();
        }

        private void ValidateDatabase(Dictionary<string, RomEntry> roms, Dictionary<string, GamePage> pages)
        {
            richTextBox1.Clear();
            foreach (RomEntry rom in roms.Values)
            {
                if (rom.md5 == rom.refmd5)
                    richTextBox1.AppendText(rom.goodname + "  self reference\n");

                if (rom.refmd5 != null)
                {
                    if (!roms.ContainsKey(rom.refmd5))
                        richTextBox1.AppendText(rom.goodname + "  invalid reference\n");
                    else
                    {
                        var parent = roms[rom.refmd5];
                        if (parent.refmd5 != null)
                            richTextBox1.AppendText(rom.goodname + "  double-linked reference\n");
                        if (parent.BaseName != rom.BaseName)
                            richTextBox1.AppendText(rom.goodname + "  incorrect reference\n");
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
                foreach (RomEntry rom in page.roms)
                {
                    if (status != null && status != rom.status)
                        richTextBox1.AppendText(rom.goodname + "  status mismatch" + "\n");
                    status = rom.status;

                    if (players != null && players != rom.players)
                        richTextBox1.AppendText(rom.goodname + "  players mismatch" + "\n");
                    players = rom.players;

                    if (savetype != null && savetype != rom.savetype)
                        richTextBox1.AppendText(rom.goodname + "  savetype mismatch" + "\n");
                    savetype = rom.savetype;

                    if (rumble != null && rumble != rom.rumble)
                        richTextBox1.AppendText(rom.goodname + "  rumble mismatch" + "\n");
                    rumble = rom.rumble;

                    Application.DoEvents();
                }
            }
            richTextBox1.AppendText("\nFinished checking database.");
        }

        public class RomEntry
        {
            public readonly string md5;
            public string goodname = null;
            public string crc = null;
            public string refmd5 = null;
            public string status = null;
            public string players = null;
            public string savetype = null;
            public string rumble = null;

            public RomEntry(string md5)
            {
                this.md5 = md5;
            }

            public string SafeName
            {
                get
                {
                    return goodname.Replace('[', '(').Replace(']', ')');
                }
            }

            public string BaseName
            {
                get
                {
                    String basename = SafeName.Split('(')[0].Trim();
                    if (goodname.Contains("(Kiosk"))
                        basename += " (Kiosk Demo)";
                    return basename;
                }
            }

            public string Description
            {
                get
                {
                    StringBuilder builder = new StringBuilder();

                    if (refmd5 == null)
                    {
                        builder.AppendLine(";" + goodname);
                        builder.AppendLine(": MD5: " + md5);
                        builder.AppendLine(": CRC: " + crc);
                        builder.AppendLine(": Status: " + status);
                        builder.AppendLine(": Players: " + players);
                        builder.AppendLine(": Save Type: " + savetype);
                        builder.AppendLine(": Rumble: " + rumble);
                    }
                    else
                    {
                        builder.AppendLine(";" + goodname);
                        builder.AppendLine(": MD5: " + md5);
                        builder.AppendLine(": CRC: " + crc);
                        builder.AppendLine(": See: " + refmd5);
                    }

                    return builder.ToString();
                }
            }
        }

        public class GamePage
        {
            public readonly string title;
            public readonly List<RomEntry> roms;

            private const string artformat = @"[http://paulscode.com/downloads/Mupen64Plus-AE/CoverArt/{0}.png Link]";

            public GamePage(string title)
            {
                this.title = title;
                roms = new List<RomEntry>();
            }

            public string ArtName
            {
                get
                {
                    string name = title;
                    name = Regex.Replace(name, @"['\.]", "");
                    name = Regex.Replace(name, @"\W+", "_");
                    return name;
                }
            }

            public string BodyText
            {
                get
                {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine("==Cover Art==");
                    builder.AppendLine();
                    builder.AppendLine(string.Format(artformat, ArtName));
                    builder.AppendLine();
                    builder.AppendLine("==Known Versions==");
                    builder.AppendLine();
                    List<string> goodnames = roms.Select(r => r.goodname).ToList();
                    goodnames.Sort();
                    foreach (string goodname in goodnames)
                    {
                        builder.AppendLine(";" + goodname);
                    }
                    builder.AppendLine();
                    builder.AppendLine("==Known Issues==");
                    builder.AppendLine();
                    builder.AppendLine("==Recommended Settings==");
                    return builder.ToString();
                }
            }
        }
    }
}