using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Textures;
using Dalamud.Utility;
using SimonSays.Helpers;
using SimonSays.ImGuiMethods;

namespace SimonSays.Windows;

internal static class ConfigWindowHelpers
{
    public static bool isFirstFrame = true;
    public static System.Numerics.Vector2 contextMenuPosition = new();
    public static string selectedLayout = string.Empty;
    private static string SelectedMember = string.Empty;
    public static bool namingWindowOpen = false;
    public static bool renamingWindowOpen = false;
    public static bool newMemberWindowOpen = false;
    public static bool contextPopupOpen = false;
    public static string nameBuffer = "Change Me";
    public static string filterText = string.Empty;
    public static string renameBuffer = string.Empty;
    public static Preset? activePreset = null;

    public static readonly List<System.Numerics.Vector4> memberColors =
    [
        new System.Numerics.Vector4(0.722f, 0.325f, 0.623f, 1.0f),
        new System.Numerics.Vector4(0.7051666666666666f, 0.335625f, 0.605875f, 1.0f),
        new System.Numerics.Vector4(0.6883333333333332f, 0.34625f, 0.5887499999999999f, 1.0f),
        new System.Numerics.Vector4(0.6715f, 0.356875f, 0.5716249999999999f, 1.0f),
        new System.Numerics.Vector4(0.6546666666666667f, 0.36750000000000005f, 0.5545f, 1.0f),
        new System.Numerics.Vector4(0.6378333333333334f, 0.37812500000000004f, 0.537375f, 1.0f),
        new System.Numerics.Vector4(0.621f, 0.38875f, 0.52025f, 1.0f),
        new System.Numerics.Vector4(0.6041666666666666f, 0.399375f, 0.503125f, 1.0f),
        new System.Numerics.Vector4(0.5873333333333334f, 0.41000000000000003f, 0.48600000000000004f, 1.0f),
        new System.Numerics.Vector4(0.5705f, 0.42062499999999997f, 0.46887500000000004f, 1.0f),
        new System.Numerics.Vector4(0.5536666666666665f, 0.43125f, 0.45174999999999993f, 1.0f),
        new System.Numerics.Vector4(0.5368333333333333f, 0.44187499999999996f, 0.434625f, 1.0f),
        new System.Numerics.Vector4(0.52f, 0.4525f, 0.4175f, 1.0f),
        new System.Numerics.Vector4(0.5031666666666667f, 0.463125f, 0.40037500000000004f, 1.0f),
        new System.Numerics.Vector4(0.4863333333333334f, 0.47374999999999995f, 0.38325000000000004f, 1.0f),
        new System.Numerics.Vector4(0.46950000000000003f, 0.484375f, 0.36612500000000003f, 1.0f),
        new System.Numerics.Vector4(0.45266666666666668f, 0.49499999999999994f, 0.3490000000000001f, 1.0f),
        new System.Numerics.Vector4(0.43583333333333334f, 0.505625f, 0.33187500000000003f, 1.0f),
        new System.Numerics.Vector4(0.4190000000000001f, 0.5162499999999999f, 0.3147500000000001f, 1.0f),
        new System.Numerics.Vector4(0.4021666666666668f, 0.526875f, 0.29762500000000014f, 1.0f),
        new System.Numerics.Vector4(0.3853333333333334f, 0.5374999999999999f, 0.28050000000000014f, 1.0f),
        new System.Numerics.Vector4(0.3685000000000001f, 0.5481249999999999f, 0.26337500000000014f, 1.0f),
        new System.Numerics.Vector4(0.3516666666666668f, 0.5587499999999999f, 0.24625000000000014f, 1.0f),
        new System.Numerics.Vector4(0.3348333333333335f, 0.5693749999999999f, 0.22912500000000016f, 1.0f),
    ];

    public static string testText = "Simon Says : hum";
    public static ISharedImmediateTexture? aboutImage;
    public static ImGuiStylePtr StylePtr = ImGui.GetStyle();

