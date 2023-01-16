using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Dalamud.Game.Text;


namespace SimonSays.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration configuration;
    private string testText = "Test Me";

    public ConfigWindow(Plugin plugin) : base(
        "SimonSays Settings",
        ImGuiWindowFlags.NoScrollbar)
    {
        this.Size = new Vector2(500, 900);
        this.SizeCondition = ImGuiCond.FirstUseEver;

        this.configuration = Plugin.Configuration;
    }

    public void Dispose() { }

    private void DrawCheckbox(int index)
    {
        var key = ChatChannelTypes.chatTypes.Keys.ToList()[index];
        var value = ChatChannelTypes.chatTypes[key];
        int channel = key;

        // get previous config value
        bool prevVal = false;
        configuration.EnabledChannels.TryGetValue(channel, out prevVal);

        // draw checkbox and update configuration if value changes
        bool newVal = prevVal;
        ImGui.TableNextColumn();
        if (ImGui.Checkbox(value, ref newVal))
        {
            configuration.EnabledChannels[channel] = newVal;
            this.configuration.Save();
        }
    }

    private void DrawEnabled()
    {
        bool isListening = configuration.IsListening;
        if (ImGui.Checkbox("Enabled", ref isListening))
        {
            configuration.IsListening = isListening;
            this.configuration.Save();
        }

    }

    private void DrawCheckboxes()
    {
        ImGui.BeginTable("simonsaystable", 3, ImGuiTableFlags.Borders);
        ImGui.TableSetupColumn("Channels");
        ImGui.TableSetupColumn("");
        ImGui.TableSetupColumn("");
        ImGui.TableHeadersRow();

        var chatTypes = ChatChannelTypes.chatTypes;

        // fucking maff shit:
        // j * max + i
        // Draws the columns up-down, left-right rather than left-right, up-down

        // Max is the maximum of rows in the table
        int max = (int)Math.Ceiling(chatTypes.Count / 3d);
        for (int i = 0; i < Math.Ceiling(chatTypes.Count / 3d); i++) // row loop
        {
            for (int j = 0; j < 3; j++) // column loop
            {
                if ((j * max) + i > chatTypes.Count - 1) // if out of bounds, commence to next iteration
                    continue;

                int index = (j * max) + i;

                // get channel by key, indexing the key list rather than the dictionary
                DrawCheckbox(index);
            }
        }
        ImGui.EndTable();
    }

    private void DrawCatchPhBox()
    {
        string inputText = configuration.CatchPhrase;
        if (ImGui.InputText("", ref inputText, 500U))
        {
            configuration.CatchPhrase = inputText.Trim();
            this.configuration.Save();
        }
    }
    private void DrawMiscOptions()
    {
        bool motionOnly = configuration.MotionOnly;
        if (ImGui.Checkbox("Motion Only", ref motionOnly))
        {
            configuration.MotionOnly = motionOnly;
            this.configuration.Save();
        }
        ImGui.InputText("Test Input", ref testText, 500U);
        if (ImGui.Button("Send"))
        {
            Meat.Command(XivChatType.None, testText, forceForTesting: true);
        }
    }

    public override void Draw()
    {
        ImGui.Text("Enable the plugin");
        DrawEnabled();
        ImGui.Separator();
        ImGui.Text("Choose which chat channels you wish SimonSays to listen to");
        DrawCheckboxes();
        ImGui.Text("Choose your catchphrase, by default 'Simon Says :'");
        DrawCatchPhBox();
        ImGui.Separator();
        ImGui.Text("Misc Options");
        DrawMiscOptions();

    }
}
