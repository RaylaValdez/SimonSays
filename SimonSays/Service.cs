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
            EmoteList = DataManager.GetExcelSheet<Emote>();
            if (EmoteList == null)
            {
                ChatGui.Print("SimonSays failed to read the Emotes from the game.");
                return;
            }
            foreach (Emote Emote in EmoteList)
            {
                TextCommand? Value = Emote.TextCommand.Value;
                SeString? Cmd = (Value != null) ? Value.Command : null;
                if (Cmd != null && Cmd != "")
                {
                    Emotes.Add(Cmd);
                }
                TextCommand? Value2 = Emote.TextCommand.Value;
                Cmd = ((Value2 != null) ? Value2.ShortCommand : null);
                if (Cmd != null && Cmd != "")
                {
                    Emotes.Add(Cmd);
                }
                TextCommand? Value3 = Emote.TextCommand.Value;
                Cmd = ((Value3 != null) ? Value3.Alias : null);
                if (Cmd != null && Cmd != "")
                {
                    Emotes.Add(Cmd);
                }
                TextCommand? Value4 = Emote.TextCommand.Value;
                Cmd = ((Value4 != null) ? Value4.ShortAlias : null);
                if (Cmd != null && Cmd != "")
                {
                    Emotes.Add(Cmd);
                }
            }
        }
        public static ExcelSheet<Emote>? EmoteList;
        public static HashSet<string> Emotes = new HashSet<string>();
        public static XivCommonBase? CommonBase;
        [PluginService] public static IPluginLog Log { get; private set; } = null!;
        [PluginService] public static IDataManager DataManager { get; private set; }
        [PluginService] public static IChatGui ChatGui { get; private set; }
        [PluginService] public static ICommandManager CommandManager { get; private set; }
        [PluginService] public static IClientState ClientState { get; private set; }
        [PluginService] public static ITargetManager TargetManager { get; private set; }
        [PluginService] public static IGameInteropProvider Hook { get; private set; }
    }


}
