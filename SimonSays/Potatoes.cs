using System;
using System.IO;
using Dalamud.Game.Command;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using SimonSays.Helpers;
using SimonSays.Windows;

namespace SimonSays;

/// <summary>
/// Main plugin class for SimonSays, handling configuration, commands, and UI.
/// </summary>
public sealed class Potatoes : IDalamudPlugin
{
    private const string Config = "/simonsaysconfig";
    private const string EzConfig = "/sscfg";
    private const string Sync = "/sync";
    private const string StopSync = "/stopsync";
    private const string DoThis = "/simonsays";

    public const string DiscordURL = "https://dsc.gg/triquetrastudios";
    public const string BuyMeACoffee = "https://www.buymeacoffee.com/raylaa";
    public const string Repo = "https://raw.githubusercontent.com/RaylaValdez/dalamudrepo/main/pluginmaster.json";
    public const string Source = "https://github.com/RaylaValdez/SimonSays";

    /// <summary>
    /// Gets the shared plugin interface instance.
    /// </summary>
    public static IDalamudPluginInterface? PluginInterfaceStatic { get; private set; }

    /// <summary>
    /// Gets the shared configuration instance.
    /// </summary>
    public static Cauliflower? Configuration { get; private set; }

    /// <summary>
    /// Gets the plugin name.
    /// </summary>
    public static string Name => "SimonSays";

    private IDalamudPluginInterface PluginInterface { get; init; }

    private ICommandManager CommandManager { get; init; }

    private readonly WindowSystem windowSystem = new("SimonSays");

    private ConfigWindow ConfigWindow { get; init; }

    public static string ConfigDirectory { get; set; } = string.Empty;
    public static string ConfigFile { get; set; } = string.Empty;
    public static string PresetDirectory { get; set; } = string.Empty;

    private readonly IDtrBarEntry dtrEntry;

