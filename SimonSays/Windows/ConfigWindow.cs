using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Dalamud.Game.Text;
using PunishLib.ImGuiMethods;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Textures;
using Vector2 = System.Numerics.Vector2;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using Vector4 = System.Numerics.Vector4;
using SimonSays.Helpers;
using Dalamud.Interface.ImGuiNotification;
using ImPlotNET;
using SimonSays.Windows.Tabs;


namespace SimonSays.Windows;

/// <summary>
/// Represents a window for configuring settings.
/// </summary>
/// <remarks>
/// This class inherits from the Window class and implements the IDisposable interface.
/// </remarks>
public class ConfigWindow : Window, IDisposable
{
    private static readonly bool EnableDebug = false;
    public const int BufferSize = 1024;

    public ConfigWindow(Potatoes plugin) : base(
        "SimonSays Settings",
        ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.Size = new System.Numerics.Vector2(1285, 883);
        this.SizeCondition = ImGuiCond.FirstUseEver | ImGuiCond.Appearing;

        var imagePath = Path.Combine(Potatoes.PluginInterfaceStatic!.AssemblyLocation.Directory?.FullName!, "ts500.png");
        ConfigWindowHelpers.aboutImage = Sausages.TextureProvider.GetFromFile(imagePath);

    }


    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public string SearchedEmoteFilter = string.Empty;

    public override void PreDraw()
    {
        //PUSH
        //ConfigWindowHelpers.oldTitleColorActive = ConfigWindowHelpers.StylePtr.Colors[(int)ImGuiCol.TitleBgActive];
        //ConfigWindowHelpers.StylePtr.Colors[(int)ImGuiCol.TitleBgActive] = (new Vector4(081, 054, 148, 211) / 255f);
        ConfigWindowHelpers.PushStyles();
        base.PreDraw();
    }

    /// <summary>
    /// Overrides the Draw method from the base class to define custom drawing behavior.
    /// </summary>
    public override void Draw()
    {
        //
        ConfigWindowHelpers.OpenReNamingWindow();
        ConfigWindowHelpers.OpenNamingWindow();
        ConfigWindowHelpers.ContextPopup();
        ImGui.Text("Enable or Disable listening to channels.");
        ConfigWindowHelpers.DrawEnabled();
        if (ImGui.BeginTabBar("Tabs"))
        {
            if (ImGui.BeginTabItem("Positional Presets"))
            {
                PositionalPresets.Draw();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Settings"))
            {
                Settings.Draw();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Usage"))
            {
                Usage.Draw();
                ImGui.EndTabItem();
            }

            if (EnableDebug && ImGui.BeginTabItem("Debug"))
            {
                ConfigWindowHelpers.DrawDebugPositionalInformation();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("About"))
            {
                About.Draw();
                ImGui.EndTabItem();
            }
            ImGui.EndTabBar();
        }


    }

    public override void PostDraw()
    {
        // POP
        ImGui.PopStyleVar(22);
        ImGui.PopStyleColor(11);
        //var StylePtr = ImGui.GetStyle();
        //
        //StylePtr.Colors[(int)ImGuiCol.TitleBgActive] = ConfigWindowHelpers.oldTitleColorActive;
        //StylePtr.Colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(6, 6, 6, 217) / 255f;


        base.PostDraw();
    }
}
