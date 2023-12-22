using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Reflection;
using Dalamud.Interface.Windowing;
using SimonSays.Windows;
using Dalamud.Game.Gui;
using XivCommon;
using Dalamud.Plugin.Services;

namespace SimonSays
{
    /// <summary>
    /// Main plugin class for SimonSays, handling configuration, commands, and UI.
    /// </summary>
    public sealed class Plugin : IDalamudPlugin
    {
        // Command constants
        private const string Config = "/simonsaysconfig";
        private const string Sync = "/sync";
        private const string StopSync = "/stopsync";
        private const string DoThis = "/simonsays";

        // Static references for plugin-wide use
        public static DalamudPluginInterface? PluginInterfaceStatic { get; private set; }
        public static Configuration? Configuration { get; private set; }

        // Plugin components
        public string Name => "SimonSays";
        private DalamudPluginInterface PluginInterface { get; init; }
        private ICommandManager CommandManager { get; init; }
        private WindowSystem WindowSystem = new("SimonSays");
        private ConfigWindow ConfigWindow { get; init; }

        // Constructor
        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface PluginInterface,
            [RequiredVersion("1.0")] ICommandManager CommandManager)
        {
            // Initialize plugin components
            this.PluginInterface = PluginInterface;
            this.CommandManager = CommandManager;
            PluginInterfaceStatic = PluginInterface;

            // Initialize configuration
            Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(this.PluginInterface);

            // Initialize and add configuration window
            ConfigWindow = new ConfigWindow(this);
            WindowSystem.AddWindow(ConfigWindow);

            // Add command handlers
            this.CommandManager.AddHandler(Config, new CommandInfo(OnCommand)
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
            PluginInterface.Create<Service>();

            // Set up chat message event handler and create emote list
            Service.ChatGui.ChatMessage += Meat.OnChatMessage;
            Service.CreateEmoteList();

            // Initialize character movement
            Meat.Setup();
        }



        /// <summary>
        /// Disposes of resources associated with the plugin.
        /// </summary>
        public void Dispose()
        {
            // Remove all windows from the WindowSystem
            this.WindowSystem.RemoveAllWindows();

            // Dispose of the configuration window and character movement resources
            ConfigWindow.Dispose();
            Meat.Dispose();

            // Remove command handlers and chat message event handler
            this.CommandManager.RemoveHandler(Config);
            this.CommandManager.RemoveHandler(Sync);
            Service.ChatGui.ChatMessage -= Meat.OnChatMessage;
        }


        /// <summary>
        /// Handles incoming commands and executes corresponding actions.
        /// </summary>
        /// <param name="Command">The command to be executed.</param>
        /// <param name="Args">The arguments associated with the command.</param>
        private void OnCommand(string Command, string Args)
        {
            // Open configuration window command
            if (Command == Config)
            {
                ConfigWindow.IsOpen = true;
            }

            // Synchronize positions command
            if (Command == Sync)
            {
                // Check if positional syncing is enabled
                if (!Configuration.PosSync)
                {
                    return;
                }

                // Start character movement for positional syncing
                Meat.StartScooch();
            }

            if (Command == StopSync)
            {
                Meat.StopScooch();
            }

            // Split arguments for further processing
            string[] ArgSplit = Args.Split(' ');

            // SimonSays command
            if (Command == DoThis)
            {
                if (ArgSplit.Length >= 1)
                {
                    // Default to false, only set to true if explicitly specified in arguments
                    bool SyncPos = false;

                    // Check if positional syncing is explicitly specified in arguments
                    if (ArgSplit.Length >= 2)
                    {
                        if (ArgSplit[1].ToLower() == "true")
                        {
                            SyncPos = true;
                        }

                        // Check if positional syncing is disabled and print a message
                        if (!Configuration.PosSync)
                        {
                            SyncPos = false;
                            Service.ChatGui.Print("Enable Positional Syncing in settings for Command-based syncing.");
                        }
                    }

                    // Initiate the SimonSays command with the specified arguments
                    Meat.SimonSays(ArgSplit[0], SyncPos);
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
            this.WindowSystem.Draw();
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