    /// <summary>
    /// Initializes a new instance of the <see cref="Potatoes"/> class.
    /// </summary>
    /// <param name="pluginInterface">The plugin interface.</param>
    /// <param name="commandManager">The command manager service.</param>
    public Potatoes(
        IDalamudPluginInterface pluginInterface,
        ICommandManager commandManager)
    {
        this.PluginInterface = pluginInterface;
        this.CommandManager = commandManager;
        PluginInterfaceStatic = this.PluginInterface;

        Configuration = this.PluginInterface.GetPluginConfig() as Cauliflower ?? new Cauliflower();
        Configuration.Initialize(this.PluginInterface);

        this.PluginInterface.Create<Sausages>();

        this.ConfigWindow = new ConfigWindow(this);
        this.windowSystem.AddWindow(this.ConfigWindow);

        ConfigDirectory = this.PluginInterface.ConfigDirectory.FullName;
        ConfigFile = this.PluginInterface.ConfigFile.FullName;
        PresetDirectory = Path.Combine(ConfigDirectory, "presets");
        if (!Directory.Exists(PresetDirectory))
        {
            Directory.CreateDirectory(PresetDirectory);
        }

        this.CommandManager.AddHandler(Config, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open SimonSays Settings",
        });
        this.CommandManager.AddHandler(EzConfig, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open SimonSays Settings",
        });
        this.CommandManager.AddHandler(Sync, new CommandInfo(OnCommand)
        {
            HelpMessage = "Begin Positional syncing, if enabled in settings.",
        });
        this.CommandManager.AddHandler(StopSync, new CommandInfo(OnCommand)
        {
            HelpMessage = "Stops Positional syncing.",
        });
        this.CommandManager.AddHandler(DoThis, new CommandInfo(OnCommand)
        {
            HelpMessage = "Usage: /simonsays hum | Targetting a player will tell the player to hum with you in sync!",
        });

        this.PluginInterface.UiBuilder.Draw += this.DrawUI;
        this.PluginInterface.UiBuilder.OpenConfigUi += this.DrawConfigUI;
        this.PluginInterface.UiBuilder.OpenMainUi += this.DrawConfigUI;

        Sausages.Framework.Update += this.FrameworkUpdate;

        this.dtrEntry = Sausages.DtrBar.Get(Name);

        Sausages.ChatGui.ChatMessage += Meat.OnChatMessage;
        Sausages.CreateEmoteList();

        Meat.Setup();

        Sausages.Log.Information($"Configuration EmoteOffsets count: {Configuration.EmoteOffsets.Count}");
    }

    /// <summary>
    /// Disposes of resources associated with the plugin.
    /// </summary>
    public void Dispose()
    {
        this.windowSystem.RemoveAllWindows();

        this.dtrEntry.Remove();

        Sausages.Framework.Update -= this.FrameworkUpdate;

        this.ConfigWindow.Dispose();
        Meat.Dispose();

        this.CommandManager.RemoveHandler(Config);
        this.CommandManager.RemoveHandler(EzConfig);
        this.CommandManager.RemoveHandler(Sync);
        this.CommandManager.RemoveHandler(StopSync);
        this.CommandManager.RemoveHandler(DoThis);
        Sausages.ChatGui.ChatMessage -= Meat.OnChatMessage;
    }

    /// <summary>
    /// Updates the DTR bar entry on every framework tick.
    /// </summary>
    /// <param name="framework">The framework instance.</param>
    private void FrameworkUpdate(IFramework framework)
    {
        if (this.dtrEntry.Shown)
        {
            this.dtrEntry.Text = new SeString(new TextPayload($"{Name}: {(Configuration!.IsListening ? "On" : "Off")}"));
            this.dtrEntry.OnClick = (e) =>
            {
                if (e.ClickType == MouseClickType.Left)
                {
                    Configuration.IsListening ^= true;
                }
            };
        }
    }

    /// <summary>
    /// Handles the /simonsays command arguments and executes the requested emotes.
    /// </summary>
    /// <param name="argSplit">An array containing the arguments for the command.</param>
    private static void DoThisCommandHandlingMeat(string[] argSplit)
    {
        var syncPos = false;

        // /simonsays emote
        var emote = argSplit[0];
        var otherEmote = emote;

        if (argSplit.Length == 2)
        {
            // /simonsays emote true
            if (argSplit[1].Equals("true", StringComparison.CurrentCultureIgnoreCase) || argSplit[1] == "1")
            {
                syncPos = true;
            }
            else
            {
                // /simonsays otheremote emote
                emote = argSplit[1];
                otherEmote = argSplit[0];
            }
        }
        else if (argSplit.Length >= 3)
        {
            // /simonsays otheremote emote true
            emote = argSplit[1];
            otherEmote = argSplit[0];

            if (argSplit[2].Equals("true", StringComparison.CurrentCultureIgnoreCase) || argSplit[2] == "1")
            {
                syncPos = true;
            }
        }

        if (!Configuration!.PosSync)
        {
            syncPos = false;
            Sausages.ChatGui.Print("Enable Positional Syncing in settings for Command-based syncing.");
        }

        Meat.SimonSays(emote, otherEmote, syncPos);
    }

    /// <summary>
    /// Handles incoming commands and executes corresponding actions.
    /// </summary>
    /// <param name="command">The command to be executed.</param>
    /// <param name="args">The arguments associated with the command.</param>
    private void OnCommand(string command, string args)
    {
        if (command == Config || command == EzConfig)
        {
            this.ConfigWindow.IsOpen = true;
        }

        var argSplit = args.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (command == Sync)
        {
            if (!Configuration!.PosSync)
            {
                return;
            }

            if (argSplit.Length >= 1)
            {
                // /sync emote
                var emote = argSplit[0];
                var offset = Meat.EmoteHasOffset(emote.ToLowerInvariant());
                Meat.StartScooch(offset);
            }
            else
            {
                Meat.StartScooch();
            }
        }

        if (command == StopSync)
        {
            Meat.StopScooch();
        }

        if (command == DoThis)
        {
            if (argSplit.Length >= 1)
            {
                DoThisCommandHandlingMeat(argSplit);
            }
            else
            {
                Sausages.ChatGui.Print("No arguments given to SimonSays.");
            }
        }
    }

    /// <summary>
    /// Draws the user interface using the WindowSystem.
    /// </summary>
    private void DrawUI()
    {
        this.windowSystem.Draw();
    }

    /// <summary>
    /// Opens the configuration window.
    /// </summary>
    public void DrawConfigUI()
    {
        this.ConfigWindow.IsOpen = true;
    }
}
