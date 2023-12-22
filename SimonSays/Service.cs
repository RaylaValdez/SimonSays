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
        /// <summary>
        /// Populates the list of emotes by extracting them from the game's Emote data.
        /// </summary>
        public static void CreateEmoteList()
        {
            // Get Emote data from the game
            EmoteList = DataManager.GetExcelSheet<Emote>();

            // Check if Emote data is successfully obtained
            if (EmoteList == null)
            {
                ChatGui.Print("SimonSays failed to read the Emotes from the game.");
                return;
            }

            // Iterate through Emote data to extract emote commands and aliases
            foreach (Emote Emote in EmoteList)
            {
                // Extract and add main emote command
                TextCommand? Value = Emote.TextCommand.Value;
                SeString? Cmd = (Value != null) ? Value.Command : null;
                if (Cmd != null && Cmd != "")
                {
                    Emotes.Add(Cmd);
                }

                // Extract and add short emote command
                TextCommand? Value2 = Emote.TextCommand.Value;
                Cmd = ((Value2 != null) ? Value2.ShortCommand : null);
                if (Cmd != null && Cmd != "")
                {
                    Emotes.Add(Cmd);
                }

                // Extract and add emote alias
                TextCommand? Value3 = Emote.TextCommand.Value;
                Cmd = ((Value3 != null) ? Value3.Alias : null);
                if (Cmd != null && Cmd != "")
                {
                    Emotes.Add(Cmd);
                }

                // Extract and add short emote alias
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
        [PluginService] public static IDataManager DataManager { get; private set; } = null!;
        [PluginService] public static IChatGui ChatGui { get; private set; } = null!;
        [PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
        [PluginService] public static IClientState ClientState { get; private set; } = null!;
        [PluginService] public static ITargetManager TargetManager { get; private set; } = null!;
        [PluginService] public static IGameInteropProvider Hook { get; private set; } = null!;
    }


}
