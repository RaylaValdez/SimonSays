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
using Dalamud.Interface;
using Dalamud.Interface.Components;

namespace SimonSays.Windows;

/// <summary>
/// Represents a window for configuring settings.
/// </summary>
/// <remarks>
/// This class inherits from the Window class and implements the IDisposable interface.
/// </remarks>
public class ConfigWindow : Window, IDisposable
{
    private Configuration configuration;
    private string testText = "Simon Says : hum";
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

    /// <summary>
    /// Draws a checkbox for a chat channel at the specified index.
    /// </summary>
    /// <param name="index">The index of the chat channel.</param>
    private void DrawCheckbox(int index)
    {
        // Get the key and value of the chat channel at the specified index
        var key = ChatChannelTypes.ChatTypes.Keys.ToList()[index];
        var Value = ChatChannelTypes.ChatTypes[key];
        int channel = key;

        // Get the previous configuration value for the channel
        bool prevVal = false;
        configuration.EnabledChannels.TryGetValue(channel, out prevVal);

        // Draw the checkbox and update the configuration if the value changes
        bool newVal = prevVal;
        ImGui.TableNextColumn();
        if (ImGui.Checkbox(Value, ref newVal))
        {
            configuration.EnabledChannels[channel] = newVal;
            this.configuration.Save();
        }
    }

    // Method to draw the "Enabled" checkbox
    /// <summary>
    /// Draws the "Enabled" checkbox and updates the IsListening property in the configuration based on the user's selection.
    /// </summary>
    private void DrawEnabled()
    {
        // Get the current value of the IsListening property from the configuration
        bool isListening = configuration.IsListening;

        // Draw the "Enabled" checkbox and update the isListening variable with the new value
        if (ImGui.Checkbox("Enabled", ref isListening))
        {
            // Update the IsListening property in the configuration with the new value
            configuration.IsListening = isListening;

            // Save the updated configuration
            this.configuration.Save();
        }
    }

    /// <summary>
    /// Draws checkboxes for each channel in a table format.
    /// </summary>
    private void DrawCheckboxes()
    {
        // Begin the table with a unique identifier and specify the number of columns and table flags
        ImGui.BeginTable("simonsaystable", 3, ImGuiTableFlags.Borders);

        // Set up the column headers
        ImGui.TableSetupColumn("Channels");
        ImGui.TableSetupColumn("");
        ImGui.TableSetupColumn("");
        ImGui.TableHeadersRow();

        // Get the list of chat types
        var ChatTypes = ChatChannelTypes.ChatTypes;

        // Calculate the maximum number of rows in the table
        int max = (int)Math.Ceiling(ChatTypes.Count / 3d);

        // Loop through each row in the table
        for (int i = 0; i < Math.Ceiling(ChatTypes.Count / 3d); i++) // row loop
        {
            // Loop through each column in the table
            for (int j = 0; j < 3; j++) // column loop
            {
                // Check if the current index is out of bounds
                if ((j * max) + i > ChatTypes.Count - 1)
                    continue; // Move to the next iteration

                // Calculate the index of the current checkbox
                int index = (j * max) + i;

                // Draw the checkbox for the current channel
                DrawCheckbox(index);
            }
        }

        // End the table
        ImGui.EndTable();
    }

    /// <summary>
    /// Draws a text box for the catch phrase and allows the user to modify it. 
    /// The catch phrase is retrieved from the configuration and updated if modified.
    /// The updated configuration is then saved.
    /// </summary>
    private void DrawCatchPhBox()
    {
        // Get the current catch phrase from the configuration
        string inputText = configuration.CatchPhrase;

        // Draw the input text box and check if the text has been modified
        if (ImGui.InputText("", ref inputText, 500U))
        {
            // Trim the input text and update the catch phrase in the configuration
            configuration.CatchPhrase = inputText.Trim();

            // Save the updated configuration
            this.configuration.Save();
        }
    }

