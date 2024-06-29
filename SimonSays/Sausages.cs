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
using FFXIVClientStructs.FFXIV.Client.System.Framework;

namespace SimonSays
{
    /// <summary>
    /// Declares used Services and Functions relating to it.
    /// </summary>
    internal class Sausages
    {
        /// <summary>
        /// Creates a list of emotes by extracting emote commands and aliases from Emote data obtained from the game.
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
            foreach (var Emote in EmoteList)
            {
                // Extract and add main emote command
                var Value = Emote.TextCommand.Value;
                var Cmd = Value?.Command;
                if (Cmd != null && Cmd != "")
                {
                    Emotes.Add(Cmd);
                }

                // Extract and add short emote command
                var Value2 = Emote.TextCommand.Value;
                Cmd = (Value2?.ShortCommand);
                if (Cmd != null && Cmd != "")
                {
                    Emotes.Add(Cmd);
                }

                // Extract and add emote alias
                var Value3 = Emote.TextCommand.Value;
                Cmd = (Value3?.Alias);
                if (Cmd != null && Cmd != "")
                {
                    Emotes.Add(Cmd);
                }

                // Extract and add short emote alias
                var Value4 = Emote.TextCommand.Value;
                Cmd = (Value4?.ShortAlias);
                if (Cmd != null && Cmd != "")
                {
                    Emotes.Add(Cmd);
                }
            }
        }

        public static ExcelSheet<Emote>? EmoteList;
        public static HashSet<string> Emotes = [];

        /// <summary>
        /// Gets or sets the DtrBar plugin service.
        /// </summary>
        [PluginService] public static IDtrBar DtrBar { get; private set; } = null!;
        /// <summary>
        /// Represents a static property in the PluginService class that provides access to an instance of IPluginLog for logging purposes.
        /// The property is read-only and can be set only within the PluginService class.
        /// The property is initialized to null! indicating that it must be set to a non-null value before use.
        /// </summary>
        [PluginService] public static IPluginLog Log { get; private set; } = null!;
        /// <summary>
        /// Static property representing the IDataManager instance for the PluginService.
        /// </summary>
        [PluginService] public static IDataManager DataManager { get; private set; } = null!;
        /// <summary>
        /// Represents a static property in the PluginService class that provides access to an IChatGui instance for interacting with chat GUI.
        /// The property is read-only and can be set only within the PluginService class.
        /// The property is initialized to null and marked as nullable reference type with the null-forgiving operator (!).
        /// </summary>
        [PluginService] public static IChatGui ChatGui { get; private set; } = null!;
        /// <summary>
        /// Gets or sets the ICommandManager instance for the PluginService.
        /// </summary>
        [PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
        /// <summary>
        /// Gets or sets the client state for the plugin service.
        /// </summary>
        [PluginService] public static IClientState ClientState { get; private set; } = null!;
        /// <summary>
        /// Gets or sets the TargetManager instance for the PluginService.
        /// </summary>
        [PluginService] public static ITargetManager TargetManager { get; private set; } = null!;
        /// <summary>
        /// Gets or sets the IGameInteropProvider instance for the PluginService.
        /// </summary>
        [PluginService] public static IGameInteropProvider Hook { get; private set; } = null!;
        /// <summary>
        /// Represents a static property in the PluginService class that provides access to an IFramework instance.
        /// The property is read-only and can be set only within the PluginService class.
        /// The property is initialized to null! indicating that it must be set before accessing it.
        /// </summary>
        [PluginService] public static IFramework Framework { get; private set; } = null!;
        /// <summary>
        /// Gets or sets the NotificationManager instance for the PluginService.
        /// </summary>
        [PluginService] public static INotificationManager NotificationManager { get; private set; } = null!;
        /// <summary>
        /// Gets or sets the texture provider for the plugin service.
        /// </summary>
        [PluginService] public static ITextureProvider TextureProvider { get; private set; } = null!;
        /// <summary>
        /// Represents a static property in the PluginService class that provides access to an IObjectTable instance.
        /// The property is read-only and can be set only within the PluginService class.
        /// The property is initialized to null! indicating that it must be set to a non-null value before use.
        /// </summary>
        [PluginService] public static IObjectTable ObjectTable { get; private set; } = null!;
        /// <summary>
        /// Represents a static property in the PluginService class that provides access to the PartyList interface.
        /// The property is read-only and can be set only within the PluginService class.
        /// The property is initialized to null and must be set to a non-null value before accessing it.
        /// </summary>
        [PluginService] public static IPartyList PartyList { get; private set; } = null!;
    }


}
