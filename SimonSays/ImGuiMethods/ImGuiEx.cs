using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface;
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

        public static void ImGuiLineRightAlign(string id, Action func)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));
            // Check if the width of the line has been stored in the dictionary
            if (CenteredLineWidths.TryGetValue(id, out var dims))
            {
                // Set the cursor position to right the line
                ImGui.SetCursorPosX(ImGui.GetWindowWidth() - dims);
            }
            var oldCur = ImGui.GetCursorPosX();
            func();
            ImGui.SameLine(0, 0);
            // Calculate the width of the line and store it in the dictionary
            CenteredLineWidths[id] = ImGui.GetCursorPosX() - oldCur;
            ImGui.Dummy(Vector2.Zero);
            ImGui.PopStyleVar();
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

        // Method to draw multiple empty lines
        public static void Spacer(int i)
        {
            float l = ImGui.GetTextLineHeight();
            if (i > 0)
            {
                ImGui.Dummy(new Vector2(0, i * l));
            }
        }

        public static bool DrawToggleButtonWithTooltip(string buttonId, string tooltip, FontAwesomeIcon icon, ref bool enabledState)
        {
            bool result = false;
            bool buttonEnabled = enabledState;
            if (buttonEnabled)
            {
                ImGuiCol imGuiCol = ImGuiCol.Button;
                Vector4 healerGreen = ImGuiColors.HealerGreen;
                healerGreen.W = 0.25f;
                ImGui.PushStyleColor(imGuiCol, healerGreen);
            }
            if (ImGuiComponents.IconButton(buttonId, icon))
            {
                result = true;
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(tooltip);
            }
            if (buttonEnabled)
            {
                ImGui.PopStyleColor();
            }
            return result;
        }

        public static bool ColoredIconButtonWithText(FontAwesomeIcon icon, Vector4 color, string text)
        {
            ImGui.PushID(text);
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0.0f));
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(color.X, color.Y, color.Z, color.W));
            bool selected = ImGuiComponents.IconButton(icon);
            ImGui.PopStyleColor();
            ImGui.SameLine();
            selected |= ImGui.Button(text);
            ImGui.PopStyleVar();
            ImGui.PopID();

            return selected;
        }

        

        public static void ColoredIconWithText(FontAwesomeIcon icon, Vector4 color, string text)
        {
            ImGui.PushFont(UiBuilder.IconFont);
            ImGui.TextColored(color, icon.ToIconString());
            ImGui.PopFont();
            ImGui.SameLine();
            ImGui.Text(text);
        }

       
    }
}
