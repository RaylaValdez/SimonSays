using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace SimonSays.Windows.Tabs;

internal class Usage
{
    public static void Draw()
    {
        var windowWidth = ImGui.GetWindowWidth();
        var windowHeight = ImGui.GetWindowHeight();
        var leftChildSize = new Vector2(windowWidth / 2.035f, windowHeight / 1.2f);
        var rightChildSize = new Vector2(windowWidth / 2.035f, windowHeight / 1.2f);
        const float CommandWidth = 150f;

        ImGui.NewLine();
        if (ImGui.BeginChild("UsageLeft", leftChildSize, true, ImGuiWindowFlags.NoCollapse))
        {
            ImGui.Text("Commands :");
            ImGui.Dummy(new Vector2(0, 10));

            ImGui.Text("Command :");
            var simonsaysconfig = "/simonsaysconfig";
            ImGui.SetNextItemWidth(CommandWidth);
            ImGui.InputText("##/simonsaysconfig", ref simonsaysconfig, 50, ImGuiInputTextFlags.ReadOnly);
            ImGui.SameLine();
            ImGui.TextWrapped("Opens this window.");
            ImGui.TextWrapped("Aliases :");
            var sscfg = "/sscfg";
            ImGui.SetNextItemWidth(CommandWidth);
            ImGui.InputText("##/sscfg", ref sscfg, 50, ImGuiInputTextFlags.ReadOnly);
            ImGui.Dummy(new Vector2(0, 10));

            ImGui.Text("Command :");
            var sync = "/sync";
            ImGui.SetNextItemWidth(CommandWidth);
            ImGui.InputText("##/sync", ref sync, 50, ImGuiInputTextFlags.ReadOnly);
            ImGui.SameLine();
            ImGui.TextWrapped("Will attempt to sync positions with your Target.");
            ImGui.Dummy(new Vector2(0, 10));

            ImGui.Text("Command :");
            var simonsays = "/simonsays <TheirEmote> <YourEmote> <ShouldSync>";
            ImGui.SetNextItemWidth(CommandWidth + 250f);
            ImGui.InputText("##/simonsays", ref simonsays, 50, ImGuiInputTextFlags.ReadOnly);
            ImGui.SameLine();
            ImGui.TextWrapped("There are multiple ways to use this command.");
            ImGui.TextWrapped("Examples :");
            var simonsays1 = "Example 1 - '/simonsays hum'";
            ImGui.SetNextItemWidth(CommandWidth + 250f);
            ImGui.InputText("##/simonsays1", ref simonsays1, 50, ImGuiInputTextFlags.ReadOnly);
            ImGui.SameLine();
            ImGui.TextWrapped("This will make you and the person you're targetting hum.");
            var simonsays2 = "Example 2 - '/simonsays hum dance'";
            ImGui.SetNextItemWidth(CommandWidth + 250f);
            ImGui.InputText("##/simonsays2", ref simonsays2, 50, ImGuiInputTextFlags.ReadOnly);
            ImGui.SameLine();
            ImGui.TextWrapped("This will make the person you're targetting hum and make yourself dance.");
            var simonsays3 = "Example 3 - '/simonsays hum dance true'";
            ImGui.SetNextItemWidth(CommandWidth + 250f);
            ImGui.InputText("##/simonsays3", ref simonsays3, 50, ImGuiInputTextFlags.ReadOnly);
            ImGui.SameLine();
            ImGui.TextWrapped("This will attempt to sync your positions prior to making your target hum and yourself dance.");
            ImGui.TextWrapped("ShouldSync being 'true' can be used on all previous examples.");

            ImGui.Dummy(new Vector2(0, 10));
        }

        ImGui.EndChild();
        ImGui.SameLine();
        if (ImGui.BeginChild("UsageRight", rightChildSize, true, ImGuiWindowFlags.NoCollapse))
        {
            ImGui.TextWrapped("Posistional Presets :");
            ImGui.Dummy(new Vector2(0, 10));
            ImGui.TextWrapped("Presets can be used to create predefined positions for a Party of up to 8 Players to move to.");
            ImGui.Dummy(new Vector2(0, 5));
            ImGui.TextWrapped("Presets can have up to 8 Player Character members, but can also have any target as a member.");
            ImGui.Dummy(new Vector2(0, 5));
            ImGui.TextWrapped("Positional Presets require an Anchor Member for them to work, this is the player that everyone else moves around.");
            ImGui.Dummy(new Vector2(0, 5));
            ImGui.TextWrapped("Each Preset Member can have their own specific emote chosen in their Member Properties.");
            ImGui.Dummy(new Vector2(0, 5));
            ImGui.TextWrapped("Positional Presets have two actions, one to take the members to their places and one to start their emotes.");
            ImGui.Dummy(new Vector2(0, 10));
            ImGui.Text("Macros :");
            ImGui.Dummy(new Vector2(0, 10));
            ImGui.Text("Macros can be used to initiate a single Emote or chain together multiple Emotes.");
            ImGui.Dummy(new Vector2(0, 10));
            ImGui.Text("Single Emote Example :");
            ImGui.Dummy(new Vector2(0, 10));
            var example =
                "/micon hum Emote\n" +
                "/tell <t> Simon Says : hum\n" +
                "/hum\n";
            ImGui.InputTextMultiline("##Example", ref example, 200, new Vector2(200, 75), ImGuiInputTextFlags.ReadOnly);
            ImGui.Dummy(new Vector2(0, 10));
            ImGui.Text("Multiple Emote Example :");
            ImGui.Dummy(new Vector2(0, 10));
            var multiexample =
                "/micon hum Emote\n" +
                "/tell <t> Simon Says : hum\n" +
                "/hum\n" +
                "/wait 3\n" +
                "/tell <t> Simon Says : dance\n" +
                "/dance\n";
            ImGui.InputTextMultiline("##MultiExample", ref multiexample, 200, new Vector2(200, 125), ImGuiInputTextFlags.ReadOnly);
        }

        ImGui.EndChild();
    }
}
