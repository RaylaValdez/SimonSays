using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PunishLib.ImGuiMethods
{
    public static partial class ImGuiGroup
    {
        public record GroupBoxOptions
        {
            // Set the default value of the Collapsible property to false
            public bool Collapsible { get; init; } = false;

            // Set the default value of the HeaderTextColor property to the color of ImGuiCol.Text
            public uint HeaderTextColor { get; init; } = ImGui.GetColorU32(ImGuiCol.Text);

            // Set the default value of the HeaderTextAction property to null
            public Action? HeaderTextAction { get; init; } = null;

            // Set the default value of the BorderColor property to the color of ImGuiCol.Border
            public uint BorderColor { get; init; } = ImGui.GetColorU32(ImGuiCol.Border);

            // Set the default value of the BorderPadding property to the value of ImGuiStyle.WindowPadding
            public Vector2 BorderPadding { get; init; } = ImGui.GetStyle().WindowPadding;

            // Set the default value of the BorderRounding property to the value of ImGuiStyle.FrameRounding
            public float BorderRounding { get; init; } = ImGui.GetStyle().FrameRounding;

            // Set the default value of the DrawFlags property to ImDrawFlags.None
            public ImDrawFlags DrawFlags { get; init; } = ImDrawFlags.None;

            // Set the default value of the BorderThickness property to 2f
            public float BorderThickness { get; init; } = 2f;

            // Declare the Width property
            public float Width { get; set; }

            // Declare the MaxX property
            public float MaxX { get; set; }
        }

        private static readonly Stack<GroupBoxOptions> GroupBoxOptionsStack = new();

        /// <summary>
        /// Begins a group box with the specified ID, minimum window percent, and options.
        /// </summary>
        /// <param name="id">The ID of the group box.</param>
        /// <param name="minimumWindowPercent">The minimum window percent for the group box.</param>
        /// <param name="options">The options for the group box.</param>
        /// <returns>A boolean indicating whether the group box was successfully begun.</returns>
        public static bool BeginGroupBox(string? id = null, float minimumWindowPercent = 1.0f, GroupBoxOptions? options = null)
        {
            // Create a new instance of GroupBoxOptions if options is null
            options ??= new GroupBoxOptions();

            // Begin a new group
            ImGui.BeginGroup();

            // Set the default value of open to true
            var open = true;

            // Check if the id is not empty or null
            if (!string.IsNullOrEmpty(id))
            {
                // Check if the group box is not collapsible
                if (!options.Collapsible)
                {
                    // Display the id text with the specified header text color
                    ImGui.TextColored(ImGui.ColorConvertU32ToFloat4(options.HeaderTextColor), id);
                }
                else
                {
                    // Push the header text color to the ImGui style color stack
                    ImGui.PushStyleColor(ImGuiCol.Text, options.HeaderTextColor);

                    // Create a collapsible tree node with the specified id and flags
                    // Set the value of open based on whether the tree node is open or closed
                    open = ImGui.TreeNodeEx(id, ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.DefaultOpen);

                    // Pop the header text color from the ImGui style color stack
                    ImGui.PopStyleColor();
                }

                // Invoke the header text action if it is not null
                options.HeaderTextAction?.Invoke();

                // This prevents rounding issues caused by ImGui flooring the cursor position after items
                // Increase the indentation level
                ImGui.Indent();

                // Decrease the indentation level
                ImGui.Unindent();
            }

            // Get the current ImGui style
            var style = ImGui.GetStyle();

            // Calculate the spacing between items in the group box
            var spacing = style.ItemSpacing.X * (1 - minimumWindowPercent);

            // Calculate the width of the content region within the group box
            var contentRegionWidth = GroupBoxOptionsStack.TryPeek(out var parent) ? parent.Width - parent.BorderPadding.X * 2 : ImGui.GetWindowContentRegionMax().X - style.WindowPadding.X;

            // Calculate the desired width of the group box
            var width = Math.Max(contentRegionWidth * minimumWindowPercent - spacing, 1);

            // Set the width of the options within the group box
            options.Width = minimumWindowPercent > 0 ? width : 0;

            // Begin a new group within the ImGui window
            ImGui.BeginGroup();

            // Set the item spacing to zero within the group
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, Vector2.Zero);

            // Create a dummy item with the desired width
            ImGui.Dummy(options.BorderPadding with { X = width });

            // Restore the original item spacing
            ImGui.PopStyleVar();

            // Get the maximum position of the current item
            var max = ImGui.GetItemRectMax();

            // Set the maximum X position of the options
            options.MaxX = max.X;

            // If the width of the options is greater than zero, set a clipping rectangle
            if (options.Width > 0)
                ImGui.PushClipRect(ImGui.GetItemRectMin(), max with { Y = 10000 }, true);

            // Indent the options by the border padding
            ImGui.Indent(Math.Max(options.BorderPadding.X, 0.01f));

            // Set the item width to a percentage of the available width
            ImGui.PushItemWidth(MathF.Floor((width - options.BorderPadding.X * 2) * 0.65f));

            // Push the options onto the stack
            GroupBoxOptionsStack.Push(options);

            // If the group box is open, return true
            if (open) return true;

            // Display a disabled text indicating that there are no options
            ImGui.TextDisabled(". . .");

            // End the group box
            EndGroupBox();

            // Return false to indicate that the group box is closed
            return false;
        }

        /// <summary>
        /// Begins a group box with the specified text and options.
        /// </summary>
        /// <param name="text">The text to display in the group box.</param>
        /// <param name="options">The options for the group box.</param>
        /// <returns>True if the group box was successfully started; otherwise, false.</returns>
        public static bool BeginGroupBox(string text, GroupBoxOptions options) => BeginGroupBox(text, 1.0f, options);

        /// <summary>
        /// Begins a group box with the specified border color and minimum window percent.
        /// </summary>
        /// <param name="borderColor">The color of the group box border.</param>
        /// <param name="minimumWindowPercent">The minimum window percent.</param>
        /// <returns>True if the group box was successfully begun; otherwise, false.</returns>
        public static bool BeginGroupBox(uint borderColor, float minimumWindowPercent = 1.0f) => BeginGroupBox(null, minimumWindowPercent, new GroupBoxOptions { BorderColor = borderColor });

        public static unsafe void EndGroupBox()
        {
            // Pop the options from the groupBoxOptionsStack
            var options = GroupBoxOptionsStack.Pop();

            // Check if autoAdjust is true if the width of options is less than or equal to 0
            var autoAdjust = options.Width <= 0;

            // Pop the item width from ImGui
            ImGui.PopItemWidth();

            // Unindent the current indentation level by the maximum value between options.BorderPadding.X and 0.01f
            ImGui.Unindent(Math.Max(options.BorderPadding.X, 0.01f));

            // If autoAdjust is false, pop the current clipping rectangle
            if (!autoAdjust)
                ImGui.PopClipRect();

            // Set the cursor position on the Y-axis to the current cursor position minus the item spacing on the Y-axis
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() - ImGui.GetStyle().ItemSpacing.Y);

            // Create a dummy element with border padding X set to 0
            ImGui.Dummy(options.BorderPadding with { X = 0 });

            // If autoAdjust is false, update the maximum cursor position on the X-axis to options.MaxX
            if (!autoAdjust)
            {
                var window = GetCurrentWindow();
                window->CursorMaxPos = window->CursorMaxPos with { X = options.MaxX };
            }

            // End the current group
            ImGui.EndGroup();

            // Get the minimum and maximum coordinates of the current item's rectangle
            var min = ImGui.GetItemRectMin();
            var max = autoAdjust ? ImGui.GetItemRectMax() : ImGui.GetItemRectMax() with { X = options.MaxX };

            // Rect with text corner missing
            /*ImGui.PushClipRect(min with { Y = min.Y + options.BorderRounding }, max, true);
            ImGui.GetWindowDrawList().AddRect(min, max, options.BorderColor, options.BorderRounding, options.DrawFlags, options.BorderThickness);
            ImGui.PopClipRect();
            ImGui.PushClipRect(min with { X = (min.X + max.X) / 2 }, max with { Y = min.Y + options.BorderRounding }, true);
            ImGui.GetWindowDrawList().AddRect(min, max, options.BorderColor, options.BorderRounding, options.DrawFlags, options.BorderThickness);
            ImGui.PopClipRect();*/

            // [ ] Brackets
            /*ImGui.PushClipRect(min, max with { X = (min.X * 2 + max.X) / 3 }, true);
            ImGui.GetWindowDrawList().AddRect(min, max, options.BorderColor, options.BorderRounding, options.DrawFlags, options.BorderThickness);
            ImGui.PopClipRect();
            ImGui.PushClipRect(min with { X = (min.X + max.X * 2) / 3 }, max, true);
            ImGui.GetWindowDrawList().AddRect(min, max, options.BorderColor, options.BorderRounding, options.DrawFlags, options.BorderThickness);
            ImGui.PopClipRect();*/

            // Horizontal brackets
            /*ImGui.PushClipRect(min, max with { Y = (min.Y * 2 + max.Y) / 3 }, true);
            ImGui.GetWindowDrawList().AddRect(min, max, options.BorderColor, options.BorderRounding, options.DrawFlags, options.BorderThickness);
            ImGui.PopClipRect();
            ImGui.PushClipRect(min with { Y = (min.Y + max.Y * 2) / 3 }, max, true);
            ImGui.GetWindowDrawList().AddRect(min, max, options.BorderColor, options.BorderRounding, options.DrawFlags, options.BorderThickness);
            ImGui.PopClipRect();*/

            ImGui.GetWindowDrawList().AddRect(min, max, options.BorderColor, options.BorderRounding, options.DrawFlags, options.BorderThickness);

            ImGui.EndGroup();
        }

        [LibraryImport("cimgui")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        private static partial nint igGetCurrentWindow();
        public static unsafe ImGuiWindow* GetCurrentWindow() => (ImGuiWindow*)igGetCurrentWindow();
        public static unsafe ImGuiWindowFlags GetCurrentWindowFlags() => GetCurrentWindow()->Flags;
        public static unsafe bool CurrentWindowHasCloseButton() => GetCurrentWindow()->HasCloseButton != 0;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct ImGuiWindow
    {
        [FieldOffset(0xC)] public ImGuiWindowFlags Flags;

        [FieldOffset(0xD5)] public byte HasCloseButton;

        // 0x118 is the start of ImGuiWindowTempData
        [FieldOffset(0x130)] public Vector2 CursorMaxPos;
    }
}
