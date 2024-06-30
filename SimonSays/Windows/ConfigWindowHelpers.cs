using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using ImGuiNET;
using ImPlotNET;
using PunishLib.ImGuiMethods;
using SimonSays.Helpers;
using SimonSays.Windows;
using SimonSays.Windows.Tabs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;
using SimonSays;

internal static class ConfigWindowHelpers
{
    public static bool isFirstFrame = true;
    public static Vector2 contextMenuPosition = new();
    public static string selectedLayout = string.Empty;
    private static string SelectedMember = string.Empty;
    public static bool namingWindowOpen = false;
    public static bool renamingWindowOpen = false;
    public static bool newMemberWindowOpen = false;
    public static bool contextPopupOpen = false;
    public static string nameBuffer = "Change Me";
    public static string filterText = string.Empty;
    public static string renameBuffer = string.Empty;
    public static Vector4 oldTitleColorActive = Vector4.Zero;
    public static Preset? activePreset = null;
    public static List<Vector4> memberColors =
    [
        new Vector4(0.722f, 0.325f, 0.623f, 1.0f),
        new Vector4(0.7051666666666666f, 0.335625f, 0.605875f, 1.0f),
        new Vector4(0.6883333333333332f, 0.34625f, 0.5887499999999999f, 1.0f),
        new Vector4(0.6715f, 0.356875f, 0.5716249999999999f, 1.0f),
        new Vector4(0.6546666666666667f, 0.36750000000000005f, 0.5545f, 1.0f),
        new Vector4(0.6378333333333334f, 0.37812500000000004f, 0.537375f, 1.0f),
        new Vector4(0.621f, 0.38875f, 0.52025f, 1.0f),
        new Vector4(0.6041666666666666f, 0.399375f, 0.503125f, 1.0f),
        new Vector4(0.5873333333333334f, 0.41000000000000003f, 0.48600000000000004f, 1.0f),
        new Vector4(0.5705f, 0.42062499999999997f, 0.46887500000000004f, 1.0f),
        new Vector4(0.5536666666666665f, 0.43125f, 0.45174999999999993f, 1.0f),
        new Vector4(0.5368333333333333f, 0.44187499999999996f, 0.434625f, 1.0f),
        new Vector4(0.52f, 0.4525f, 0.4175f, 1.0f),
        new Vector4(0.5031666666666667f, 0.463125f, 0.40037500000000004f, 1.0f),
        new Vector4(0.4863333333333334f, 0.47374999999999995f, 0.38325000000000004f, 1.0f),
        new Vector4(0.46950000000000003f, 0.484375f, 0.36612500000000003f, 1.0f),
        new Vector4(0.4526666666666668f, 0.49499999999999994f, 0.3490000000000001f, 1.0f),
        new Vector4(0.4358333333333334f, 0.505625f, 0.33187500000000003f, 1.0f),
        new Vector4(0.4190000000000001f, 0.5162499999999999f, 0.3147500000000001f, 1.0f),
        new Vector4(0.4021666666666668f, 0.526875f, 0.29762500000000014f, 1.0f),
        new Vector4(0.3853333333333334f, 0.5374999999999999f, 0.28050000000000014f, 1.0f),
        new Vector4(0.3685000000000001f, 0.5481249999999999f, 0.26337500000000014f, 1.0f),
        new Vector4(0.3516666666666668f, 0.5587499999999999f, 0.24625000000000014f, 1.0f),
        new Vector4(0.3348333333333335f, 0.5693749999999999f, 0.22912500000000016f, 1.0f)

    ];
    public static string testText = "Simon Says : hum";
    public static ISharedImmediateTexture? aboutImage;

    public static string GetSelectedMember()
    {
        return SelectedMember;
    }

    public static void SetSelectedMember(string value)
    {
        SelectedMember = value;
    }

