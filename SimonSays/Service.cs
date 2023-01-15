using Dalamud.Data;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Text;
using Dalamud.Game;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Utility;
using Lumina.Excel.GeneratedSheets;
using Lumina.Excel;
using Lumina.Text;
using SimonSays;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using XivCommon;

namespace SimonSays
{
    internal class Service
    {
        // Token: 0x06000031 RID: 49 RVA: 0x00002998 File Offset: 0x00000B98
        public static void InitializeEmotes()
        {
            Service.emoteCommands = Service.DataManager.GetExcelSheet<Emote>();
            if (Service.emoteCommands == null)
            {
                Service.ChatGui.Print("[PuppetMaster][Error] Failed to read Emotes list");
                return;
            }
            foreach (Emote emote in Service.emoteCommands)
            {
                TextCommand value = emote.TextCommand.Value;
                SeString cmd = (value != null) ? value.Command : null;
                if (cmd != null && cmd != "")
                {
                    Service.Emotes.Add(cmd);
                }
                TextCommand value2 = emote.TextCommand.Value;
                cmd = ((value2 != null) ? value2.ShortCommand : null);
                if (cmd != null && cmd != "")
                {
                    Service.Emotes.Add(cmd);
                }
                TextCommand value3 = emote.TextCommand.Value;
                cmd = ((value3 != null) ? value3.Alias : null);
                if (cmd != null && cmd != "")
                {
                    Service.Emotes.Add(cmd);
                }
                TextCommand value4 = emote.TextCommand.Value;
                cmd = ((value4 != null) ? value4.ShortAlias : null);
                if (cmd != null && cmd != "")
                {
                    Service.Emotes.Add(cmd);
                }
            }
            if (Service.Emotes.Count == 0)
            {
                Service.ChatGui.Print("[PuppetMaster][Error] Failed to build Emotes list");
            }
        }

        // Token: 0x06000033 RID: 51 RVA: 0x00002B43 File Offset: 0x00000D43
        public static string GetDefaultReplaceMatch()
        {
            return "/$1$2";
        }


        // Token: 0x06000035 RID: 53 RVA: 0x00002C04 File Offset: 0x00000E04
        public static Service.ParsedTextCommand FormatCommand(string command)
        {
            Service.ParsedTextCommand textCommand = new Service.ParsedTextCommand();
            if (command != string.Empty)
            {
                command = command.Trim();
                if (command.StartsWith('/'))
                {
                    command = command.Replace('[', '<').Replace(']', '>');
                    int space = command.IndexOf(' ');
                    textCommand.Main = ((space == -1) ? command : command.Substring(0, space)).ToLower();
                    string args;
                    if (space != -1)
                    {
                        string text = command;
                        int num = space + 1;
                        args = text.Substring(num, text.Length - num);
                    }
                    else
                    {
                        args = string.Empty;
                    }
                    textCommand.Args = args;
                }
                else
                {
                    textCommand.Main = command;
                }
            }
            return textCommand;
        }

        // Token: 0x06000036 RID: 54 RVA: 0x00002CA4 File Offset: 0x00000EA4
        public static Service.ParsedTextCommand GetTestInputCommand()
        {
            Service.ParsedTextCommand result = new Service.ParsedTextCommand();
            MatchCollection matches = usingRegex ? Service.CustomRx.Matches(Service.configuration.TestInput) : Service.Rx.Matches(Service.configuration.TestInput);
            if (matches.Count != 0)
            {
                result.Args = matches[0].ToString();
                try
                {
                    result.Main = (usingRegex ? Service.CustomRx.Replace(matches[0].Value, Service.configuration.ReplaceMatch) : Service.Rx.Replace(matches[0].Value, Service.GetDefaultReplaceMatch()));
                }
                catch (Exception)
                {
                }
            }
            result.Main = Service.FormatCommand(result.Main).ToString();
            return result;
        }

