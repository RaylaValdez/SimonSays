using System;
using System.IO;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace SimonSays.Windows;

/// <summary>
/// Represents a window for configuring settings.
/// </summary>
public class ConfigWindow : Window, IDisposable
{
    private const bool EnableDebug = false;
    public const int BufferSize = 1024;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigWindow"/> class.
    /// </summary>
    /// <param name="plugin">The plugin instance.</param>
    public ConfigWindow(Potatoes plugin)
        : base(
            "SimonSays Settings",
            ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.Size = new System.Numerics.Vector2(1285, 883);
        this.SizeCondition = ImGuiCond.FirstUseEver | ImGuiCond.Appearing;

        var imagePath = Path.Combine(Potatoes.PluginInterfaceStatic!.AssemblyLocation.Directory?.FullName!, "ts500.png");
        ConfigWindowHelpers.aboutImage = Sausages.TextureProvider.GetFromFile(imagePath);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public override void PreDraw()
    {
        ConfigWindowHelpers.PushStyles();
        base.PreDraw();
    }

    /// <inheritdoc/>
    public override void Draw()
    {
        ConfigWindowHelpers.OpenReNamingWindow();
        ConfigWindowHelpers.OpenNamingWindow();
        ConfigWindowHelpers.ContextPopup();
        ImGui.Text("Enable or Disable listening to channels.");
        ConfigWindowHelpers.DrawEnabled();
        if (ImGui.BeginTabBar("Tabs"))
        {
            if (ImGui.BeginTabItem("Positional Presets"))
            {
                Tabs.PositionalPresets.Draw();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Settings"))
            {
                Tabs.Settings.Draw();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Usage"))
            {
                Tabs.Usage.Draw();
                ImGui.EndTabItem();
            }

            if (EnableDebug && ImGui.BeginTabItem("Debug"))
            {
                ConfigWindowHelpers.DrawDebugPositionalInformation();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("About"))
            {
                Tabs.About.Draw();
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }

    /// <inheritdoc/>
    public override void PostDraw()
    {
        ImGui.PopStyleVar(22);
        ImGui.PopStyleColor(11);
        base.PostDraw();
    }
}
