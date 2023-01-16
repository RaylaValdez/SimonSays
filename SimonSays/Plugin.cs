using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Reflection;
using Dalamud.Interface.Windowing;
using SimonSays.Windows;
using Dalamud.Game.Gui;
using XivCommon;

namespace SimonSays
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "SimonSays";
        private const string CommandName = "/simonsays";

        private DalamudPluginInterface PluginInterface { get; init; }
        private CommandManager CommandManager { get; init; }
        public static Configuration Configuration { get; private set; }
        public WindowSystem WindowSystem = new("SimonSays");

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager)
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;

            Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(this.PluginInterface);

            WindowSystem.AddWindow(new ConfigWindow(this));

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
            this.CommandManager.RemoveHandler(CommandName);
            Service.ChatGui.ChatMessage -= Meat.OnChatMessage;
        }

        private void OnCommand(string command, string args)
        {
            WindowSystem.GetWindow("SimonSays Settings").IsOpen = true;
        }

        private void DrawUI()
        {
            this.WindowSystem.Draw();
        }

        public void DrawConfigUI()
        {
            WindowSystem.GetWindow("SimonSays Settings").IsOpen = true;
        }
    }
}