        // Token: 0x06000037 RID: 55 RVA: 0x00002D9C File Offset: 0x00000F9C
        public static void InitializeConfig()
        {
            Service.configuration = ((Service.PluginInterface.GetPluginConfig() as Configuration) ?? new Configuration());
            Service.configuration.Initialize(Service.PluginInterface);
            if ((long)Service.configuration.EnabledChannels.Count != 23L)
            {
                Service.configuration.EnabledChannels = new List<ChannelSetting>
                {
                    new ChannelSetting
                    {
                        ChatType = 37,
                        Name = "CWLS1"
                    },
                    new ChannelSetting
                    {
                        ChatType = 101,
                        Name = "CWLS2"
                    },
                    new ChannelSetting
                    {
                        ChatType = 102,
                        Name = "CWLS3"
                    },
                    new ChannelSetting
                    {
                        ChatType = 103,
                        Name = "CWLS4"
                    },
                    new ChannelSetting
                    {
                        ChatType = 104,
                        Name = "CWLS5"
                    },
                    new ChannelSetting
                    {
                        ChatType = 105,
                        Name = "CWLS6"
                    },
                    new ChannelSetting
                    {
                        ChatType = 106,
                        Name = "CWLS7"
                    },
                    new ChannelSetting
                    {
                        ChatType = 107,
                        Name = "CWLS8"
                    },
                    new ChannelSetting
                    {
                        ChatType = 16,
                        Name = "LS1"
                    },
                    new ChannelSetting
                    {
                        ChatType = 17,
                        Name = "LS2"
                    },
                    new ChannelSetting
                    {
                        ChatType = 18,
                        Name = "LS3"
                    },
                    new ChannelSetting
                    {
                        ChatType = 19,
                        Name = "LS4"
                    },
                    new ChannelSetting
                    {
                        ChatType = 20,
                        Name = "LS5"
                    },
                    new ChannelSetting
                    {
                        ChatType = 21,
                        Name = "LS6"
                    },
                    new ChannelSetting
                    {
                        ChatType = 22,
                        Name = "LS7"
                    },
                    new ChannelSetting
                    {
                        ChatType = 23,
                        Name = "LS8"
                    },
                    new ChannelSetting
                    {
                        ChatType = 13,
                        Name = "Tell"
                    },
                    new ChannelSetting
                    {
                        ChatType = 10,
                        Name = "Say",
                        Enabled = true
                    },
                    new ChannelSetting
                    {
                        ChatType = 14,
                        Name = "Party"
                    },
                    new ChannelSetting
                    {
                        ChatType = 30,
                        Name = "Yell"
                    },
                    new ChannelSetting
                    {
                        ChatType = 11,
                        Name = "Shout"
                    },
                    new ChannelSetting
                    {
                        ChatType = 24,
                        Name = "Free Company"
                    },
                    new ChannelSetting
                    {
                        ChatType = 15,
                        Name = "Alliance"
                    }
                };
            }
            int i = 0;
            while ((long)i < 23L)
            {
                if (Service.configuration.EnabledChannels[i].Enabled)
                {
                    Service.enabledChannels.Add(Service.configuration.EnabledChannels[i].ChatType);
                }
                i++;
            }
            Service.InitializeRegex(false);
            Service.configuration.Save();
        }

        // Token: 0x1700000F RID: 15
        // (get) Token: 0x06000038 RID: 56 RVA: 0x0000310A File Offset: 0x0000130A
        // (set) Token: 0x06000039 RID: 57 RVA: 0x00003111 File Offset: 0x00001311
        [PluginService]
        public static DalamudPluginInterface PluginInterface { get; private set; } = null;

        // Token: 0x17000010 RID: 16
        // (get) Token: 0x0600003A RID: 58 RVA: 0x00003119 File Offset: 0x00001319
        // (set) Token: 0x0600003B RID: 59 RVA: 0x00003120 File Offset: 0x00001320
        [PluginService]
        public static CommandManager CommandManager { get; private set; } = null;

