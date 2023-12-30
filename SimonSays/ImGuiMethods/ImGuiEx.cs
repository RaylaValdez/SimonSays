using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PunishLib.ImGuiMethods
{
    internal static class ImGuiEx
    {
        // Dictionary to store the widths of centered lines
        static readonly Dictionary<string, float> CenteredLineWidths = new();

        // Method to draw a centered line in ImGui
        public static void ImGuiLineCentered(string id, Action func)
        {
            // Check if the width of the line has been stored in the dictionary
            if (CenteredLineWidths.TryGetValue(id, out var dims))
            {
                // Set the cursor position to center the line
                ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X / 2 - dims / 2);
            }
            var oldCur = ImGui.GetCursorPosX();
            func();
            ImGui.SameLine(0, 0);
            // Calculate the width of the line and store it in the dictionary
            CenteredLineWidths[id] = ImGui.GetCursorPosX() - oldCur;
            ImGui.Dummy(Vector2.Zero);
        }

        // Method to draw text in ImGui
        public static void Text(string s)
        {
            ImGui.TextUnformatted(s);
        }

        // Method to draw colored text in ImGui
        public static void Text(Vector4 col, string s)
        {
            // Push the text color to the ImGui style stack
            ImGui.PushStyleColor(ImGuiCol.Text, col);
            ImGui.TextUnformatted(s);
            // Pop the text color from the ImGui style stack
            ImGui.PopStyleColor();
        }
    }
}
