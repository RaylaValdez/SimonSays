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
        private const string CommandName = "/simonsays";


        public static DalamudPluginInterface PluginInterfaceStatic { get; private set; }
        public DalamudPluginInterface PluginInterface { get; init; }
        private ICommandManager CommandManager { get; init; }
        public static Configuration Configuration { get; private set; }
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

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

            PluginInterface.Create<Service>();

            Service.ChatGui.ChatMessage += Meat.OnChatMessage;

            Service.CreateEmoteList();
        }

        public void Dispose()
        {
            this.WindowSystem.RemoveAllWindows();

            ConfigWindow.Dispose();

            this.CommandManager.RemoveHandler(CommandName);
            Service.ChatGui.ChatMessage -= Meat.OnChatMessage;
        }

        private void OnCommand(string command, string args)
        {
            ConfigWindow.IsOpen = true;
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
