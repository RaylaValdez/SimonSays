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
using Dalamud.Plugin.Services;

namespace SimonSays
{
    internal class Service
    {
        public static void CreateEmoteList()
        {
            emoteList = DataManager.GetExcelSheet<Emote>();
            if (emoteList == null)
            {
                ChatGui.Print("SimonSays failed to read the emotes from the game.");
                return;
            }
            foreach (Emote emote in emoteList)
            {
                TextCommand? value = emote.TextCommand.Value;
                SeString? cmd = (value != null) ? value.Command : null;
                if (cmd != null && cmd != "")
                {
                    Emotes.Add(cmd);
                }
                TextCommand? value2 = emote.TextCommand.Value;
                cmd = ((value2 != null) ? value2.ShortCommand : null);
                if (cmd != null && cmd != "")
                {
                    Emotes.Add(cmd);
                }
                TextCommand? value3 = emote.TextCommand.Value;
                cmd = ((value3 != null) ? value3.Alias : null);
                if (cmd != null && cmd != "")
                {
                    Emotes.Add(cmd);
                }
                TextCommand? value4 = emote.TextCommand.Value;
                cmd = ((value4 != null) ? value4.ShortAlias : null);
                if (cmd != null && cmd != "")
                {
                    Emotes.Add(cmd);
                }
            }
        }
        public static ExcelSheet<Emote>? emoteList;
        public static HashSet<string> Emotes = new HashSet<string>();
        public static XivCommonBase? commonBase;

        [PluginService] public static IDataManager? DataManager { get; private set; }
        [PluginService] public static IChatGui? ChatGui { get; private set; }
        [PluginService] public static ICommandManager? CommandManager { get; private set; }
    }


}
