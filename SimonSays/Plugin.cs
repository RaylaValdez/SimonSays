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
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "SimonSays";
        private const string CommandName = "/simonsaysconfig";
        private const string Sync = "/sync";
        private const string DoThis = "/simonsays";


        public static DalamudPluginInterface? PluginInterfaceStatic { get; private set; }
        public DalamudPluginInterface PluginInterface { get; init; }
        private ICommandManager CommandManager { get; init; }
        public static Configuration? Configuration { get; private set; }
        public WindowSystem WindowSystem = new("SimonSays");

        private ConfigWindow ConfigWindow { get; init; }

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] ICommandManager commandManager)
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;

            PluginInterfaceStatic = pluginInterface;

            Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(this.PluginInterface);

            ConfigWindow = new ConfigWindow(this);

            WindowSystem.AddWindow(ConfigWindow);

            this.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Open SimonSays Settings"
            });

            this.CommandManager.AddHandler(Sync, new CommandInfo(OnCommand)
            {
                HelpMessage = "Begin Positional syncing, if enabled in settings."
            });

            this.CommandManager.AddHandler(DoThis, new CommandInfo(OnCommand)
            {
                HelpMessage = "Usage : /simonsays hum | Targetting a player will tell the player to hum with you in sync!"
            });

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

            PluginInterface.Create<Service>();

            Service.ChatGui.ChatMessage += Meat.OnChatMessage;

            Service.CreateEmoteList();

            Meat.Setup();
        }

        public void Dispose()
        {
            this.WindowSystem.RemoveAllWindows();

            ConfigWindow.Dispose();
            Meat.Dispose();

            this.CommandManager.RemoveHandler(CommandName);
            this.CommandManager.RemoveHandler(Sync);
            Service.ChatGui.ChatMessage -= Meat.OnChatMessage;
        }

        private void OnCommand(string command, string args)
        {
            if (command == CommandName)
            {
                ConfigWindow.IsOpen = true;
            }

            if (command == Sync)
            {
                if (!Configuration.PosSync)
                {
                    return;
                }
                Meat.StartScooch();
            }

            string[] argSplit = args.Split(' ');

            if (command == DoThis)
            {
                if (argSplit.Length >= 1)
                {
                    bool syncPos = false;
                    if (argSplit.Length >= 2)
                    {
                        if (argSplit[1].ToLower() == "true")
                        {
                            syncPos = true;
                        }
                        if (!Configuration.PosSync)
                        {
                            syncPos = false;
                            Service.ChatGui.Print("Enable Positional Syncing in settings for command based syncing.");
                        }
                    }
                    Meat.SimonSays(argSplit[0], syncPos);
                }
                else
                {
                    // No arguments given
                    Service.ChatGui.Print("No arguments given to simonsays");
                }
            }
        }

        private void DrawUI()
        {
            this.WindowSystem.Draw();
        }

        public void DrawConfigUI()
        {
            ConfigWindow.IsOpen = true;
        }
    }
}