        // Token: 0x17000011 RID: 17
        // (get) Token: 0x0600003C RID: 60 RVA: 0x00003128 File Offset: 0x00001328
        // (set) Token: 0x0600003D RID: 61 RVA: 0x0000312F File Offset: 0x0000132F
        [PluginService]
        public static ClientState ClientState { get; private set; } = null;

        // Token: 0x17000012 RID: 18
        // (get) Token: 0x0600003E RID: 62 RVA: 0x00003137 File Offset: 0x00001337
        // (set) Token: 0x0600003F RID: 63 RVA: 0x0000313E File Offset: 0x0000133E
        [PluginService]
        public static ChatGui ChatGui { get; private set; } = null;

        // Token: 0x17000013 RID: 19
        // (get) Token: 0x06000040 RID: 64 RVA: 0x00003146 File Offset: 0x00001346
        // (set) Token: 0x06000041 RID: 65 RVA: 0x0000314D File Offset: 0x0000134D
        [PluginService]
        public static SigScanner SigScanner { get; private set; } = null;

        // Token: 0x17000014 RID: 20
        // (get) Token: 0x06000042 RID: 66 RVA: 0x00003155 File Offset: 0x00001355
        // (set) Token: 0x06000043 RID: 67 RVA: 0x0000315C File Offset: 0x0000135C
        [PluginService]
        public static ObjectTable ObjectTable { get; private set; } = null;

        // Token: 0x17000015 RID: 21
        // (get) Token: 0x06000044 RID: 68 RVA: 0x00003164 File Offset: 0x00001364
        // (set) Token: 0x06000045 RID: 69 RVA: 0x0000316B File Offset: 0x0000136B
        [PluginService]
        public static TargetManager TargetManager { get; private set; } = null;

        // Token: 0x17000016 RID: 22
        // (get) Token: 0x06000046 RID: 70 RVA: 0x00003173 File Offset: 0x00001373
        // (set) Token: 0x06000047 RID: 71 RVA: 0x0000317A File Offset: 0x0000137A
        [PluginService]
        public static DataManager DataManager { get; private set; } = null;

        // Token: 0x06000048 RID: 72 RVA: 0x00003182 File Offset: 0x00001382
        public Service()
        {
        }

        // Token: 0x06000049 RID: 73 RVA: 0x0000318C File Offset: 0x0000138C
        // Note: this type is marked as 'beforefieldinit'.
        static Service()
        {
        }

        // Token: 0x04000016 RID: 22
        [Nullable(2)]
        public static Plugin plugin;

        // Token: 0x04000017 RID: 23
        [Nullable(2)]
        public static Configuration configuration;

        // Token: 0x04000018 RID: 24
        [Nullable(2)]
        public static XivCommonBase commonBase;

        // Token: 0x04000019 RID: 25
        [Nullable(2)]
        public static Regex Rx;

        // Token: 0x0400001A RID: 26
        [Nullable(2)]
        public static Regex CustomRx;

        // Token: 0x0400001B RID: 27
        public static List<XivChatType> enabledChannels = new List<XivChatType>();

        // Token: 0x0400001C RID: 28
        [Nullable(new byte[]
        {
            2,
            1
        })]
        public static ExcelSheet<Emote> emoteCommands;

        // Token: 0x0400001D RID: 29
        public static HashSet<string> Emotes = new HashSet<string>();

        // Token: 0x0400001E RID: 30
        private const uint CHANNEL_COUNT = 23U;

        // Token: 0x0200000B RID: 11
        [Nullable(0)]
        public struct ParsedTextCommand
        {
            // Token: 0x0600004A RID: 74 RVA: 0x000031DD File Offset: 0x000013DD
            public ParsedTextCommand()
            {
                this.Main = string.Empty;
                this.Args = string.Empty;
            }

            // Token: 0x0600004B RID: 75 RVA: 0x000031F5 File Offset: 0x000013F5
            public override string ToString()
            {
                return (this.Main + " " + this.Args).Trim();
            }

            // Token: 0x04000027 RID: 39
            public string Main;

            // Token: 0x04000028 RID: 40
            public string Args;
        }
    }
}
