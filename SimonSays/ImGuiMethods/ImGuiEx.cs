using System;
using System.Collections.Generic;
using Dalamud.Bindings.ImGui;
using System.Numerics;

namespace SimonSays.ImGuiMethods;

/// <summary>
/// Small ImGui layout helpers. Originally derived from PunishLib (see PunishLib LICENSE).
/// </summary>
internal static class ImGuiEx
{
    private static readonly Dictionary<string, float> LineWidths = [];

    /// <summary>
    /// Draws the given content centered on the current line.
    /// </summary>
    /// <param name="id">A stable identifier used to remember the measured width.</param>
    /// <param name="func">The draw action to invoke.</param>
    public static void ImGuiLineCentered(string id, Action func)
    {
        if (LineWidths.TryGetValue(id, out var dims))
        {
            ImGui.SetCursorPosX((ImGui.GetContentRegionAvail().X / 2) - (dims / 2));
        }

        var oldCur = ImGui.GetCursorPosX();
        func();
        ImGui.SameLine(0, 0);
        LineWidths[id] = ImGui.GetCursorPosX() - oldCur;
        ImGui.Dummy(Vector2.Zero);
    }

    /// <summary>
    /// Draws the given content right-aligned on the current line.
    /// </summary>
    /// <param name="id">A stable identifier used to remember the measured width.</param>
    /// <param name="func">The draw action to invoke.</param>
    public static void ImGuiLineRightAlign(string id, Action func)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));
        if (LineWidths.TryGetValue(id, out var dims))
        {
            ImGui.SetCursorPosX(ImGui.GetWindowWidth() - dims);
        }

        var oldCur = ImGui.GetCursorPosX();
        func();
        ImGui.SameLine(0, 0);
        LineWidths[id] = ImGui.GetCursorPosX() - oldCur;
        ImGui.Dummy(Vector2.Zero);
        ImGui.PopStyleVar();
    }
}
