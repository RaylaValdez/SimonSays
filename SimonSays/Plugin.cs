using System.IO;
using System.Reflection;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Game.Gui;
using Dalamud.Plugin.Services;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using SimonSays.Windows;
using XivCommon;

namespace SimonSays
{
    /// <summary>
    /// Main plugin class for SimonSays, handling configuration, commands, and UI.
    /// </summary>
    public sealed class Plugin : IDalamudPlugin
    {
        // Command constants
        private const string Config = "/simonsaysconfig";
        private const string EzConfig = "/sscfg";
        private const string Sync = "/sync";
        private const string StopSync = "/stopsync";
        private const string DoThis = "/simonsays";

        public const string DiscordURL = "https://dsc.gg/triquetrastudios";
        public const string BuyMeACoffee = "https://www.buymeacoffee.com/raylaa";
        public const string Repo = "https://raw.githubusercontent.com/RaylaValdez/MyDalamudPlugins/main/pluginmaster.json";
        public const string Source = "https://github.com/RaylaValdez/SimonSays";

        // Static references for plugin-wide use
        public static IDalamudPluginInterface? PluginInterfaceStatic { get; private set; }
        public static Configuration? Configuration { get; private set; }

        // Plugin components
        public string Name => "SimonSays";
        private IDalamudPluginInterface PluginInterface { get; init; }
        private ICommandManager CommandManager { get; init; }
        private WindowSystem windowSystem = new("SimonSays");
        private ConfigWindow ConfigWindow { get; init; }
        public static string ConfigDirectory { get; set; } = string.Empty;
        public static string ConfigFile { get; set; } = string.Empty;
        public static string PresetDirectory { get; set; } = string.Empty;
        private readonly IDtrBarEntry dtrEntry;

        // Constructor
        public Plugin(
            IDalamudPluginInterface pluginInterface,
            ICommandManager commandManager)
        {
            // Initialize plugin components
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;
            PluginInterfaceStatic = PluginInterface;

            // Initialize configuration
            Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(this.PluginInterface);

            PluginInterface.Create<Service>();
            // Initialize and add configuration window
            ConfigWindow = new ConfigWindow(this);
            windowSystem.AddWindow(ConfigWindow);

            // Create Directory

            ConfigDirectory = PluginInterface.ConfigDirectory.FullName;
            ConfigFile = PluginInterface.ConfigFile.FullName;
            PresetDirectory = Path.Combine(ConfigDirectory, "presets");
            if (!Directory.Exists(PresetDirectory))
            {
                Directory.CreateDirectory(PresetDirectory);
            }

            // Add command handlers
            this.CommandManager.AddHandler(Config, new CommandInfo(OnCommand)
            {
                HelpMessage = "Open SimonSays Settings"
            });
            this.CommandManager.AddHandler(EzConfig, new CommandInfo(OnCommand)
            {
                HelpMessage = "Open SimonSays Settings"
            });
            this.CommandManager.AddHandler(Sync, new CommandInfo(OnCommand)
            {
                HelpMessage = "Begin Positional syncing, if enabled in settings."
            });
            this.CommandManager.AddHandler(StopSync, new CommandInfo(OnCommand)
            {
                HelpMessage = "Stops Positional syncing."
            });
            this.CommandManager.AddHandler(DoThis, new CommandInfo(OnCommand)
            {
                HelpMessage = "Usage: /simonsays hum | Targetting a player will tell the player to hum with you in sync!"
            });

            // Set up UI events and create necessary services
            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
            this.PluginInterface.UiBuilder.OpenMainUi += DrawConfigUI;
            

            // Set up Framework Update
            Service.Framework.Update += FrameworkUpdate;

            // Set up DTRBar 
            dtrEntry = Service.DtrBar.Get("SimonSays");

            // Set up chat message event handler and create emote list
            Service.ChatGui.ChatMessage += Meat.OnChatMessage;
            Service.CreateEmoteList();

            // Initialize character movement
            Meat.Setup();

            // This line logs the count of EmoteOffsets in the Configuration object as an information message.
            Service.Log.Information($"Configuration EmoteOffsets count: {Configuration.EmoteOffsets.Count}");
        }



        /// <summary>
        /// Disposes of resources associated with the plugin.
        /// </summary>
        public void Dispose()
        {
            // Remove all windows from the WindowSystem
            this.windowSystem.RemoveAllWindows();

            // Remove DTR Entry
            dtrEntry.Remove();

            // Remove Framework
            Service.Framework.Update -= FrameworkUpdate;

            // Dispose of the configuration window and character movement resources
            ConfigWindow.Dispose();
            Meat.Dispose();

            // Remove command handlers and chat message event handler
            this.CommandManager.RemoveHandler(Config);
            this.CommandManager.RemoveHandler(Sync);
            this.CommandManager.RemoveHandler(StopSync);
            this.CommandManager.RemoveHandler(DoThis);
            Service.ChatGui.ChatMessage -= Meat.OnChatMessage;
        }

