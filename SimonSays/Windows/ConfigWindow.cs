using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Dalamud.Game.Text;
using Lumina.Excel.GeneratedSheets2;
using System.Reflection;
using PunishLib.ImGuiMethods;
using Dalamud.Interface.Internal;
using FFXIVClientStructs.FFXIV.Common.Math;

namespace SimonSays.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration configuration;
    private string testText = "Test Me";
    private IDalamudTextureWrap? aboutImage;

    private static readonly bool EnableDebug = false;

    public ConfigWindow(Plugin plugin) : base(
        "SimonSays Settings",
        ImGuiWindowFlags.NoScrollbar)
    {
        this.Size = new System.Numerics.Vector2(500, 900);
        this.SizeCondition = ImGuiCond.FirstUseEver;

        this.configuration = Plugin.Configuration;

        var imagePath = Path.Combine(Plugin.PluginInterfaceStatic.AssemblyLocation.Directory?.FullName!, "ts500.png");
        aboutImage = Plugin.PluginInterfaceStatic.UiBuilder.LoadImage(imagePath);
    }

    public void Dispose() { }

    private void DrawCheckbox(int index)
    {
        var key = ChatChannelTypes.ChatTypes.Keys.ToList()[index];
        var Value = ChatChannelTypes.ChatTypes[key];
        int channel = key;

        // get previous config Value
        bool prevVal = false;
        configuration.EnabledChannels.TryGetValue(channel, out prevVal);

        // draw checkbox and update configuration if Value changes
        bool newVal = prevVal;
        ImGui.TableNextColumn();
        if (ImGui.Checkbox(Value, ref newVal))
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

        var ChatTypes = ChatChannelTypes.ChatTypes;

        // fucking maff shit:
        // j * max + i
        // Draws the columns up-down, left-right rather than left-right, up-down

        // Max is the maximum of rows in the table
        int max = (int)Math.Ceiling(ChatTypes.Count / 3d);
        for (int i = 0; i < Math.Ceiling(ChatTypes.Count / 3d); i++) // row loop
        {
            for (int j = 0; j < 3; j++) // column loop
            {
                if ((j * max) + i > ChatTypes.Count - 1) // if out of bounds, commence to next iteration
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
            Meat.Command(XivChatType.None, testText, ForceForTesting: true);
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
        ImGui.TextColored(new System.Numerics.Vector4(160, 160, 160, 0.8f), "Position Sync requires a Target.");
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

    private void DrawDebugPositionalInformation()
    {
        if (Meat.movement == null)
            return;

        ImGui.Text($"SoftDisable: {(Meat.movement.SoftDisable ? "True" : "False")}");
        ImGui.Text($"Distance to target: {MathF.Sqrt(Meat.movement.DistanceSquared)}");
        ImGui.Text($"Rotation distance (deg): {Meat.movement.RotationDistance}");

        DateTime currTime = DateTime.Now;
        ImGui.Text($"Time since last moved: {(currTime - Meat.movement.LastTimeMoved).TotalSeconds}");
        ImGui.Text($"Time since last turned: {(currTime - Meat.movement.LastTimeTurned).TotalSeconds}");

        ImGui.Spacing();

        var tarPos = Meat.movement.DesiredPosition;
        float tarRot = Meat.movement.DesiredRotation;
        tarRot = Helpers.AngleConversions.ToDeg(tarRot);

        ImGui.Text("Target Destination:");
        ImGui.Text($"X: {tarPos.X} Y: {tarPos.Y} Z: {tarPos.Z}");
        ImGui.Text($"Angle: {tarRot}");

        ImGui.Spacing();

        ImGui.Text($"Last Forward: {Meat.movement.LastForward}");
        ImGui.Text($"Last Left: {Meat.movement.LastLeft}");
        ImGui.Text($"Last Turn Left: {Meat.movement.LastTurnLeft}");
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
                ImGui.Text("");
                ImGui.Text("/sync - Will attempt to sync positions with your Target, only works if Position Syncing is enabled in Experimental Features.");
                ImGui.Text("");
                ImGui.Text("/simonsays <TheirEmote> <YourEmote> <ShouldSync> - There are multiple ways to use this command.");
                ImGui.Text("Example 1 - '/simonsays hum' - This will make you and the person you're targetting hum.");
                ImGui.Text("Example 2 - '/simonsays hum dance' - This will make the person you're targetting hum and make yourself dance.");
                ImGui.Text("Example 3 - '/simonsays hum dance true' - This will attempt to sync your positions prior to making your target hum and yourself dance.");
                ImGui.Text("ShouldSync being 'true' in this case can be used on all previous examples.");
                ImGui.Text("");
                ImGui.Separator();
                ImGui.Text("");
                ImGui.Text("Macros :");
                ImGui.Text("");
                ImGui.Text("Macros can be used to initiate a single Emote or chain together multiple Emotes.");
                ImGui.Text("");
                ImGui.Text("Single Emote Example :");
                ImGui.Text("");
                string example =
                    "/micon hum Emote\n" +                    "/tell <t> Simon Says : hum\n" +                    "/hum\n";
                ImGui.InputTextMultiline("##Example", ref example, 200, new(200, 75), ImGuiInputTextFlags.ReadOnly);
                ImGui.Text("");
                ImGui.Text("Multiple Emote Example :");
                ImGui.Text("");
                string multiexample =
                    "/micon hum Emote\n" +                    "/tell <t> Simon Says : hum\n" +                    "/hum\n" +
                    "/wait 3\n" +
                    "/tell <t> Simon Says : dance\n" +
                    "/dance\n";
                ImGui.InputTextMultiline("##MultiExample", ref multiexample, 200, new(200, 125), ImGuiInputTextFlags.ReadOnly);
                ImGui.Text("");
                ImGui.Separator();
                ImGui.Text("");
                ImGui.Text("Experimental Features :");
                ImGui.Text("");
                ImGui.Text("Positional Syncing : ");
                ImGui.Text("");
                ImGui.Text("Positional Syncing is a way to align perfectly with your Target.");
                ImGui.Text("Enable it in the Settings Tab.");
                ImGui.Text("Simply Target the person you wish to sync positions with and use the /sync Command.");
                ImGui.Text("Should you fail to sync and end up running around your Target, use the stop sync button in the Settings tab.");
                ImGui.EndTabItem();
            }

            if (EnableDebug && ImGui.BeginTabItem("Debug"))
            {
                DrawDebugPositionalInformation();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("About"))
            {
                ImGuiEx.ImGuiLineCentered("AboutVersion", () =>
                {
                    ImGui.Text("SimonSays - " + typeof(Plugin).Assembly.GetName().Version?.ToString());
                });
                ImGui.Text("");
                ImGuiEx.ImGuiLineCentered("AboutCreators", () =>
                {
                    ImGui.Text("Created by Rayla & Frdhog @ Triquetra Studios");
                });
                ImGui.Text("");
                if (aboutImage != null)
                {
                    ImGuiEx.ImGuiLineCentered("AboutImage", () =>
                    {
                        ImGui.Image(aboutImage.ImGuiHandle, new System.Numerics.Vector2(300, 300));
                    });
                }
                ImGui.Text("");
                ImGuiEx.ImGuiLineCentered("AboutDiscord", () =>
                {
                    ImGui.Text("Join our Discord to stay up to date with releases, updates & more!");
                });
                ImGui.Text("");
                ImGuiEx.ImGuiLineCentered("AboutButtons", () =>
                {
                    if (ImGui.Button("Discord"))
                    {
                        Process.Start(new ProcessStartInfo()
                        {
                            FileName = Plugin.DiscordURL,
                            UseShellExecute = true
                        });
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Repo"))
                    {
                        Process.Start(new ProcessStartInfo()
                        {
                            FileName = Plugin.Repo,
                            UseShellExecute = true
                        });
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Source"))
                    {
                        Process.Start(new ProcessStartInfo()
                        {
                            FileName = Plugin.Source,
                            UseShellExecute = true
                        });
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Buy me a coffee!"))
                    {
                        Process.Start(new ProcessStartInfo()
                        {
                            FileName = Plugin.BuyMeACoffee,
                            UseShellExecute = true
                        });
                    }
                });


                ImGui.EndTabItem();
            }
            ImGui.EndTabBar();
        }
    }
}
