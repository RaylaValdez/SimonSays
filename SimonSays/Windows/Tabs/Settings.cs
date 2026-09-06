using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Text;
using SimonSays.ImGuiMethods;

namespace SimonSays.Windows.Tabs;

internal class Settings
{
    public static void Draw()
    {
        var windowWidth = ImGui.GetWindowWidth();
        var windowHeight = ImGui.GetWindowHeight();
        var leftChildSize = new Vector2(windowWidth / 2.035f, windowHeight / 1.2f);
        var rightChildSize = new Vector2(windowWidth / 2.035f, windowHeight / 1.2f);

        ImGui.NewLine();
        if (ImGui.BeginChild("Left", leftChildSize, true, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollWithMouse))
        {
            ImGui.TextWrapped("Catchphrase Options");
            ImGui.Dummy(new Vector2(0, 10));
            ImGui.TextWrapped("Catchphrase - This is what SimonSays listens to.");
            ImGui.SameLine();
            ImGuiEx.ImGuiLineRightAlign("Settings_Catchphrase", () =>
            {
                ConfigWindowHelpers.DrawCatchPhBox();
                ImGui.SameLine();
                ImGui.Text("   ");
            });

            ImGui.TextColored(new Vector4(160, 160, 160, 0.8f), "Your Catchprase IS your security, change this to control who can command you.");
            ImGui.TextColored(new Vector4(160, 160, 160, 0.8f), "Only share this to people you want to be able to command you.");
            ImGui.Dummy(new Vector2(0, 10));
            ImGui.TextWrapped("Emote Options");
            ImGui.Dummy(new Vector2(0, 10));
            ImGui.TextWrapped("Motion Only - Emote will not print to chat.");
            ImGui.SameLine();
            var motionOnly = Potatoes.Configuration!.MotionOnly;
            ImGuiEx.ImGuiLineRightAlign("Settings_Motion", () =>
            {
                if (ImGui.Checkbox("## Motion", ref motionOnly))
                {
                    Potatoes.Configuration!.MotionOnly = motionOnly;
                    Potatoes.Configuration!.Save();
                }

                ImGui.SameLine();
                ImGui.Text("   ");
            });
            ImGui.Dummy(new Vector2(0, 5));
            ImGui.PushTextWrapPos(ImGui.GetCursorPos().X + leftChildSize.X - 350f);
            ImGui.TextWrapped("Test - Send a fake chat message to yourself to test your catchphrase.");
            ImGui.PopTextWrapPos();

            ImGui.SameLine();
            ImGuiEx.ImGuiLineRightAlign("Settings_Test", () =>
            {
                ImGui.SetNextItemWidth(250f);
                ImGui.InputText("##", ref ConfigWindowHelpers.testText, 50);
                ImGui.SameLine();
                ImGui.Text("   ");
            });

            ImGuiEx.ImGuiLineRightAlign("Settings_SendTest", () =>
            {
                ImGui.SetNextItemWidth(100f);
                if (ImGui.Button("Send"))
                {
                    Meat.ProccessChatCommands(XivChatType.None, ConfigWindowHelpers.testText, forceForTesting: true);
                }

                ImGui.SameLine();
                ImGui.Text("   ");
            });
        }

        ImGui.EndChild();
        ImGui.SameLine();
        if (ImGui.BeginChild("Right", rightChildSize, true, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollWithMouse))
        {
            ImGui.TextWrapped("These are the channels SimonSays will listen to, waiting for your catchphrase.");
            ImGui.Dummy(new Vector2(0, 10));
            ConfigWindowHelpers.DrawCheckboxes();
            ImGui.Dummy(new Vector2(0, 10));
            ImGui.TextWrapped("Override Actions");
            ImGui.Dummy(new Vector2(0, 10));
            ImGui.Text("Positional Sync Overrides");
            ConfigWindowHelpers.DrawExperimentCheckboxes();
        }

        ImGui.EndChild();
    }
}