        /// <summary>
        /// Updates the framework with the specified object.
        /// </summary>
        /// <param name="framework">The object representing the framework.</param>
        private void FrameworkUpdate(object framework)
        {
            // Check if DtrEntry is shown
            if (dtrEntry.Shown)
            {
                // Set the text of DtrEntry to display the name and the current listening status
                dtrEntry.Text = new SeString(new TextPayload($"{Name}: {(Configuration!.IsListening ? "On" : "Off")}"));

                // Set the OnClick event of DtrEntry to toggle the listening status when clicked
                dtrEntry.OnClick = () => Configuration.IsListening ^= true;
            }
        }


        /// <summary>
        /// Handles the execution of a command related to meat processing.
        /// </summary>
        /// <param name="ArgSplit">An array containing the arguments for the command.</param>
        private void DoThisCommandHandlingMeat(string[] ArgSplit)
        {
            // Default to false, only set to true if explicitly specified in arguments
            var SyncPos = false;

            // /simonsays emote
            // Applying emote to other and yourself
            var emote = ArgSplit[0];
            var otherEmote = emote;

            // Check if positional syncing is explicitly specified in arguments OR other emote specified
            if (ArgSplit.Length == 2)
            {
                // /simonsays emote true
                // Emote is still
                if (ArgSplit[1].ToLower() == "true" || ArgSplit[1] == "1")
                {
                    SyncPos = true;
                }
                // /simonsays otheremote emote
                // emote becomes second argument
                else
                {
                    emote = ArgSplit[1];
                    otherEmote = ArgSplit[0];
                }
            }
            // /simonsays otheremote emote true
            // Emote becomes second argument with position syncing as third
            else if (ArgSplit.Length >= 3)
            {
                emote = ArgSplit[1];
                otherEmote = ArgSplit[0];

                if (ArgSplit[2].ToLower() == "true" || ArgSplit[2] == "1")
                {
                    SyncPos = true;
                }
            }

            // Check if positional syncing is disabled and print a message
            if (!Configuration!.PosSync)
            {
                SyncPos = false;
                Service.ChatGui.Print("Enable Positional Syncing in settings for Command-based syncing.");
            }

            // Initiate the SimonSays command with the specified arguments
            Meat.SimonSays(emote, otherEmote, SyncPos);
        }

        /// <summary>
        /// Handles incoming commands and executes corresponding actions.
        /// </summary>
        /// <param name="Command">The command to be executed.</param>
        /// <param name="Args">The arguments associated with the command.</param>
        private void OnCommand(string Command, string Args)
        {
            // Open configuration window command
            if (Command == Config || Command == EzConfig)
            {
                ConfigWindow.IsOpen = true;
            }


            // Split arguments for further processing
            string[] ArgSplit = Args.Split(' ');

            // Synchronize positions command
            if (Command == Sync)
            {
                // Check if positional syncing is enabled
                if (!Configuration!.PosSync)
                {
                    return;
                }

                string Emote = "";

                // /sync emote
                if (ArgSplit.Length >= 1)
                {
                    Emote = ArgSplit[0];

                    // Get offset from emote passed as first argument
                    var offset = Meat.EmoteHasOffset(Emote.ToLower());

                    Meat.StartScooch(offset);
                }
                // just /sync
                else
                {
                    // Start character movement for positional syncing
                    Meat.StartScooch();
                }
            }

            if (Command == StopSync)
            {
                Meat.StopScooch();
            }

            // SimonSays command
            if (Command == DoThis)
            {
                if (ArgSplit.Length >= 1)
                {
                    DoThisCommandHandlingMeat(ArgSplit);
                }
                else
                {
                    // No arguments given for SimonSays command
                    Service.ChatGui.Print("No arguments given to SimonSays.");
                }
            }
        }


        /// <summary>
        /// Draws the user interface using the WindowSystem.
        /// </summary>
        private void DrawUI()
        {
            // Use the WindowSystem to draw the user interface
            this.windowSystem.Draw();
        }


        /// <summary>
        /// Opens the configuration window to draw the user interface.
        /// </summary>
        public void DrawConfigUI()
        {
            // Set the configuration window to be open
            ConfigWindow.IsOpen = true;
        }

    }
}
