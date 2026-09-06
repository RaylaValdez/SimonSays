using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using Dalamud.Bindings.ImGui;
using Dalamud.Bindings.ImPlot;
using Dalamud.Game.Text;
using Dalamud.Interface;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using SimonSays.Helpers;
using SimonSays.ImGuiMethods;

namespace SimonSays.Windows.Tabs;

internal class PositionalPresets
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    public static void Draw()
    {
        var windowWidth = ImGui.GetWindowWidth();
        var windowHeight = ImGui.GetWindowHeight();

        var leftSize = new Vector2(windowWidth / 4.3f, windowHeight / 1.2f);
        var layoutsChildSize = new Vector2(-1, leftSize.Y / 2.5f);
        var layoutsSize = new Vector2(-1, -1);
        var membersSize = new Vector2(-1, -1);
        var middleSize = new Vector2(windowWidth / 1.95f, windowHeight / 1.2f);
        var rightSize = new Vector2(windowWidth / 4.3f, windowHeight / 1.2f);

        ImGui.NewLine();
        if (ImGui.BeginChild("left", leftSize, true, ImGuiWindowFlags.NoCollapse))
        {
            if (ImGui.BeginChild("layouts", layoutsChildSize, true, ImGuiWindowFlags.NoCollapse))
            {
                if (ImGui.BeginChild("buttons", new Vector2(-1, ImGui.GetTextLineHeight() * 2.3f), false, ImGuiWindowFlags.NoCollapse))
                {
                    ImGui.Text("Presets");
                    ImGui.SameLine();
                    ImGuiEx.ImGuiLineRightAlign("LayoutButtons", () =>
                    {
                        if (ConfigWindowHelpers.IconButtonWithText(FontAwesomeIcon.Plus, string.Empty, "Create new Preset"))
                        {
                            ImGui.SetNextWindowPos(new Vector2(10.0f, 100.0f));
                            ImGui.SetNextWindowSize(new Vector2(200.0f, 60.0f));
                            ConfigWindowHelpers.namingWindowOpen = true;
                        }

                        ImGui.SameLine();
                        ImGui.Dummy(new Vector2(1, 0));
                        ImGui.SameLine();
                        if (ConfigWindowHelpers.IconButtonWithText(FontAwesomeIcon.Minus, string.Empty, "Delete selected Preset"))
                        {
                            File.Delete(Path.Combine(Potatoes.PresetDirectory, ConfigWindowHelpers.selectedLayout + ".json"));
                            ConfigWindowHelpers.selectedLayout = string.Empty;
                            ConfigWindowHelpers.activePreset = null;
                        }

                        ImGui.SameLine();
                        ImGui.Dummy(new Vector2(1, 0));
                        ImGui.SameLine();
                        if (ConfigWindowHelpers.IconButtonWithText(FontAwesomeIcon.Clipboard, string.Empty, "Copy Preset to Clipboard"))
                        {
                            if (!string.IsNullOrEmpty(ConfigWindowHelpers.selectedLayout))
                            {
                                var presetContent = File.ReadAllLines(Path.Combine(Potatoes.PresetDirectory, ConfigWindowHelpers.selectedLayout + ".json"));
                                var contentToCopy = string.Join(Environment.NewLine, presetContent);

                                ImGui.SetClipboardText(contentToCopy);

                                Sausages.NotificationManager.AddNotification(new Notification
                                {
                                    Content = "Preset copied to Clipboard.",
                                });
                            }
                            else
                            {
                                Sausages.NotificationManager.AddNotification(new Notification
                                {
                                    Content = "Select a preset to copy to Clipboard.",
                                });
                            }
                        }

                        ImGui.SameLine();
                        ImGui.Dummy(new Vector2(1, 0));
                        ImGui.SameLine();
                        if (ConfigWindowHelpers.IconButtonWithText(FontAwesomeIcon.FileImport, string.Empty, "Import Preset from Clipboard"))
                        {
                            ImportPresetFromClipboard();
                        }
                    });
                }

                ImGui.EndChild();

                using var layouts = ImRaii.ListBox("##", layoutsSize);
                if (layouts.Success)
                {
                    foreach (var file in Directory.EnumerateFiles(Potatoes.PresetDirectory))
                    {
                        var fileName = Path.GetFileNameWithoutExtension(file);
                        if (ImGui.Selectable(fileName, ConfigWindowHelpers.selectedLayout == fileName))
                        {
                            ConfigWindowHelpers.selectedLayout = fileName;

                            var jsonString = File.ReadAllText(file);
                            ConfigWindowHelpers.activePreset = JsonSerializer.Deserialize<Preset>(jsonString) ?? new Preset();
                            ConfigWindowHelpers.activePreset.Members ??= [];
                        }

                        if (ConfigWindowHelpers.selectedLayout == fileName)
                        {
                            if (ImGui.IsItemHovered())
                            {
                                if (ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                                {
                                    Sausages.Log.Debug("Opening context popup");
                                    ConfigWindowHelpers.contextPopupOpen = true;
                                }
                            }
                        }
                    }
                }
            }

            ImGui.EndChild();
            if (ImGui.BeginChild("members", membersSize, true, ImGuiWindowFlags.NoCollapse))
            {
                if (!ConfigWindowHelpers.selectedLayout.IsNullOrEmpty())
                {
                    if (ImGui.BeginChild("memberbuttons", new Vector2(-1, ImGui.GetTextLineHeight() * 2.3f), false, ImGuiWindowFlags.NoCollapse))
                    {
                        ImGui.Text("Members");
                        ImGui.SameLine();
                        ImGuiEx.ImGuiLineRightAlign("MembersButtons", () =>
                        {
                            if (ConfigWindowHelpers.IconButtonWithText(FontAwesomeIcon.StreetView, string.Empty, "Add targetted Character."))
                            {
                                var target = Sausages.TargetManager.Target;
                                if (target != null && ConfigWindowHelpers.activePreset != null)
                                {
                                    ConfigWindowHelpers.activePreset.Members!.Add(new PresetMember(target.Name.TextValue));
                                    SaveActivePreset();
                                }
                            }

                            ImGui.SameLine();
                            ImGui.Dummy(new Vector2(1, 0));
                            ImGui.SameLine();

                            if (ConfigWindowHelpers.IconButtonWithText(FontAwesomeIcon.Minus, string.Empty, "Remove Selected Member"))
                            {
                                if (ConfigWindowHelpers.activePreset != null)
                                {
                                    ConfigWindowHelpers.activePreset.Members = ConfigWindowHelpers.activePreset.Members
                                        .Where((member) => member.CharacterName != ConfigWindowHelpers.GetSelectedMember())
                                        .ToList();
                                    SaveActivePreset();
                                }
                            }
                        });
                    }

                    ImGui.EndChild();

                    using var members = ImRaii.ListBox("##", membersSize);
                    if (members.Success)
                    {
                        if (ConfigWindowHelpers.activePreset?.Members != null)
                        {
                            foreach (var member in ConfigWindowHelpers.activePreset.Members.ToList())
                            {
                                if (ImGui.Selectable(member.CharacterName, ConfigWindowHelpers.GetSelectedMember() == member.CharacterName))
                                {
                                    ConfigWindowHelpers.SetSelectedMember(member.CharacterName);
                                }
                            }
                        }
                    }
                }
            }

            ImGui.EndChild();
        }

        ImGui.EndChild();
        ImGui.SameLine();
        if (ImGui.BeginChild("middle", middleSize, true, ImGuiWindowFlags.NoCollapse))
        {
            DrawPresetPlot();
        }

        ImGui.EndChild();
        ImGui.SameLine();
        if (ImGui.BeginChild("right", rightSize, true, ImGuiWindowFlags.NoCollapse))
        {
            if (ImGui.BeginChild("Properties", new Vector2(-1, ImGui.GetWindowHeight() - (rightSize.Y / 2.29f)), true, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollWithMouse))
            {
                DrawMemberProperties();
            }

            ImGui.EndChild();
            if (ImGui.BeginChild("Actions", layoutsChildSize, true, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollWithMouse))
            {
                DrawPresetActions();
            }

            ImGui.EndChild();
        }

        ImGui.EndChild();
    }

    private static void ImportPresetFromClipboard()
    {
        try
        {
            var presetContent = ImGui.GetClipboardText();

            // Clear clipboard text immediately to avoid re-imports
            ImGui.SetClipboardText(string.Empty);

            ConfigWindowHelpers.activePreset = JsonSerializer.Deserialize<Preset>(presetContent) ?? new Preset();
            ConfigWindowHelpers.selectedLayout = ConfigWindowHelpers.activePreset.PresetName;

            ConfigWindowHelpers.activePreset.Members ??= [];

            if (!string.IsNullOrEmpty(ConfigWindowHelpers.activePreset.PresetName))
            {
                var jsonString = JsonSerializer.Serialize(ConfigWindowHelpers.activePreset, SerializerOptions);
                File.WriteAllText(Path.Combine(Potatoes.PresetDirectory, ConfigWindowHelpers.activePreset.PresetName + ".json"), jsonString);
            }
            else
            {
                throw new Exception("PresetName is null or empty");
            }
        }
        catch (Exception)
        {
            Sausages.NotificationManager.AddNotification(new Notification
            {
                Content = "You can only import valid Presets",
            });
        }
    }

    private static void SaveActivePreset()
    {
        if (ConfigWindowHelpers.activePreset == null || string.IsNullOrEmpty(ConfigWindowHelpers.selectedLayout))
        {
            return;
        }

        var jsonString = JsonSerializer.Serialize(ConfigWindowHelpers.activePreset, SerializerOptions);
        File.WriteAllText(Path.Combine(Potatoes.PresetDirectory, ConfigWindowHelpers.selectedLayout + ".json"), jsonString);
    }

    private static void DrawPresetPlot()
    {
        if (ConfigWindowHelpers.selectedLayout == null)
        {
            return;
        }

        double minX = -10;
        double maxX = 10;
        double minY = -10;
        double maxY = 10;
        ImPlot.SetNextAxesLimits(minX, maxX, minY, maxY, ImPlotCond.Always);
        if (!ImPlot.BeginPlot(ConfigWindowHelpers.selectedLayout, new Vector2(-1, -1), ImPlotFlags.NoLegend))
        {
            return;
        }

        if (ConfigWindowHelpers.activePreset?.Members != null)
        {
            var mousePos = ImPlot.GetPlotMousePos();

            var colorIndex = 0;
            foreach (var member in ConfigWindowHelpers.activePreset.Members.ToList())
            {
                var x = member.X;
                var y = member.Y;
                var rot = member.ROT;

                if (colorIndex >= ConfigWindowHelpers.memberColors.Count)
                {
                    colorIndex = 0;
                }

                var plotSize = ImPlot.GetPlotSize();

                // Constants
                var arrowLength = 1.75f * 600f / plotSize.Length();
                var arrowheadLength = 0.3f * arrowLength;
                var angleOffset = 135f.Degrees().Rad;

                // Calculate arrow end point
                var rotOffset = -rot + (MathF.PI / 2);
                var endX = (float)(x + (arrowLength * Math.Cos(rotOffset)));
                var endY = (float)(y + (arrowLength * Math.Sin(rotOffset)));

                // Prepare the main arrow points
                var xs = new[] { (float)x, endX };
                var ys = new[] { (float)y, endY };

                ImPlot.SetupLegend(ImPlotLocation.NorthWest, ImPlotLegendFlags.NoMenus);

                void PlotArrowhead(float baseX, float baseY, float angle)
                {
                    var offsetX = (float)(arrowheadLength * Math.Cos(angle));
                    var offsetY = (float)(arrowheadLength * Math.Sin(angle));

                    var arrowheadXs = new[] { baseX, baseX + offsetX };
                    var arrowheadYs = new[] { baseY, baseY + offsetY };

                    ImPlot.PlotLine(member.CharacterName + "_arrowhead", ref arrowheadXs[0], ref arrowheadYs[0], 2);
                }

                var memberColor = member.isAnchor
                    ? new Vector4(76 / 255f, 114 / 255f, 176 / 255f, 255 / 255f)
                    : ConfigWindowHelpers.memberColors[colorIndex];

                ImPlot.SetNextLineStyle(memberColor, 2f);
                ImPlot.PlotLine(member.CharacterName, ref xs[0], ref ys[0], 2);
                ImPlot.SetNextLineStyle(memberColor, 2f);
                PlotArrowhead(endX, endY, rotOffset + angleOffset);
                ImPlot.SetNextLineStyle(memberColor, 2f);
                PlotArrowhead(endX, endY, rotOffset - angleOffset);

                if (member.isAnchor)
                {
                    var plotFloat = new[] { (float)x, (float)y };

                    ImPlot.GetStyle().MarkerSize = 15f;
                    ImPlot.PlotScatter("##DummyPlot", ref plotFloat[0], 1);
                }
                else
                {
                    ImPlot.GetStyle().MarkerSize = 15f;
                }

                if (!member.isAnchor)
                {
                    if (ImPlot.DragPoint((int)ImGui.GetID(member.CharacterName), ref x, ref y, ConfigWindowHelpers.memberColors[colorIndex], 15f))
                    {
                        ConfigWindowHelpers.SetSelectedMember(member.CharacterName);
                    }

                    if (ImGui.IsItemHovered())
                    {
                        // where a is [x,y] and b is [mouseX, mouseY] : (a squared) + (y squared) = vector magnitude aka distance
                        var distance = ((x - mousePos.X) * (x - mousePos.X)) + ((y - mousePos.Y) * (y - mousePos.Y));
                        var verticalScrollInput = ImGui.GetIO().MouseWheel;

                        if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                        {
                            if (distance < 0.15f)
                            {
                                ConfigWindowHelpers.SetSelectedMember(member.CharacterName);
                                Veggies.SendNotification("Selected : " + ConfigWindowHelpers.GetSelectedMember());
                            }
                        }

                        if (distance < 0.15f && verticalScrollInput != 0f)
                        {
                            if (verticalScrollInput > 0f)
                            {
                                // Mouse scrolled up
                                rot += 15f.Degrees().Rad;
                                if (rot >= 180f.Degrees().Rad)
                                {
                                    rot -= 360f.Degrees().Rad;
                                }
                            }

                            if (verticalScrollInput < 0f)
                            {
                                // Mouse scrolled down
                                rot -= 15f.Degrees().Rad;
                                if (rot <= -180f.Degrees().Rad)
                                {
                                    rot += 360f.Degrees().Rad;
                                }
                            }
                        }
                    }
                }

                var initials = string.Join(string.Empty, member.CharacterName.Split(' ').SelectMany(s => s.FirstOrDefault().ToString()));

                ImPlot.PlotText(initials, x, y);

                member.X = x;
                member.Y = y;
                member.ROT = rot;

                colorIndex++;
            }

            SaveActivePreset();
        }

        ImPlot.EndPlot();
    }

    private static void DrawMemberProperties()
    {
        if (ConfigWindowHelpers.GetSelectedMember().IsNullOrEmpty())
        {
            ImGui.TextWrapped("Select a Preset Member to begin editing their properties.");
            return;
        }

        var member = ConfigWindowHelpers.activePreset?.Members?.FirstOrDefault((m) => m.CharacterName == ConfigWindowHelpers.GetSelectedMember()) ?? new PresetMember();
        var dragFloatWidth = 0f;

        var x = (float)member.X;
        var y = (float)member.Y;
        var rot = member.ROT.Radians().Deg;
        var isAnchor = member.isAnchor;
        var emote = member.emote;

        var xchanged = false;
        var ychanged = false;
        var rotchanged = false;
        var anchorchanged = false;

        ImGui.Text(ConfigWindowHelpers.GetSelectedMember() + " Properties");
        ImGui.Dummy(new Vector2(0, 10));
        ImGui.Text("Anchor Member");
        ImGui.SameLine();
        ImGuiEx.ImGuiLineRightAlign("Offsetanchor", () =>
        {
            anchorchanged = ImGui.Checkbox("##anchor", ref isAnchor);
            if (ImGui.IsItemHovered())
            {
                using var tooltip = ImRaii.Tooltip();
                ImGui.Text("An Anchor Member is the member that the rest");
                ImGui.Text("of the members base their positions off of.");
            }

            ImGui.SameLine();
            ImGui.Text("  ");
        });
        ImGui.Dummy(new Vector2(0, 10));
        ImGui.Text("Offset X");
        ImGui.SameLine();
        ImGuiEx.ImGuiLineRightAlign("Offsetx", () =>
        {
            xchanged = ImGui.DragFloat("##x", ref x, 0.05f, 0f, 0f, "%.2f");
            ImGui.SameLine();
            ImGui.Text("  ");
        });
        ImGui.Dummy(new Vector2(0, 10));
        ImGui.Text("Offset Y");
        ImGui.SameLine();
        ImGuiEx.ImGuiLineRightAlign("Offsety", () =>
        {
            ychanged = ImGui.DragFloat("##y", ref y, 0.05f, 0f, 0f, "%.2f");
            ImGui.SameLine();
            ImGui.Text("  ");
        });
        ImGui.Dummy(new Vector2(0, 10));
        ImGui.Text("Rotation");
        ImGui.SameLine();
        ImGuiEx.ImGuiLineRightAlign("Offsetrot", () =>
        {
            dragFloatWidth = ImGui.CalcItemWidth();
            rotchanged = ImGui.DragFloat("##rot", ref rot, 0.05f, 0f, 0f, "%.2f");
            ImGui.SameLine();
            ImGui.Text("  ");
        });

        ImGui.Dummy(new Vector2(0, 10));
        ImGui.Text("Emote");
        ImGui.SameLine();
        ImGuiEx.ImGuiLineRightAlign("Offsetcombo", () =>
        {
            var comboSize = new Vector2(150f, 600f);
            ImGui.SetNextWindowSize(comboSize);

            ImGui.SetNextItemWidth(dragFloatWidth - 30f);
            if (ImGui.BeginCombo("##EmoteCombo", emote))
            {
                if (ImGui.BeginChild("##EmoteSearch", new Vector2(-1, 25f), false, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar))
                {
                    ImGui.SetNextItemWidth(comboSize.X - 10f);
                    ImGui.InputText("##S", ref ConfigWindowHelpers.filterText, ConfigWindow.BufferSize);
                }

                ImGui.EndChild();
                ImGui.Dummy(new Vector2(0f, 5f));

                if (ImGui.BeginChild("##EmoteList", new Vector2(-1, -1), false, ImGuiWindowFlags.NoCollapse))
                {
                    foreach (var j in Sausages.Emotes)
                    {
                        var i = j.Replace("/", string.Empty);
                        if (!ConfigWindowHelpers.filterText.IsNullOrEmpty() && !i.Contains(ConfigWindowHelpers.filterText))
                        {
                            continue;
                        }

                        if (ImGui.Selectable(i, i == member.emote))
                        {
                            emote = i;
                        }

                        if (i == member.emote)
                        {
                            ImGui.SetItemDefaultFocus();
                        }
                    }
                }

                ImGui.EndChild();

                ImGui.EndCombo();
            }

            ImGui.SameLine();
            ImGui.Dummy(new Vector2(5f, 0f));
            ImGui.SameLine();
            if (ConfigWindowHelpers.IconButtonWithText(FontAwesomeIcon.Trash, string.Empty, "Remove emote for Member"))
            {
                emote = string.Empty;
            }

            ImGui.SameLine();
            ImGui.Text("  ");
        });

        if (member.isAnchor)
        {
            x = 0;
            y = 0;
            rot = 0;
        }

        member.X = Math.Clamp(x, -10, 10);
        member.Y = Math.Clamp(y, -10, 10);
        member.ROT = Math.Clamp(rot, -180, 180).Degrees().Rad;
        member.isAnchor = isAnchor;
        member.emote = emote;

        if (xchanged || ychanged || rotchanged || anchorchanged)
        {
            SaveActivePreset();
        }
    }

    private static void DrawPresetActions()
    {
        if (ConfigWindowHelpers.selectedLayout.IsNullOrEmpty())
        {
            ImGui.Text("Select a preset to see available actions.");
            return;
        }

        ImGui.Text("Actions");
        ImGui.Dummy(new Vector2(0, 10));
        ImGui.Text("Move to Positions");
        ImGui.SameLine();
        ImGuiEx.ImGuiLineRightAlign("OffsetPos", () =>
        {
            if (ConfigWindowHelpers.IconButtonWithText(FontAwesomeIcon.PersonWalkingArrowRight, string.Empty, "Send a party message instructing members to move to their positions."))
            {
                SendPresetToParty(false);
            }

            ImGui.SameLine();
            ImGui.Text("  ");
        });
        ImGui.Dummy(new Vector2(0, 10));
        ImGui.Text("Begin Preset");
        ImGui.SameLine();
        ImGuiEx.ImGuiLineRightAlign("OffsetPlay", () =>
        {
            if (ConfigWindowHelpers.IconButtonWithText(FontAwesomeIcon.Play, string.Empty, "Send a party message instructing members to begin their emotes."))
            {
                SendPresetToParty(true);
            }

            ImGui.SameLine();
            ImGui.Text("  ");
        });
        ImGui.Dummy(new Vector2(0, 10));
        ImGui.Text("Abort Movement");
        ImGui.SameLine();
        ImGuiEx.ImGuiLineRightAlign("OffsetAbort", () =>
        {
            if (ConfigWindowHelpers.IconButtonWithText(FontAwesomeIcon.Stop, string.Empty, "Stop all movement."))
            {
                Meat.StopScooch();
            }

            ImGui.SameLine();
            ImGui.Text("  ");
        });
    }

    private static void SendPresetToParty(bool doEmote)
    {
        if (ConfigWindowHelpers.activePreset == null)
        {
            return;
        }

        try
        {
            var commandToSend = Gravy.CreatePartyChatPresetString(ConfigWindowHelpers.activePreset, Potatoes.Configuration!.CatchPhrase, doEmote);
            Meat.ProccessChatCommands(XivChatType.None, commandToSend.Replace("/p ", string.Empty), true);
            if (Sausages.PartyList.Length > 1)
            {
                Veggies.SendChatMessageAsIfPlayer(commandToSend);
                Veggies.SendNotification("Sent instructions to party");
            }
        }
        catch (Exception ex)
        {
            Sausages.NotificationManager.AddNotification(new Notification
            {
                Content = "Instructions failed to send: " + ex.Message,
            });
        }
    }
}