    /// <summary>
    /// Draws miscellaneous options in the ImGui window, including a checkbox for motion only, an input text field, and a send button.
    /// </summary>
    private void DrawMiscOptions()
    {
        // Get the value of the MotionOnly property from the configuration object
        bool motionOnly = configuration.MotionOnly;

        // Display a checkbox in the ImGui window with the label "Motion Only" and bind it to the motionOnly variable
        // If the checkbox value is changed, the motionOnly variable will be updated
        if (ImGui.Checkbox("Motion Only", ref motionOnly))
        {
            // Update the MotionOnly property in the configuration object with the new value
            configuration.MotionOnly = motionOnly;

            // Save the updated configuration
            this.configuration.Save();
        }

        // Display an input text field in the ImGui window with the label "Test Input" and bind it to the testText variable
        ImGui.InputText("Test Input", ref testText, 500U);

        // Display a button in the ImGui window with the label "Send"
        // If the button is clicked, execute the following code
        if (ImGui.Button("Send"))
        {
            // Call the Meat.Command method with the XivChatType.None parameter, the value of the testText variable, and ForceForTesting set to true
            Meat.Command(XivChatType.None, testText, ForceForTesting: true);
        }
    }

    /// <summary>
    /// Draws checkboxes for the experiment.
    /// </summary>
    private void DrawExperimentCheckboxes()
    {
        // Get the value of the PosSync property from the configuration object
        bool posSync = configuration.PosSync;

        // Display a checkbox labeled "Positional Sync" and bind its value to the posSync variable
        if (ImGui.Checkbox("Positional Sync", ref posSync))
        {
            // If the checkbox value is changed, update the PosSync property in the configuration object
            configuration.PosSync = posSync;
            // Save the updated configuration
            this.configuration.Save();
        }

        // Display a colored text indicating that Position Sync requires a Target
        ImGui.TextColored(new System.Numerics.Vector4(160, 160, 160, 0.8f), "Position Sync requires a Target.");

        // If PosSync is true, display buttons for syncing position and stopping sync
        if (configuration.PosSync)
        {
            if (ImGui.Button("Sync Position"))
            {
                // Call the StartScooch method to sync the position
                Meat.StartScooch();
            }
            if (ImGui.Button("Stop Sync"))
            {
                // Call the StopScooch method to stop syncing the position
                Meat.StopScooch();
            }
        }
    }

    /// <summary>
    /// Draws debug positional information on the screen.
    /// </summary>
    private void DrawDebugPositionalInformation()
    {
        // Check if the movement object is null
        if (Meat.movement == null)
            return;

        // Display whether SoftDisable is true or false
        ImGui.Text($"SoftDisable: {(Meat.movement.SoftDisable ? "True" : "False")}");

        // Display the distance to the target
        ImGui.Text($"Distance to target: {MathF.Sqrt(Meat.movement.DistanceSquared)}");

        // Display the rotation distance in degrees
        ImGui.Text($"Rotation distance (deg): {Meat.movement.RotationDistance}");

        // Get the current time
        DateTime currTime = DateTime.Now;

        // Display the time since the last movement
        ImGui.Text($"Time since last moved: {(currTime - Meat.movement.LastTimeMoved).TotalSeconds}");

        // Display the time since the last turn
        ImGui.Text($"Time since last turned: {(currTime - Meat.movement.LastTimeTurned).TotalSeconds}");

        // Add spacing between sections
        ImGui.Spacing();

        // Get the target position and rotation
        var tarPos = Meat.movement.DesiredPosition;
        float tarRot = Meat.movement.DesiredRotation;

        // Convert the rotation to degrees
        tarRot = Helpers.AngleConversions.ToDeg(tarRot);

        // Display the target destination
        ImGui.Text("Target Destination:");
        ImGui.Text($"X: {tarPos.X} Y: {tarPos.Y} Z: {tarPos.Z}");
        ImGui.Text($"Angle: {tarRot}");

        // Add spacing between sections
        ImGui.Spacing();

        // Display the last forward, left, and turn left values
        ImGui.Text($"Last Forward: {Meat.movement.LastForward}");
        ImGui.Text($"Last Left: {Meat.movement.LastLeft}");
        ImGui.Text($"Last Turn Left: {Meat.movement.LastTurnLeft}");
    }

    public string SearchedEmoteFilter = string.Empty;

