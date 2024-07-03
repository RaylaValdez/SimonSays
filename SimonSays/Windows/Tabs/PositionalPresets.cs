using Dalamud.Game.Config;
using Dalamud.Game.Text;
using Dalamud.Interface;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using ImGuiNET;
using ImPlotNET;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using PunishLib.ImGuiMethods;
using SimonSays.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Vector2 = System.Numerics.Vector2;
using Dalamud.Game.ClientState.Objects.SubKinds;

namespace SimonSays.Windows.Tabs
{
    internal class PositionalPresets
    {

        public static void Draw()
        {
            var WindowWidth = ImGui.GetWindowWidth();
            var WindowHeight = ImGui.GetWindowHeight();

            var LeftSize = new Vector2(WindowWidth / 4.3f, WindowHeight / 1.2f);
            var LayoutsChildSize = new Vector2(-1, LeftSize.Y / 2.5f);
            var LayoutsSize = new Vector2(-1, -1);
            var MembersSize = new Vector2(-1, -1);
            var MiddleSize = new Vector2(WindowWidth / 1.95f, WindowHeight / 1.2f);
            var RightSize = new Vector2(WindowWidth / 4.3f, WindowHeight / 1.2f);

            ImGui.NewLine();
            if (ImGui.BeginChild("left", LeftSize, true, ImGuiWindowFlags.NoCollapse))
            {
                if (ImGui.BeginChild("layouts", LayoutsChildSize, true, ImGuiWindowFlags.NoCollapse))
                {
                    if (ImGui.BeginChild("buttons", new Vector2(-1, ImGui.GetTextLineHeight() * 2.3f), false, ImGuiWindowFlags.NoCollapse))
                    {
                        ImGui.Text("Presets");
                        ImGui.SameLine();
                        ImGuiEx.ImGuiLineRightAlign("LayoutButtons", () =>
                        {
                            if (ConfigWindowHelpers.IconButtonWithText(FontAwesomeIcon.Plus, "", "Create new Preset"))
                            {
                                ImGui.SetNextWindowPos(new Vector2(10.0f, 100.0f));
                                ImGui.SetNextWindowSize(new Vector2(200.0f, 60.0f));
                                ConfigWindowHelpers.namingWindowOpen = true;
                            }

                            ImGui.SameLine();
                            ImGui.Dummy(new Vector2(1, 0));
                            ImGui.SameLine();
                            if (ConfigWindowHelpers.IconButtonWithText(FontAwesomeIcon.Minus, "", "Delete selected Preset"))
                            {
                                File.Delete(Potatoes.PresetDirectory + "/" + ConfigWindowHelpers.selectedLayout + ".json");
                                ConfigWindowHelpers.selectedLayout = string.Empty;
                                ConfigWindowHelpers.activePreset = null;
                            }
                            ImGui.SameLine();
                            ImGui.Dummy(new Vector2(1, 0));
                            ImGui.SameLine();
                            if (ConfigWindowHelpers.IconButtonWithText(FontAwesomeIcon.Clipboard, "", "Copy Preset to Clipboard"))
                            {
                                if (!string.IsNullOrEmpty(ConfigWindowHelpers.selectedLayout))
                                {
                                    var presetContent = File.ReadAllLines(Potatoes.PresetDirectory + "/" + ConfigWindowHelpers.selectedLayout + ".json");
                                    var contentToCopy = string.Join(Environment.NewLine, presetContent);

                                    // Copy to clipboard
                                    ImGui.SetClipboardText(contentToCopy);

                                    var clipboardsuccess = new Notification();
                                    clipboardsuccess.Content = "Preset copied to Clipboard.";

                                    Sausages.NotificationManager.AddNotification(clipboardsuccess);
                                }
                                else
                                {
                                    var clipboardfailure = new Notification();
                                    clipboardfailure.Content = "Select a preset to copy to Clipboard.";

                                    Sausages.NotificationManager.AddNotification(clipboardfailure);
                                }
                            }
                            ImGui.SameLine();
                            ImGui.Dummy(new Vector2(1, 0));
                            ImGui.SameLine();
                            var importInProgress = false; // Flag to track import progress

                            if (ConfigWindowHelpers.IconButtonWithText(FontAwesomeIcon.FileImport, "", "Import Preset from Clipboard"))
                            {
                                // Check if import is not already in progress and the button is pressed
                                if (!importInProgress)
                                {
                                    importInProgress = true; // Set flag to indicate import is in progress

                                    try
                                    {
                                        // Retrieve clipboard text
                                        var presetContent = ImGui.GetClipboardText();

                                        // Clear clipboard text immediately to avoid re-imports
                                        ImGui.SetClipboardText("");
                                        ConfigWindowHelpers.

                                                                            // Deserialize preset content
                                                                            activePreset = JsonSerializer.Deserialize<Preset>(presetContent) ?? new Preset();
                                        ConfigWindowHelpers.selectedLayout = ConfigWindowHelpers.activePreset.PresetName;

                                        // Initialize Members if null
                                        ConfigWindowHelpers.activePreset.Members ??= [];

                                        // Check if PresetName is valid
                                        if (!string.IsNullOrEmpty(ConfigWindowHelpers.activePreset.PresetName))
                                        {
                                            // Serialize and write to file
                                            var options = new JsonSerializerOptions { WriteIndented = true };
                                            var jsonString = JsonSerializer.Serialize(ConfigWindowHelpers.activePreset, options);
                                            File.WriteAllText(Potatoes.PresetDirectory + "/" + ConfigWindowHelpers.activePreset.PresetName + ".json", jsonString);

                                        }
                                        else
                                        {
                                            throw new Exception("PresetName is null or empty");
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        var importFailure = new Notification();
                                        importFailure.Content = "You can only import valid Presets";
                                        Sausages.NotificationManager.AddNotification(importFailure);
                                    }
                                    finally
                                    {
                                        importInProgress = false; // Reset flag whether import succeeded or failed
                                    }
                                }
                            }

                        });
                    }
                    ImGui.EndChild();

                    //ImGui.Dummy(new Vector2(0, 0));

                    using var Layouts = ImRaii.ListBox("##", LayoutsSize);
                    if (Layouts.Success)
                    {
                        foreach (var file in Directory.EnumerateFiles(Potatoes.PresetDirectory))
                        {
                            var fileName = Path.GetFileNameWithoutExtension(file);
                            if (ImGui.Selectable(fileName, ConfigWindowHelpers.selectedLayout == fileName))
                            {
                                ConfigWindowHelpers.selectedLayout = fileName;

                                var presetContent = File.ReadAllLines(Potatoes.PresetDirectory + "/" + ConfigWindowHelpers.selectedLayout + ".json");
                                var jsonString = string.Join(Environment.NewLine, presetContent);
                                ConfigWindowHelpers.


                                                                activePreset = JsonSerializer.Deserialize<Preset>(jsonString) ?? new Preset();
                                ConfigWindowHelpers.activePreset.Members ??= [];

                            }

                            if (ConfigWindowHelpers.selectedLayout == fileName)
                            {
                                if (ImGui.IsItemHovered())
                                {
                                    if (ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                                    {
                                        Sausages.Log.Debug("Opening context popup");
                                        ConfigWindowHelpers.
                                                                                contextPopupOpen = true;
                                    }
                                }
                            }
                        }

                    }
                }
                ImGui.EndChild();
                if (ImGui.BeginChild("members", MembersSize, true, ImGuiWindowFlags.NoCollapse))
                {
                    if (!ConfigWindowHelpers.selectedLayout.IsNullOrEmpty())
                    {
                        if (ImGui.BeginChild("memberbuttons", new Vector2(-1, ImGui.GetTextLineHeight() * 2.3f), false, ImGuiWindowFlags.NoCollapse))
                        {
                            ImGui.Text("Members");
                            ImGui.SameLine();
                            ImGuiEx.ImGuiLineRightAlign("MembersButtons", () =>
                            {
                                if (ConfigWindowHelpers.IconButtonWithText(FontAwesomeIcon.StreetView, "", "Add targetted Character."))
                                {
                                    var target = Sausages.TargetManager.Target;
                                    if (target != null)
                                    {
                                        if (ConfigWindowHelpers.activePreset != null)
                                        {
                                            ConfigWindowHelpers.activePreset.Members!.Add(new PresetMember(target.Name.ToString()));
                                            var options = new JsonSerializerOptions { WriteIndented = true };
                                            var jsonString = JsonSerializer.Serialize(ConfigWindowHelpers.activePreset, options);
                                            File.WriteAllText(Potatoes.PresetDirectory + "/" + ConfigWindowHelpers.selectedLayout + ".json", jsonString);
                                        }
                                    }

                                }
                                ImGui.SameLine();
                                ImGui.Dummy(new Vector2(1, 0));
                                ImGui.SameLine();

                                if (ConfigWindowHelpers.IconButtonWithText(FontAwesomeIcon.Minus, "", "Remove Selected Member"))
                                {
                                    if (ConfigWindowHelpers.activePreset != null)
                                    {
                                        ConfigWindowHelpers.
                                                                            // activePreset.Members.Remove(selectedMember);
                                                                            activePreset.Members = ConfigWindowHelpers.activePreset.Members.Where((member) => member.CharacterName != ConfigWindowHelpers.GetSelectedMember()).ToList(); // select every member that doesn't have the same character name
                                        var options = new JsonSerializerOptions { WriteIndented = true };
                                        var jsonString = JsonSerializer.Serialize(ConfigWindowHelpers.activePreset, options);
                                        File.WriteAllText(Potatoes.PresetDirectory + "/" + ConfigWindowHelpers.selectedLayout + ".json", jsonString);
                                    }
                                }
                            });
                        }
                        ImGui.EndChild();

                        using var Members = ImRaii.ListBox("##", MembersSize);
                        if (Members.Success)
                        {
                            if (ConfigWindowHelpers.activePreset != null)
                            {
                                if (ConfigWindowHelpers.activePreset.Members != null)
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
                }
                ImGui.EndChild();
            }
            ImGui.EndChild();
            ImGui.SameLine();
            if (ImGui.BeginChild("middle", MiddleSize, true, ImGuiWindowFlags.NoCollapse))
            {
                if (ConfigWindowHelpers.selectedLayout != null)
                {
                    double minX = -10;
                    double maxX = 10;
                    double minY = -10;
                    double maxY = 10;
                    ImPlot.SetNextAxesLimits(minX, maxX, minY, maxY, ImPlotCond.Always);
                    if (ImPlot.BeginPlot(ConfigWindowHelpers.selectedLayout, new Vector2(-1, -1), ImPlotFlags.None | ImPlotFlags.NoLegend))
                    {
                        if (ConfigWindowHelpers.activePreset != null)
                        {
                            if (ConfigWindowHelpers.activePreset.Members != null)
                            {
                                var mousePos = ImPlot.GetPlotMousePos();


                                var colorIndex = 0;
                                foreach (var member in ConfigWindowHelpers.activePreset.Members.ToList())
                                {
                                    var X = member.X;
                                    var Y = member.Y;
                                    var rot = member.ROT;

                                    // Ensure colorIndex does not exceed the bounds of memberColors
                                    if (colorIndex >= ConfigWindowHelpers.memberColors.Count)
                                    {
                                        colorIndex = 0; // Reset colorIndex if it exceeds the list size
                                    }

                                    var plotSize = ImPlot.GetPlotSize();

                                    // Constants
                                    var arrowLength = 1.75f * 600f / plotSize.Length();
                                    var arrowheadLength = 0.3f * arrowLength;
                                    var angleOffset = 135f.Degrees().Rad;

                                    // Calculate arrow end point
                                    var rotOffset = -rot + (MathF.PI / 2);
                                    var endX = (float)(X + (arrowLength * Math.Cos(rotOffset)));
                                    var endY = (float)(Y + (arrowLength * Math.Sin(rotOffset)));

                                    // Prepare the main arrow points
                                    var xs = new float[] { (float)X, endX };
                                    var ys = new float[] { (float)Y, endY };

                                    ImPlot.SetupLegend(ImPlotLocation.NorthWest, ImPlotLegendFlags.NoMenus);
                                    
                                    // Helper function for arrowhead calculation
                                    void PlotArrowhead(float baseX, float baseY, float angle)
                                    {
                                        var offsetX = (float)(arrowheadLength * Math.Cos(angle));
                                        var offsetY = (float)(arrowheadLength * Math.Sin(angle));

                                        var arrowheadXs = new float[] { baseX, baseX + offsetX };
                                        var arrowheadYs = new float[] { baseY, baseY + offsetY };

                                        ImPlot.PlotLine(member.CharacterName + "_arrowhead", ref arrowheadXs[0], ref arrowheadYs[0], 2);
                                    }

                                    if (!member.isAnchor)
                                    {
                                        ImPlot.SetNextLineStyle(ConfigWindowHelpers.memberColors[colorIndex], 2f);
                                        ImPlot.PlotLine(member.CharacterName, ref xs[0], ref ys[0], 2);
                                        ImPlot.SetNextLineStyle(ConfigWindowHelpers.memberColors[colorIndex], 2f);
                                        PlotArrowhead(endX, endY, rotOffset + angleOffset);
                                        ImPlot.SetNextLineStyle(ConfigWindowHelpers.memberColors[colorIndex], 2f);
                                        PlotArrowhead(endX, endY, rotOffset - angleOffset);
                                    }
                                    else
                                    {
                                        // Blue of the dummy plot rgb(76, 114, 176)
                                        ImPlot.SetNextLineStyle(new System.Numerics.Vector4(76 / 255f, 114 / 255f, 176 / 255f, 255 / 255f), 2f);
                                        ImPlot.PlotLine(member.CharacterName, ref xs[0], ref ys[0], 2);
                                        ImPlot.SetNextLineStyle(new System.Numerics.Vector4(76 / 255f, 114 / 255f, 176 / 255f, 255 / 255f), 2f);
                                        PlotArrowhead(endX, endY, rotOffset + angleOffset);
                                        ImPlot.SetNextLineStyle(new System.Numerics.Vector4(76 / 255f, 114 / 255f, 176 / 255f, 255 / 255f), 2f);
                                        PlotArrowhead(endX, endY, rotOffset - angleOffset);
                                    }
                                    
                                    


                                    var oldMarkerSize = ImPlot.GetStyle().MarkerSize;
                                    if (member.isAnchor)
                                    {
                                        var plotDouble = new float[] { (float)X, (float)Y };
                                        var localPlotSize = new float[] { (float)1 / 15f, (float)1 / 15f };

                                        ImPlot.GetStyle().MarkerSize = 15f;
                                        ImPlot.PlotScatter("##DummyPlot", ref plotDouble[0], 1);

                                        //ImPlot.PlotText(FontAwesomeIcon.Anchor.ToString(), X, Y); // supposed to be an anchor icon 
                                    }
                                    else
                                    {
                                        ImPlot.GetStyle().MarkerSize = 15f;
                                    }

                                    if ((!member.isAnchor) && ImPlot.DragPoint((int)ImGui.GetID(member.CharacterName), ref X, ref Y, ConfigWindowHelpers.memberColors[colorIndex], 15f))
                                    {
                                        ConfigWindowHelpers.SetSelectedMember(member.CharacterName);
                                    }
                                    if (ImGui.IsItemHovered())
                                    {
                                        // where a is [x,y] and b is [mouseX, mouseY] : (a squared) + (y squared) = vector magnitude aka distance
                                        var distance = ((X - mousePos.x) * (X - mousePos.x)) + ((Y - mousePos.y) * (Y - mousePos.y));
                                        var verticalScrollInput = ImGui.GetIO().MouseWheel;

                                        if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                                        {
                                            if (distance < 0.15f)
                                            {
                                                ConfigWindowHelpers.SetSelectedMember(member.CharacterName);
                                                Veggies.SendNotification("Selected : " + ConfigWindowHelpers.GetSelectedMember());
                                            }
                                        }
                                        
                                        

                                        if (distance < 0.15f)
                                        {
                                            //Sausages.Log.Debug("Hovering Over " + member.CharacterName.ToString());
                                            if (verticalScrollInput != 0f)
                                            {
                                                if (verticalScrollInput > 0f)
                                                {
                                                    //Sausages.Log.Debug("Scrolled Up While Hovering " + member.CharacterName.ToString());
                                                    //Mouse Scrolled up
                                                    rot += 15f.Degrees().Rad;
                                                    if (rot >= 180f.Degrees().Rad)
                                                    {
                                                        rot -= 360f.Degrees().Rad;
                                                    }
                                                }

                                                if (verticalScrollInput < 0f)
                                                {
                                                    //Sausages.Log.Debug("Scrolled Down While Hovering " + member.CharacterName.ToString());
                                                    //Mouse scrolled down
                                                    rot -= 15f.Degrees().Rad;
                                                    if (rot <= -180f.Degrees().Rad)
                                                    {
                                                        rot += 360f.Degrees().Rad;
                                                    }
                                                }
                                            }
                                        }
                                        
                                        
                                    }
                                    var initials = string.Join("", member.CharacterName.Split(" ").SelectMany(s => s.FirstOrDefault().ToString()));


                                    ImPlot.PlotText(initials, X, Y);


                                    member.X = X;
                                    member.Y = Y;
                                    member.ROT = rot;

                                    var jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };
                                    var options = jsonSerializerOptions;
                                    var jsonString = JsonSerializer.Serialize(ConfigWindowHelpers.activePreset, options);
                                    File.WriteAllText(Potatoes.PresetDirectory + "/" + ConfigWindowHelpers.selectedLayout + ".json", jsonString);

                                    colorIndex++; // Increment colorIndex for the next iteration
                                }



                            }
                        }
                    }
                    ImPlot.EndPlot();
                }
            }
            ImGui.EndChild();
            ImGui.SameLine();
            if (ImGui.BeginChild("right", RightSize, true, ImGuiWindowFlags.NoCollapse))
            {
                if (ImGui.BeginChild("Properties", new Vector2(-1, ImGui.GetWindowHeight() - (RightSize.Y / 2.29f)), true, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollWithMouse))
                {
                    if (!ConfigWindowHelpers.GetSelectedMember().IsNullOrEmpty())
                    {
                        var member = ConfigWindowHelpers.activePreset?.Members?.FirstOrDefault(((m) => m.CharacterName == ConfigWindowHelpers.GetSelectedMember())) ?? new PresetMember();
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
                        var emotechanged = false;

                        ImGui.Text(ConfigWindowHelpers.GetSelectedMember().ToString() + " Properties");
                        ImGui.Dummy(new Vector2(0, 10));
                        ImGui.Text("Anchor Member");
                        ImGui.SameLine();
                        ImGuiEx.ImGuiLineRightAlign("Offsetanchor", () =>
                        {
                            anchorchanged = ImGui.Checkbox("##anchor", ref isAnchor);
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.BeginTooltip();
                                ImGui.Text("An Anchor Member is the member that the rest");
                                ImGui.Text("of the members base their positions off of.");
                                ImGui.EndTooltip();
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
                            var ComboSize = new Vector2(150f, 600f);
                            ImGui.SetNextWindowSize(ComboSize);

                            ImGui.SetNextItemWidth(dragFloatWidth - 30f);
                            if (ImGui.BeginCombo("##EmoteCombo", emote))
                            {
                                if (ImGui.BeginChild("##EmoteSearch",new Vector2(-1 , 25f),false, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar))
                                {
                                    ImGui.SetNextItemWidth(ComboSize.X - 10f);
                                    ImGui.InputText("##S", ref ConfigWindowHelpers.filterText, ConfigWindow.BufferSize);
                                    
                                }
                                ImGui.EndChild();
                                ImGui.Dummy(new Vector2(0f,5f));
                                
                                if (ImGui.BeginChild("##EmoteList", new Vector2(-1,-1),false, ImGuiWindowFlags.NoCollapse))
                                {
                                    foreach (var j in Sausages.Emotes)
                                    {
                                        var i = j.Replace("/", "");
                                        if (!ConfigWindowHelpers.filterText.IsNullOrEmpty() && !i.Contains(ConfigWindowHelpers.filterText))
                                        {
                                            continue;
                                        }
                                        if (ImGui.Selectable(i, (i == member.emote)))
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
                            if (ConfigWindowHelpers.IconButtonWithText(FontAwesomeIcon.Trash, "", "Remove emote for Member"))
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



                        if (xchanged || ychanged || rotchanged || anchorchanged || emotechanged)
                        {
                            var jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };
                            var options = jsonSerializerOptions;
                            var jsonString = JsonSerializer.Serialize<Preset?>(ConfigWindowHelpers.activePreset, options);
                            File.WriteAllText(Potatoes.PresetDirectory + "/" + ConfigWindowHelpers.selectedLayout + ".json", jsonString);
                        }

                    }
                    else
                    {
                        ImGui.TextWrapped("Select a Preset Member to begin editing their properties.");
                    }
                }
                ImGui.EndChild();
                if (ImGui.BeginChild("Actions", LayoutsChildSize, true, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollWithMouse))
                {
                    if (!ConfigWindowHelpers.selectedLayout.IsNullOrEmpty())
                    {
                        ImGui.Text("Actions");
                        ImGui.Dummy(new Vector2(0, 10));
                        ImGui.Text("Move to Positions");
                        ImGui.SameLine();
                        ImGuiEx.ImGuiLineRightAlign("OffsetPos", () =>
                        {
                            if (ConfigWindowHelpers.IconButtonWithText(FontAwesomeIcon.PersonWalkingArrowRight, "", "Send a party message instructing members to move to their positions."))
                            {
                                if (ConfigWindowHelpers.activePreset != null)
                                {
                                    try
                                    {
                                        var commandToSend = Gravy.CreatePartyChatPresetString(ConfigWindowHelpers.activePreset, Potatoes.Configuration!.CatchPhrase, false); // don't do emote
                                        Meat.ProccessChatCommands(XivChatType.None, commandToSend.Replace("/p ", ""), true);
                                        if (Sausages.PartyList.Length > 1)
                                        {
                                            Veggies.SendChatMessageAsIfPlayer(commandToSend);
                                            Veggies.SendNotification("Sent instructions to party");
                                        }
                                    }
                                    catch (OverflowException overflow)
                                    {
                                        Veggies.SendNotification("Instructions failed to send: " + overflow.Message);
                                    }
                                    catch (Exception ex)
                                    {
                                        Veggies.SendNotification("Instructions failed to send: " + ex.Message);
                                    }
                                }
                            }
                            ImGui.SameLine();
                            ImGui.Text("  ");
                        });
                        ImGui.Dummy(new Vector2(0, 10));
                        ImGui.Text("Begin Preset");
                        ImGui.SameLine();
                        ImGuiEx.ImGuiLineRightAlign("OffsetPlay", () =>
                        {
                            if (ConfigWindowHelpers.IconButtonWithText(FontAwesomeIcon.Play, "", "Send a party message instructing members to begin their emotes."))
                            {
                                if (ConfigWindowHelpers.activePreset != null)
                                {
                                    try
                                    {
                                        var commandToSend = Gravy.CreatePartyChatPresetString(ConfigWindowHelpers.activePreset, Potatoes.Configuration!.CatchPhrase, true); // do do (hehe) emote
                                        Meat.ProccessChatCommands(XivChatType.None, commandToSend.Replace("/p ", ""), true);
                                        if (Sausages.PartyList.Length > 1)
                                        {
                                            Veggies.SendChatMessageAsIfPlayer(commandToSend);
                                            Veggies.SendNotification("Sent instructions to party");
                                        }

                                    }
                                    catch (OverflowException overflow)
                                    {
                                        Veggies.SendNotification("Instructions failed to send: " + overflow.Message);
                                    }
                                    catch (Exception ex)
                                    {
                                        Veggies.SendNotification("Instructions failed to send: " + ex.Message);
                                    }
                                }
                            }
                            ImGui.SameLine();
                            ImGui.Text("  ");
                        });
                        ImGui.Dummy(new Vector2(0, 10));
                        ImGui.Text("Abort Movement");
                        ImGui.SameLine();
                        ImGuiEx.ImGuiLineRightAlign("OffsetAbort", () =>
                        {
                            if (ConfigWindowHelpers.IconButtonWithText(FontAwesomeIcon.Stop, "", "Stop all movement."))
                            {
                                Meat.StopScooch();
                            }
                            ImGui.SameLine();
                            ImGui.Text("  ");
                        });

                    }
                    else
                    {
                        ImGui.Text("Select a preset to see available actions.");
                    }
                }
                ImGui.EndChild();
            }
            ImGui.EndChild();



        }
    }
}
