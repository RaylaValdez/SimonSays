using System.Collections.Generic;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace SimonSays;

/// <summary>
/// Declares used Services and Functions relating to it.
/// </summary>
internal class Sausages
{
    /// <summary>
    /// Creates a list of emotes by extracting emote commands and aliases from the game's Emote sheet.
    /// </summary>
    public static void CreateEmoteList()
    {
        EmoteList = DataManager.GetExcelSheet<Emote>();

        if (EmoteList == null)
        {
            ChatGui.PrintError("SimonSays failed to read the Emotes from the game.");
            return;
        }

        foreach (var emote in EmoteList)
        {
            if (!emote.TextCommand.IsValid)
            {
                continue;
            }

            var textCommand = emote.TextCommand.Value;

            AddEmoteCommand(textCommand.Command.ToString());
            AddEmoteCommand(textCommand.ShortCommand.ToString());
            AddEmoteCommand(textCommand.Alias.ToString());
            AddEmoteCommand(textCommand.ShortAlias.ToString());
        }
    }

    private static void AddEmoteCommand(string command)
    {
        if (!string.IsNullOrEmpty(command))
        {
            Emotes.Add(command);
        }
    }

    public static ExcelSheet<Emote>? EmoteList;
    public static HashSet<string> Emotes = [];

    /// <summary>
    /// Gets the DTR bar service.
    /// </summary>
    [PluginService] public static IDtrBar DtrBar { get; private set; } = null!;

    /// <summary>
    /// Gets the plugin log service.
    /// </summary>
    [PluginService] public static IPluginLog Log { get; private set; } = null!;

    /// <summary>
    /// Gets the data manager service.
    /// </summary>
    [PluginService] public static IDataManager DataManager { get; private set; } = null!;

    /// <summary>
    /// Gets the chat GUI service.
    /// </summary>
    [PluginService] public static IChatGui ChatGui { get; private set; } = null!;

    /// <summary>
    /// Gets the command manager service.
    /// </summary>
    [PluginService] public static ICommandManager CommandManager { get; private set; } = null!;

    /// <summary>
    /// Gets the target manager service.
    /// </summary>
    [PluginService] public static ITargetManager TargetManager { get; private set; } = null!;

    /// <summary>
    /// Gets the game interop provider service.
    /// </summary>
    [PluginService] public static IGameInteropProvider Hook { get; private set; } = null!;

    /// <summary>
    /// Gets the framework service.
    /// </summary>
    [PluginService] public static IFramework Framework { get; private set; } = null!;

    /// <summary>
    /// Gets the notification manager service.
    /// </summary>
    [PluginService] public static INotificationManager NotificationManager { get; private set; } = null!;

    /// <summary>
    /// Gets the texture provider service.
    /// </summary>
    [PluginService] public static ITextureProvider TextureProvider { get; private set; } = null!;

    /// <summary>
    /// Gets the object table service.
    /// </summary>
    [PluginService] public static IObjectTable ObjectTable { get; private set; } = null!;

    /// <summary>
    /// Gets the party list service.
    /// </summary>
    [PluginService] public static IPartyList PartyList { get; private set; } = null!;
}