    public static bool IconButtonWithText(FontAwesomeIcon icon, string text, string tooltip)
    {
        ImGui.PushID(text);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0.0f));
        var selected = ImGuiComponents.IconButton(icon); // <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< Imgui icon button here
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(tooltip);
        }

        if (!text.IsNullOrEmpty())
        {
            ImGui.SameLine();
            selected |= ImGui.Button(text);
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(tooltip);
            }
        }
        ImGui.PopStyleVar();
        ImGui.PopID();

        return selected;
    }

    /// <summary>
    /// Draws a checkbox for a chat channel at the specified index.
    /// </summary>
    /// <param name="index">The index of the chat channel.</param>
    public static void DrawCheckbox(int index)
    {
        // Get the key and value of the chat channel at the specified index
        var key = SaltAndPepper.ChatTypes.Keys.ToList()[index];
        var Value = SaltAndPepper.ChatTypes[key];
        var channel = key;

        // Get the previous configuration value for the channel
        Potatoes.Configuration!.EnabledChannels.TryGetValue(channel, out var prevVal);

        // Draw the checkbox and update the configuration if the value changes
        var newVal = prevVal;
        ImGui.TableNextColumn();
        if (ImGui.Checkbox(Value, ref newVal))
        {
            Potatoes.Configuration!.EnabledChannels[channel] = newVal;
            Potatoes.Configuration!.Save();
        }
    }

    // Method to draw the "Enabled" checkbox
    /// <summary>
    /// Draws the "Enabled" checkbox and updates the IsListening property in the configuration based on the user's selection.
    /// </summary>
    public static void DrawEnabled()
    {
        // Get the current value of the IsListening property from the configuration
        var isListening = Potatoes.Configuration!.IsListening;

        // Draw the "Enabled" checkbox and update the isListening variable with the new value
        if (ImGui.Checkbox("Enabled", ref isListening))
        {
            // Update the IsListening property in the configuration with the new value
            Potatoes.Configuration!.IsListening = isListening;

            // Save the updated configuration
            Potatoes.Configuration!.Save();
        }
    }

    /// <summary>
    /// Draws checkboxes for each channel in a table format.
    /// </summary>
    public static void DrawCheckboxes()
    {
        // Begin the table with a unique identifier and specify the number of columns and table flags
        ImGui.BeginTable("simonsaystable", 3, ImGuiTableFlags.Borders);

        // Set up the column headers
        ImGui.TableSetupColumn("Channels");
        ImGui.TableSetupColumn("");
        ImGui.TableSetupColumn("");
        ImGui.TableHeadersRow();

        // Get the list of chat types
        var ChatTypes = SaltAndPepper.ChatTypes;

        // Calculate the maximum number of rows in the table
        var max = (int)Math.Ceiling(ChatTypes.Count / 3d);

        // Loop through each row in the table
        for (var i = 0; i < Math.Ceiling(ChatTypes.Count / 3d); i++) // row loop
        {
            // Loop through each column in the table
            for (var j = 0; j < 3; j++) // column loop
            {
                // Check if the current index is out of bounds
                if ((j * max) + i > ChatTypes.Count - 1)
                    continue; // Move to the next iteration

                // Calculate the index of the current checkbox
                var index = (j * max) + i;
                ConfigWindowHelpers.

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
    public static void DrawCatchPhBox()
    {
        // Get the current catch phrase from the configuration
        var inputText = Potatoes.Configuration!.CatchPhrase;
        ImGui.SetNextItemWidth(250f);
        // Draw the input text box and check if the text has been modified
        if (ImGui.InputText("", ref inputText, 500U))
        {
            // Trim the input text and update the catch phrase in the configuration
            Potatoes.Configuration!.CatchPhrase = inputText.Trim();

            // Save the updated configuration
            Potatoes.Configuration!.Save();
        }
    }

    /// <summary>
    /// Draws miscellaneous options in the ImGui window, including a checkbox for motion only, an input text field, and a send button.
    /// </summary>
    public static void DrawMiscOptions()
    {
        
    }

    /// <summary>
    /// Draws checkboxes for the experiment.
    /// </summary>
    public static void DrawExperimentCheckboxes()
    {
        // Get the value of the PosSync property from the configuration object
        var posSync = Potatoes.Configuration!.PosSync;
            

        // Display a colored text indicating that Position Sync requires a Target
        ImGui.TextColored(new System.Numerics.Vector4(160, 160, 160, 0.8f), "Position Sync requires a Target.");

        ImGuiEx.ImGuiLineRightAlign("Settings_Override_Position_Sync", () =>
        {
            // If PosSync is true, display buttons for syncing position and stopping sync
            if (Potatoes.Configuration!.PosSync)
            {
                if (ImGui.Button("Sync Position"))
                {
                    // Call the StartScooch method to sync the position
                    Meat.StartScooch();
                }
                ImGui.SameLine();
                ImGui.Text("  ");
            }
        });

        ImGuiEx.ImGuiLineRightAlign("Settings_Override_Positon_Stop", () =>
        {
            // If PosSync is true, display buttons for syncing position and stopping sync
            if (Potatoes.Configuration!.PosSync)
            {
                if(ImGui.Button("Stop Sync"))
{
                    // Call the StopScooch method to stop syncing the position
                    Meat.StopScooch();
                }
                ImGui.SameLine();
                ImGui.Text("  ");
            }
        });


    }

    /// <summary>
    /// Draws debug positional information on the screen.
    /// </summary>
    public static void DrawDebugPositionalInformation()
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
        var currTime = DateTime.Now;

        // Display the time since the last movement
        ImGui.Text($"Time since last moved: {(currTime - Meat.movement.LastTimeMoved).TotalSeconds}");

        // Display the time since the last turn
        ImGui.Text($"Time since last turned: {(currTime - Meat.movement.LastTimeTurned).TotalSeconds}");

        // Add spacing between sections
        ImGui.Spacing();

        // Get the target position and rotation
        var tarPos = Meat.movement.DesiredPosition;
        var tarRot = Meat.movement.DesiredRotation;

        // Convert the rotation to degrees
        tarRot = SimonSays.Helpers.AngleConversions.ToDeg(tarRot);

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

    public static void PushStyles()
    {
        // Custom Style
        // Variables
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10, 10));
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(4, 4));
        ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(4, 4));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4, 4));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemInnerSpacing, new Vector2(4, 4));
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 21f);
        ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarSize, 21f);
        ImGui.PushStyleVar(ImGuiStyleVar.GrabMinSize, 21f);
        // Borders
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, 1f);
        ImGui.PushStyleVar(ImGuiStyleVar.PopupBorderSize, 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0f);
        // Rounding
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 6f);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 4f);
        ImGui.PushStyleVar(ImGuiStyleVar.PopupRounding, 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarRounding, 9f);
        ImGui.PushStyleVar(ImGuiStyleVar.GrabRounding, 3f);
        ImGui.PushStyleVar(ImGuiStyleVar.TabRounding, 4f);
        // Alignment
        ImGui.PushStyleVar(ImGuiStyleVar.WindowTitleAlign, new Vector2(0.0f, 0.50f));
        ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new Vector2(0.5f, 0.5f));
        ImGui.PushStyleVar(ImGuiStyleVar.SelectableTextAlign, new Vector2(0, 0));
        // Colors
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, new Vector4(081, 054, 148, 211) / 255f);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(130, 068, 153, 241) / 255f);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(184, 083, 159, 241) / 255f);
        ImGui.PushStyleColor(ImGuiCol.Separator, new Vector4(130, 068, 153, 241) / 255f);
        ImGui.PushStyleColor(ImGuiCol.SeparatorHovered, new Vector4(130, 068, 153, 200) / 255f);
        ImGui.PushStyleColor(ImGuiCol.SeparatorActive, new Vector4(184, 083, 159, 241) / 255f);
        ImGui.PushStyleColor(ImGuiCol.ResizeGripActive, new Vector4(184, 083, 159, 241) / 255f);
        ImGui.PushStyleColor(ImGuiCol.TabHovered, new Vector4(130, 068, 153, 200) / 255f);
        ImGui.PushStyleColor(ImGuiCol.TabActive, new Vector4(184, 083, 159, 241) / 255f);
        ImGui.PushStyleColor(ImGuiCol.Border, (new Vector4(0.35f, 0.35f, 0.35f, 0.75f)));
        ImGui.PushStyleColor(ImGuiCol.FrameBg, (new Vector4(50, 46, 51, 122) / 255f));
    }

    public static void ContextPopup()
    {


        var contextMenuSize = new Vector2(135, 34);
        if (ConfigWindowHelpers.contextPopupOpen)
        {
            // Set window size on the first frame
            if (isFirstFrame)
            {
                contextMenuPosition = ImGui.GetMousePos();
                isFirstFrame = false;
            }

            ImGui.SetNextWindowSize(contextMenuSize);
            ImGui.SetNextWindowPos(contextMenuPosition);

            if (ImGui.Begin("Contextual Menu", ref ConfigWindowHelpers.contextPopupOpen, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar))
            {
                Sausages.Log.Debug("You should be seeing a context menu right about now");

                if (ImGui.Selectable("Rename Preset"))
                {
                    ImGui.SetNextWindowPos(new Vector2(10.0f, 100.0f));
                    ImGui.SetNextWindowSize(new Vector2(200.0f, 60.0f));
                    ConfigWindowHelpers.renamingWindowOpen = true;
                    ConfigWindowHelpers.contextPopupOpen = false;
                    ConfigWindowHelpers.renameBuffer = ConfigWindowHelpers.activePreset?.PresetName ?? "";
                }
            }
            ImGui.End();
            if (!ImGui.IsItemHovered() && ImGui.IsMouseReleased(ImGuiMouseButton.Left))
            {
                ConfigWindowHelpers.contextPopupOpen = false;
            }

            // Reset isFirstFrame if contextPopupOpen is false
            if (!ConfigWindowHelpers.contextPopupOpen)
            {
                isFirstFrame = true;
            }


        }
    }

    public static void OpenNamingWindow()
    {
        if (ConfigWindowHelpers.namingWindowOpen)
        {

            if (ImGui.Begin("Preset Name", ref ConfigWindowHelpers.namingWindowOpen))
            {
                if (ImGui.BeginChild("##", new Vector2(-1, -1), true))
                {
                    ImGui.Text("Name");
                    ImGui.SameLine();
                    ImGui.InputText("##", ref ConfigWindowHelpers.nameBuffer, (uint)ConfigWindow.BufferSize);
                    ImGui.SameLine();
                    if (ConfigWindowHelpers.IconButtonWithText(FontAwesomeIcon.Save, "", "Save"))
                    {
                        var preset = new Preset
                        {
                            PresetName = ConfigWindowHelpers.nameBuffer,
                        };

                        var jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };
                        var options = jsonSerializerOptions;
                        var jsonString = JsonSerializer.Serialize(preset, options);
                        File.WriteAllText(Potatoes.PresetDirectory + "/" + ConfigWindowHelpers.nameBuffer + ".json", jsonString);
                        ConfigWindowHelpers.
                                                namingWindowOpen = false;

                    }
                }
                ImGui.EndChild();
            }
            ImGui.End();
        }
    }

    public static void OpenReNamingWindow()
    {
        if (ConfigWindowHelpers.renamingWindowOpen)
        {

            if (ImGui.Begin("Rename a preset", ref ConfigWindowHelpers.renamingWindowOpen))
            {
                if (ImGui.BeginChild("##", new Vector2(-1, -1), true))
                {
                    ImGui.Text("Name");
                    ImGui.SameLine();
                    ImGui.InputText("##", ref ConfigWindowHelpers.renameBuffer, (uint)ConfigWindow.BufferSize);
                    ImGui.SameLine();
                    if (ConfigWindowHelpers.IconButtonWithText(FontAwesomeIcon.Save, "", "Save"))
                    {
                        if (ConfigWindowHelpers.activePreset == null)
                        {
                            return;
                        }
                        var prevName = ConfigWindowHelpers.activePreset.PresetName;
                        ConfigWindowHelpers.activePreset.PresetName = ConfigWindowHelpers.renameBuffer;

                        var jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };
                        var options = jsonSerializerOptions;
                        var jsonString = JsonSerializer.Serialize(ConfigWindowHelpers.activePreset, options);
                        File.WriteAllText(Potatoes.PresetDirectory + "/" + ConfigWindowHelpers.renameBuffer + ".json", jsonString);
                        File.Delete(Potatoes.PresetDirectory + "/" + prevName + ".json");
                        ConfigWindowHelpers.
                                                renamingWindowOpen = false;

                    }
                }
                ImGui.EndChild();
            }
            ImGui.End();
        }
    }

    public static void OpenNewMemberWindow()
    {
        if (ConfigWindowHelpers.newMemberWindowOpen)
        {

            if (ImGui.Begin("Member's Character Name", ref ConfigWindowHelpers.newMemberWindowOpen))
            {
                if (ImGui.BeginChild("##", new Vector2(-1, -1), true))
                {
                    ImGui.Text("Character Name");
                    ImGui.SameLine();
                    ImGui.InputText("##", ref ConfigWindowHelpers.nameBuffer, (uint)ConfigWindow.BufferSize);
                    ImGui.SameLine();
                    if (ConfigWindowHelpers.IconButtonWithText(FontAwesomeIcon.Save, "", "Save"))
                    {
                        var preset = new Preset
                        {
                            PresetName = ConfigWindowHelpers.nameBuffer,
                        };

                        var jsonString = JsonSerializer.Serialize(preset);
                        File.WriteAllText(Potatoes.PresetDirectory + "/" + ConfigWindowHelpers.nameBuffer + ".json", jsonString);
                        ConfigWindowHelpers.
                                                newMemberWindowOpen = false;

                    }
                }
                ImGui.EndChild();
            }
            ImGui.End();
        }
    }
}