    public static string GetSelectedMember()
    {
        return SelectedMember;
    }

    public static void SetSelectedMember(string value)
    {
        SelectedMember = value;
    }

    /// <summary>
    /// Draws an icon button with optional text and a tooltip.
    /// </summary>
    /// <param name="icon">The FontAwesome icon to draw.</param>
    /// <param name="text">Optional text drawn next to the icon.</param>
    /// <param name="tooltip">The tooltip shown on hover.</param>
    /// <returns>True if either the icon or the text button was clicked.</returns>
    public static bool IconButtonWithText(FontAwesomeIcon icon, string text, string tooltip)
    {
        ImGui.PushID(text);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new System.Numerics.Vector2(0.0f));
        var selected = ImGuiComponents.IconButton(icon);
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
        var key = SaltAndPepper.ChatTypes.Keys.ToList()[index];
        var value = SaltAndPepper.ChatTypes[key];
        var channel = key;

        Potatoes.Configuration!.EnabledChannels.TryGetValue(channel, out var prevVal);

        var newVal = prevVal;
        ImGui.TableNextColumn();
        if (ImGui.Checkbox(value, ref newVal))
        {
            Potatoes.Configuration!.EnabledChannels[channel] = newVal;
            Potatoes.Configuration!.Save();
        }
    }

    /// <summary>
    /// Draws the "Enabled" checkbox and updates the IsListening configuration value.
    /// </summary>
    public static void DrawEnabled()
    {
        var isListening = Potatoes.Configuration!.IsListening;

        if (ImGui.Checkbox("Enabled", ref isListening))
        {
            Potatoes.Configuration!.IsListening = isListening;
            Potatoes.Configuration!.Save();
        }
    }

    /// <summary>
    /// Draws checkboxes for each channel in a table format.
    /// </summary>
    public static void DrawCheckboxes()
    {
        ImGui.BeginTable("simonsaystable", 3, ImGuiTableFlags.Borders);

        ImGui.TableSetupColumn("Channels");
        ImGui.TableSetupColumn(string.Empty);
        ImGui.TableSetupColumn(string.Empty);
        ImGui.TableHeadersRow();

        var chatTypes = SaltAndPepper.ChatTypes;

        var max = (int)Math.Ceiling(chatTypes.Count / 3d);

        for (var i = 0; i < Math.Ceiling(chatTypes.Count / 3d); i++)
        {
            for (var j = 0; j < 3; j++)
            {
                if ((j * max) + i > chatTypes.Count - 1)
                {
                    continue;
                }

                var index = (j * max) + i;
                DrawCheckbox(index);
            }
        }

        ImGui.EndTable();
    }

    /// <summary>
    /// Draws a text box for the catch phrase and saves it when modified.
    /// </summary>
    public static void DrawCatchPhBox()
    {
        var inputText = Potatoes.Configuration!.CatchPhrase;
        ImGui.SetNextItemWidth(250f);
        if (ImGui.InputText(string.Empty, ref inputText, 500))
        {
            Potatoes.Configuration!.CatchPhrase = inputText.Trim();
            Potatoes.Configuration!.Save();
        }
    }

    /// <summary>
    /// Draws the positional sync override buttons.
    /// </summary>
    public static void DrawExperimentCheckboxes()
    {
        ImGui.TextColored(new System.Numerics.Vector4(160, 160, 160, 0.8f), "Position Sync requires a Target.");

        ImGuiEx.ImGuiLineRightAlign("Settings_Override_Position_Sync", () =>
        {
            if (Potatoes.Configuration!.PosSync)
            {
                if (ImGui.Button("Sync Position"))
                {
                    Meat.StartScooch();
                }

                ImGui.SameLine();
                ImGui.Text("  ");
            }
        });

        ImGui.Dummy(new System.Numerics.Vector2(0, 5));

        ImGuiEx.ImGuiLineRightAlign("Settings_Override_Positon_Stop", () =>
        {
            if (Potatoes.Configuration!.PosSync)
            {
                if (ImGui.Button("Stop Sync"))
                {
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
        if (Meat.movement == null)
        {
            return;
        }

        ImGui.Text($"SoftDisable: {(Meat.movement.SoftDisable ? "True" : "False")}");
        ImGui.Text($"Distance to target: {MathF.Sqrt(Meat.movement.DistanceSquared)}");
        ImGui.Text($"Rotation distance (deg): {Meat.movement.RotationDistance}");

        var currTime = DateTime.Now;

        ImGui.Text($"Time since last moved: {(currTime - Meat.movement.LastTimeMoved).TotalSeconds}");
        ImGui.Text($"Time since last turned: {(currTime - Meat.movement.LastTimeTurned).TotalSeconds}");

        ImGui.Spacing();

        var tarPos = Meat.movement.DesiredPosition;
        var tarRot = AngleConversions.ToDeg(Meat.movement.DesiredRotation);

        ImGui.Text("Target Destination:");
        ImGui.Text($"X: {tarPos.X} Y: {tarPos.Y} Z: {tarPos.Z}");
        ImGui.Text($"Angle: {tarRot}");

        ImGui.Spacing();

        ImGui.Text($"Last Forward: {Meat.movement.LastForward}");
        ImGui.Text($"Last Left: {Meat.movement.LastLeft}");
        ImGui.Text($"Last Turn Left: {Meat.movement.LastTurnLeft}");
    }

    /// <summary>
    /// Pushes the custom SimonSays window styles. Must be balanced by the pops in PostDraw.
    /// </summary>
    public static void PushStyles()
    {
        // Variables
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new System.Numerics.Vector2(10, 10));
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new System.Numerics.Vector2(4, 4));
        ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new System.Numerics.Vector2(4, 4));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new System.Numerics.Vector2(4, 4));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemInnerSpacing, new System.Numerics.Vector2(4, 4));
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
        ImGui.PushStyleVar(ImGuiStyleVar.WindowTitleAlign, new System.Numerics.Vector2(0.0f, 0.50f));
        ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new System.Numerics.Vector2(0.5f, 0.5f));
        ImGui.PushStyleVar(ImGuiStyleVar.SelectableTextAlign, new System.Numerics.Vector2(0, 0));

        // Colors
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, new System.Numerics.Vector4(081, 054, 148, 211) / 255f);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new System.Numerics.Vector4(130, 068, 153, 241) / 255f);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new System.Numerics.Vector4(184, 083, 159, 241) / 255f);
        ImGui.PushStyleColor(ImGuiCol.Separator, new System.Numerics.Vector4(130, 068, 153, 241) / 255f);
        ImGui.PushStyleColor(ImGuiCol.SeparatorHovered, new System.Numerics.Vector4(130, 068, 153, 200) / 255f);
        ImGui.PushStyleColor(ImGuiCol.SeparatorActive, new System.Numerics.Vector4(184, 083, 159, 241) / 255f);
        ImGui.PushStyleColor(ImGuiCol.ResizeGripActive, new System.Numerics.Vector4(184, 083, 159, 241) / 255f);
        ImGui.PushStyleColor(ImGuiCol.TabHovered, new System.Numerics.Vector4(130, 068, 153, 200) / 255f);
        ImGui.PushStyleColor(ImGuiCol.TabActive, new System.Numerics.Vector4(184, 083, 159, 241) / 255f);
        ImGui.PushStyleColor(ImGuiCol.Border, new System.Numerics.Vector4(0.35f, 0.35f, 0.35f, 0.75f));
        ImGui.PushStyleColor(ImGuiCol.FrameBg, new System.Numerics.Vector4(50, 46, 51, 122) / 255f);
    }

    /// <summary>
    /// Draws the preset context popup menu.
    /// </summary>
    public static void ContextPopup()
    {
        var contextMenuSize = new System.Numerics.Vector2(135, 34);
        if (contextPopupOpen)
        {
            if (isFirstFrame)
            {
                contextMenuPosition = ImGui.GetMousePos();
                isFirstFrame = false;
            }

            ImGui.SetNextWindowSize(contextMenuSize);
            ImGui.SetNextWindowPos(contextMenuPosition);

            if (ImGui.Begin("Contextual Menu", ref contextPopupOpen, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar))
            {
                if (ImGui.Selectable("Rename Preset"))
                {
                    ImGui.SetNextWindowPos(new System.Numerics.Vector2(10.0f, 100.0f));
                    ImGui.SetNextWindowSize(new System.Numerics.Vector2(200.0f, 60.0f));
                    renamingWindowOpen = true;
                    contextPopupOpen = false;
                    renameBuffer = activePreset?.PresetName ?? string.Empty;
                }
            }

            ImGui.End();

            if (!ImGui.IsItemHovered() && ImGui.IsMouseReleased(ImGuiMouseButton.Left))
            {
                contextPopupOpen = false;
            }

            if (!contextPopupOpen)
            {
                isFirstFrame = true;
            }
        }
    }

    /// <summary>
    /// Draws the preset naming window.
    /// </summary>
    public static void OpenNamingWindow()
    {
        if (!namingWindowOpen)
        {
            return;
        }

        ImGui.SetNextWindowPos(new System.Numerics.Vector2((ImGui.GetMainViewport().Size.X / 2f) - 250, (ImGui.GetMainViewport().Size.Y / 2) - 50));
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(500, 100), ImGuiCond.Appearing);
        if (ImGui.Begin("Preset Name", ref namingWindowOpen))
        {
            if (ImGui.BeginChild("##", new System.Numerics.Vector2(-1, -1), true))
            {
                ImGui.Text("Name");
                ImGui.SameLine();
                ImGui.InputText("##", ref nameBuffer, ConfigWindow.BufferSize);
                ImGui.SameLine();
                if (IconButtonWithText(FontAwesomeIcon.Save, string.Empty, "Save"))
                {
                    var preset = new Preset
                    {
                        PresetName = nameBuffer,
                    };

                    var jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };
                    var jsonString = JsonSerializer.Serialize(preset, jsonSerializerOptions);
                    File.WriteAllText(Path.Combine(Potatoes.PresetDirectory, nameBuffer + ".json"), jsonString);
                    namingWindowOpen = false;
                }
            }

            ImGui.EndChild();
        }

        ImGui.End();
    }

    /// <summary>
    /// Draws the preset renaming window.
    /// </summary>
    public static void OpenReNamingWindow()
    {
        if (!renamingWindowOpen)
        {
            return;
        }

        if (ImGui.Begin("Rename a preset", ref renamingWindowOpen))
        {
            if (ImGui.BeginChild("##", new System.Numerics.Vector2(-1, -1), true))
            {
                ImGui.Text("Name");
                ImGui.SameLine();
                ImGui.InputText("##", ref renameBuffer, ConfigWindow.BufferSize);
                ImGui.SameLine();
                if (IconButtonWithText(FontAwesomeIcon.Save, string.Empty, "Save"))
                {
                    if (activePreset == null)
                    {
                        return;
                    }

                    var prevName = activePreset.PresetName;
                    activePreset.PresetName = renameBuffer;

                    var jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };
                    var jsonString = JsonSerializer.Serialize(activePreset, jsonSerializerOptions);
                    File.WriteAllText(Path.Combine(Potatoes.PresetDirectory, renameBuffer + ".json"), jsonString);
                    File.Delete(Path.Combine(Potatoes.PresetDirectory, prevName + ".json"));
                    renamingWindowOpen = false;
                }
            }

            ImGui.EndChild();
        }

        ImGui.End();
    }
}
