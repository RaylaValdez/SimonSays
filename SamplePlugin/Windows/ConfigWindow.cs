using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace SimonSays.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;

    public ConfigWindow(Plugin plugin) : base(
        "SimonSays Settings",
        ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.Size = new Vector2(232, 75);
        this.SizeCondition = ImGuiCond.Always;

        this.Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    private static void DrawCheckbox(int index)
    {
        if (index % 4 != 0)
        {
            ImGui.SameLine();
        }
        bool enabled = Service.Configuration.EnabledChannels[index].Enabled;
        if (ImGui.Checkbox(this.configuration.EnabledChannels[index].Name, ref enabled))
        {
            this.configuration.EnabledChannels[index].Enabled = enabled;
            this.configuration.Save();
            if (this.configuration.EnabledChannels[index].Enabled)
            {
                this.enabledChannels.Add(this.configuration.EnabledChannels[index].ChatType);
                return;
            }
            this.enabledChannels.Remove(this.configuration.EnabledChannels[index].ChatType);
        }
    }

    public override void Draw()
    {
        // can't ref a property, so use a local copy
        var configValue = this.Configuration.SomePropertyToBeSavedAndWithADefault;
        if (ImGui.Checkbox("Random Config Bool", ref configValue))
        {
            this.Configuration.SomePropertyToBeSavedAndWithADefault = configValue;
            // can save immediately on change, if you don't want to provide a "Save and Close" button
            this.Configuration.Save();
        }
    }
}
