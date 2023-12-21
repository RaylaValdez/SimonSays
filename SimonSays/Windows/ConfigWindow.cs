using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Dalamud.Game.Text;
using Lumina.Excel.GeneratedSheets2;


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

    private void DrawExperimentCheckboxes()
    {
        bool posSync = configuration.PosSync;
        if (ImGui.Checkbox("Positional Sync", ref posSync))
        {
            configuration.PosSync = posSync;
            this.configuration.Save();

        }
        ImGui.TextColored(new Vector4(160, 160, 160, 0.8f), "Position Sync requires a target.");
        if (configuration.PosSync)
        {
            if (ImGui.Button("Sync Position"))
            {
                Meat.StartScooch();
            }
            if (ImGui.Button("Stop Sync"))
            {
                Meat.StopScooch();
            }
        }


    }

    public override void Draw()
    {
        if (ImGui.BeginTabBar("Tabs"))
        {
            if (ImGui.BeginTabItem("Settings"))
            {
                ImGui.Text("Enable the plugin");
                DrawEnabled();
                ImGui.Separator();
                ImGui.Text("Experimental Features");
                DrawExperimentCheckboxes();
                ImGui.Separator();
                ImGui.Text("Choose which chat channels you wish SimonSays to listen to");
                DrawCheckboxes();
                ImGui.Text("Choose your catchphrase, by default 'Simon Says :'");
                DrawCatchPhBox();
                ImGui.Separator();
                ImGui.Text("Misc Options");
                DrawMiscOptions();
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Usage"))
            {
                ImGui.Text("Commands :");
                ImGui.Text("");
                ImGui.Text("/simonsaysconfig - Will open this window.");
                ImGui.Text("/sync - Will attempt to sync positions with your target, only works if Position Syncing is enabled in Experimental Features.");
                ImGui.Text("/simonsays <emote> - Will sync the stated emote with your target, emote does not require slash '/'.");
                ImGui.Text("");
                ImGui.Separator();
                ImGui.Text("");
                ImGui.Text("Macros :");
                ImGui.Text("");
                ImGui.Text("Macros can be used to initiate a single emote or chain together multiple emotes.");
                ImGui.Text("");
                ImGui.Text("Single Emote Example :");
                ImGui.Text("");
                string example =
                    "/micon hum emote\n" +                    "/tell <t> Simon Says: hum\n" +                    "/hum\n";
                ImGui.InputTextMultiline("##Example", ref example, 200, new(200, 75), ImGuiInputTextFlags.ReadOnly);
                ImGui.Text("");
                ImGui.Text("Multiple Emote Example :");
                ImGui.Text("");
                string multiexample =
                    "/micon hum emote\n" +                    "/tell <t> Simon Says: hum\n" +                    "/hum\n" +
                    "/wait 3\n" +
                    "/tell <t> Simon Says : dance\n" +
                    "/dance\n";
                ImGui.InputTextMultiline("##MultiExample", ref multiexample, 200, new(200,125), ImGuiInputTextFlags.ReadOnly);
                ImGui.Text("");
                ImGui.Separator();
                ImGui.Text("");
                ImGui.Text("Experimental Features :");
                ImGui.Text("");
                ImGui.Text("Positional Syncing : ");
                ImGui.Text("");
                ImGui.Text("Positional Syncing is a way to align perfectly with your target.");
                ImGui.Text("Enable it in the Settings Tab.");
                ImGui.Text("Simply target the person you wish to sync positions with and use the /sync command.");
                ImGui.Text("Should you fail to sync and end up running around your target, use the stop sync button in the Settings tab.");
                ImGui.EndTabItem();
            }
        ImGui.EndTabBar();
        }
    }
}
