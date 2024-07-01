using Dalamud.Game.Text;
using ImGuiNET;
using PunishLib.ImGuiMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimonSays.Windows.Tabs
{
    internal class Settings
    {
        public static void DrawOld()
        {
           
            ImGui.Text("");
            ImGui.Separator();
            ImGui.Text("");

            ImGui.Text("");
            ImGui.Separator();
            ImGui.Text("");
            ImGui.Text("Experimental Features");
            ConfigWindowHelpers.DrawExperimentCheckboxes();
        }

        public static void Draw()
        {
            var WindowWidth = ImGui.GetWindowWidth();
            var WindowHeight = ImGui.GetWindowHeight();
            var LeftChildSize = new System.Numerics.Vector2(WindowWidth / 2.035f, WindowHeight / 1.2f);
            var RightchildSize = new System.Numerics.Vector2(WindowWidth / 2.035f, WindowHeight / 1.2f);

            ImGui.NewLine();
            if (ImGui.BeginChild("Left", LeftChildSize, true, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollWithMouse))
            {
                ImGui.TextWrapped("Catchphrase Options");
                ImGui.Dummy(new System.Numerics.Vector2(0, 10));
                ImGui.TextWrapped("Catchphrase - This is what SimonSays listens to.");
                ImGui.SameLine();
                ImGuiEx.ImGuiLineRightAlign("Settings_Catchphrase", () =>
                {
                    ConfigWindowHelpers.DrawCatchPhBox();
                    ImGui.SameLine();
                    ImGui.Text("   ");
                });
                

                ImGui.TextColored(new System.Numerics.Vector4(160, 160, 160, 0.8f), "Your Catchprase IS your security, change this to control who can command you.");
                ImGui.TextColored(new System.Numerics.Vector4(160, 160, 160, 0.8f), "Only share this to people you want to be able to command you.");
                ImGui.Dummy(new System.Numerics.Vector2(0, 10));
                ImGui.TextWrapped("Emote Options");
                ImGui.Dummy(new System.Numerics.Vector2(0, 10));
                ImGui.TextWrapped("Motion Only - Emote will not print to chat.");
                ImGui.SameLine();
                var motionOnly = Potatoes.Configuration!.MotionOnly;
                ImGuiEx.ImGuiLineRightAlign("Settings_Motion", () =>
                {
                    if (ImGui.Checkbox("## Motion", ref motionOnly))
                    {
                        // Update the MotionOnly property in the configuration object with the new value
                        Potatoes.Configuration!.MotionOnly = motionOnly;

                        // Save the updated configuration
                        Potatoes.Configuration!.Save();
                    }
                    ImGui.SameLine();
                    ImGui.Text("   ");
                });
                ImGui.Dummy(new System.Numerics.Vector2(0, 5));
                ImGui.PushTextWrapPos(ImGui.GetCursorPos().X + LeftChildSize.X - 350f);
                ImGui.TextWrapped("Test - Send a fake chat message to yourself to test your catchphrase.");
                ImGui.PopTextWrapPos();
                
                ImGui.SameLine();
                ImGuiEx.ImGuiLineRightAlign("Settings_Test", () =>
                {
                    ImGui.SetNextItemWidth(250f);
                    ImGui.InputText("##", ref ConfigWindowHelpers.testText, 50U);
                    ImGui.SameLine();
                    ImGui.Text("   ");
                });

                ImGuiEx.ImGuiLineRightAlign("Settings_SendTest", () =>
                {
                    ImGui.SetNextItemWidth(100f);
                    if (ImGui.Button("Send"))
                    {
                        // Call the Meat.Command method with the XivChatType.None parameter, the value of the testText variable, and ForceForTesting set to true
                        Meat.ProccessChatCommands(XivChatType.None, ConfigWindowHelpers.testText, ForceForTesting: true);
                    }
                    ImGui.SameLine();
                    ImGui.Text("   ");
                });




            }
            ImGui.EndChild();
            ImGui.SameLine();
            if (ImGui.BeginChild("Right", RightchildSize, true, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollWithMouse))
            {
                ImGui.TextWrapped("These are the channels SimonSays will listen to, waiting for your catchphrase.");
                ImGui.Dummy(new System.Numerics.Vector2(0, 10));
                ConfigWindowHelpers.DrawCheckboxes();
                ImGui.Dummy(new System.Numerics.Vector2(0, 10));
                ImGui.TextWrapped("Override Actions");
                ImGui.Dummy(new System.Numerics.Vector2(0, 10));
                ImGui.Text("Positional Sync Overrides");
                ConfigWindowHelpers.DrawExperimentCheckboxes();
            }
            ImGui.EndChild();
        }
    }

}
