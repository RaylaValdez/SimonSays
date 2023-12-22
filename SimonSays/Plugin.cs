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
        private const string Config = "/simonsaysconfig";
        private const string Sync = "/sync";
        private const string DoThis = "/simonsays";


        public static DalamudPluginInterface? PluginInterfaceStatic { get; private set; }
        public DalamudPluginInterface PluginInterface { get; init; }
        private ICommandManager CommandManager { get; init; }
        public static Configuration? Configuration { get; private set; }
        public WindowSystem WindowSystem = new("SimonSays");

        private ConfigWindow ConfigWindow { get; init; }

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface PluginInterface,
            [RequiredVersion("1.0")] ICommandManager CommandManager)
        {
            this.PluginInterface = PluginInterface;
            this.CommandManager = CommandManager;

            PluginInterfaceStatic = PluginInterface;

            Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(this.PluginInterface);

            ConfigWindow = new ConfigWindow(this);

            WindowSystem.AddWindow(ConfigWindow);

            this.CommandManager.AddHandler(Config, new CommandInfo(OnCommand)
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

            this.CommandManager.RemoveHandler(Config);
            this.CommandManager.RemoveHandler(Sync);
            Service.ChatGui.ChatMessage -= Meat.OnChatMessage;
        }

        private void OnCommand(string Command, string Args)
        {
            if (Command == Config)
            {
                ConfigWindow.IsOpen = true;
            }

            if (Command == Sync)
            {
                if (!Configuration.PosSync)
                {
                    return;
                }
                Meat.StartScooch();
            }

            string[] ArgSplit = Args.Split(' ');

            if (Command == DoThis)
            {
                if (ArgSplit.Length >= 1)
                {
                    bool SyncPos = false;
                    if (ArgSplit.Length >= 2)
                    {
                        if (ArgSplit[1].ToLower() == "true")
                        {
                            SyncPos = true;
                        }
                        if (!Configuration.PosSync)
                        {
                            SyncPos = false;
                            Service.ChatGui.Print("Enable Positional Syncing in settings for Command based syncing.");
                        }
                    }
                    Meat.SimonSays(ArgSplit[0], SyncPos);
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