    /// <summary>
    /// Overrides the Draw method from the base class to define custom drawing behavior.
    /// </summary>
    public override void Draw()
    {
        ImGui.Text("Enable or Disable listening to channels.");
        DrawEnabled();
        if (ImGui.BeginTabBar("Tabs"))
        {
            if (ImGui.BeginTabItem("Settings"))
            {
                ImGui.Text("");
                ImGui.Text("Catchphrase - This is what SimonSays listens to.");
                DrawCatchPhBox();
                ImGui.TextColored(new System.Numerics.Vector4(160, 160, 160, 0.8f), "Your Catchprase IS your security, change this to control who can command you.");
                ImGui.TextColored(new System.Numerics.Vector4(160, 160, 160, 0.8f), "Only share this to people you want to be able to command you.");
                ImGui.Text("");
                ImGui.Text("Channels - These are the channels SimonSays will listen to, waiting for your catchphrase.");
                DrawCheckboxes();
                ImGui.Text("");
                ImGui.Separator();
                ImGui.Text("");
                ImGui.Text("Misc Options");
                DrawMiscOptions();
                ImGui.Text("");
                ImGui.Separator();
                ImGui.Text("");
                ImGui.Text("Experimental Features");
                DrawExperimentCheckboxes();
                ImGui.EndTabItem();
            }
            if (configuration.PosSync && ImGui.BeginTabItem("Offsets"))
            {
                ImGui.Text("");
                ImGui.SetNextItemOpen(true);
                if (ImGui.TreeNode("Info"))
                {
                    ImGui.Text("");
                    ImGui.Text("Set offsets for particular emotes.");
                    ImGui.Text("");
                    ImGui.TextColored(new System.Numerics.Vector4(160, 160, 160, 0.8f), "X - Foward / Back.");
                    ImGui.TextColored(new System.Numerics.Vector4(160, 160, 160, 0.8f), "Z - Left / Right.");
                    ImGui.TextColored(new System.Numerics.Vector4(160, 160, 160, 0.8f), "R - Rotation.");
                    ImGui.EndTabItem();
                    ImGui.Text("");
                    ImGui.Text("Usage :");
                    ImGui.Text("");
                    ImGui.TextColored(new System.Numerics.Vector4(160, 160, 160, 0.8f), "Click Add Emote Offset to get started.");
                    ImGui.TextColored(new System.Numerics.Vector4(160, 160, 160, 0.8f), "Label this offset however you wish, you may have multiple offsets for a single emote.");
                    ImGui.TextColored(new System.Numerics.Vector4(160, 160, 160, 0.8f), "Choose the emote you wish to offset your positon with.");
                    ImGui.TextColored(new System.Numerics.Vector4(160, 160, 160, 0.8f), "Increase / decrease each offset field to your desired position.");
                    ImGui.Text("");
                    ImGui.TextColored(new System.Numerics.Vector4(160, 160, 160, 0.8f), "Now, when you use /simonsays with an emote you've specified an offset for, you'll move the offset you've provided.");
                    ImGui.TextColored(new System.Numerics.Vector4(160, 160, 160, 0.8f), "If you wish to test your offset without emoting, do /sync emote!");
                    ImGui.Text("");
                    ImGui.Separator();

                    ImGui.TreePop();
                }

                ImGui.Text("");
                if (ImGui.BeginTable("OffsetsTable", 5))
                {
                    ImGui.TableSetupColumn("Enable", ImGuiTableColumnFlags.WidthStretch, 40.0f);
                    ImGui.TableSetupColumn("Label", ImGuiTableColumnFlags.WidthStretch, 100.0f);
                    ImGui.TableSetupColumn("Emote", ImGuiTableColumnFlags.WidthStretch, 100.0f);
                    ImGui.TableSetupColumn("Offsets : Fwd/Back | Left/Right | Rotation", ImGuiTableColumnFlags.WidthStretch, 90.0f);
                    ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch, 10.0f);
                    ImGui.TableHeadersRow();

                    int i = 0;
                    foreach (EmoteOffsets offset in configuration.EmoteOffsets)
                    {
                        ImGui.TableNextRow();
                        ImGui.PushID(i++);

                        // Enable
                        ImGui.TableNextColumn();

                        if (ImGuiComponents.IconButton(FontAwesomeIcon.Trash))
                        {
                            configuration.EmoteOffsets.Remove(offset);
                            configuration.Save();
                            break;
                        }
                        ImGui.SameLine();
                        if (ImGui.Checkbox("##Enabled", ref offset.Enabled))
                        {
                            configuration.Save();
                        }

                        // Label
                        ImGui.TableNextColumn();
                        ImGui.SetNextItemWidth(-1);
                        if (ImGui.InputTextWithHint("##Label", "Label", ref offset.Label, 64))
                        {
                            configuration.Save();
                        }

                        // Emote
                        ImGui.TableNextColumn();
                        ImGui.SetNextItemWidth(-1);

                        if (ImGui.BeginCombo("##Emote", offset.Emote, ImGuiComboFlags.HeightLargest))
                        {
                            // Filter through the emotes by the searched

                            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                            ImGui.InputTextWithHint("##EmoteFilter", "Search...", ref SearchedEmoteFilter, 128);

                            var size = new System.Numerics.Vector2(ImGui.GetItemRectSize().X, 240);

                            // Add a scroll panel inside the combobox
                            if (ImGui.BeginChild("##EmoteScroll", size))
                            {
                                IEnumerable<string> emoteList = Service.Emotes.Where((s) => s.Contains(SearchedEmoteFilter));

                                if (ImGui.Selectable("[None]"))
                                {
                                    offset.Emote = string.Empty;
                                }

                                foreach (string emote in emoteList)
                                {
                                    string e = emote;
                                    Meat.SanitizeEmote(ref e);
                                    if (ImGui.Selectable(e))
                                    {
                                        offset.Emote = e;
                                    }
                                }

                                ImGui.EndChild();
                            }
                            ImGui.EndCombo();
                        }

                        // Offset
                        ImGui.TableNextColumn();
                        float offsetColumnSize = (ImGui.GetColumnWidth() / 3.0f) - 6.0f;
                        ImGui.SetNextItemWidth(offsetColumnSize);
                        if (ImGui.DragFloat("##OffsetX", ref offset.X, 0.05f, -10.0f, 10.0f))
                        {
                            configuration.Save();
                        }
                        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                        {
                            ImGui.SetTooltip("Offset X");
                        }

                        ImGui.SameLine();
                        ImGui.SetNextItemWidth(offsetColumnSize);

                        if (ImGui.DragFloat("##OffsetZ", ref offset.Z, 0.05f, -10.0f, 10.0f))
                        {
                            configuration.Save();
                        }
                        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                        {
                            ImGui.SetTooltip("Offset Z");
                        }

                        ImGui.SameLine();
                        ImGui.SetNextItemWidth(offsetColumnSize);
                        if (ImGui.DragFloat("##OffsetR", ref offset.R, 0.05f, -180.0f, 180f))
                        {
                            configuration.Save();
                        }
                        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                        {
                            ImGui.SetTooltip("Offset Rotation");
                        }

                        ImGui.PopID();
                    }

                    ImGui.EndTable();
                }
                if (ImGui.Button("Add Emote Offset"))
                {
                    configuration.EmoteOffsets.Add(new EmoteOffsets());
                    configuration.Save();
                }
            }
            if (ImGui.BeginTabItem("Usage"))
            {
                ImGui.Text("");
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
                ImGui.Text("");
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
                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                    {
                        ImGui.SetTooltip("Join our Discord!");
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Repo"))
                    {
                        ImGui.SetClipboardText(Plugin.Repo);

                        //Process.Start(new ProcessStartInfo()
                        //{
                        //    FileName = Plugin.Repo,
                        //    UseShellExecute = true
                        //});
                    }
                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                    {
                        ImGui.SetTooltip("Copy repo to Clipboard.");
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
                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                    {
                        ImGui.SetTooltip("Take a peak under the hood.");
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
                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                    {
                        ImGui.SetTooltip("Only if you want to!");
                    }
                });


                ImGui.EndTabItem();
            }
            ImGui.EndTabBar();
        }
    }
}
