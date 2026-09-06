using System.Diagnostics;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using SimonSays.ImGuiMethods;

namespace SimonSays.Windows.Tabs;

internal class About
{
    public static void Draw()
    {
        var windowWidth = ImGui.GetWindowWidth();
        var windowHeight = ImGui.GetWindowHeight();
        var aboutSize = new Vector2(windowWidth / 1.015f, windowHeight / 1.2f);

        ImGui.NewLine();
        if (ImGui.BeginChild("AboutBox", aboutSize, true, ImGuiWindowFlags.NoCollapse))
        {
            ImGui.Text(string.Empty);
            ImGuiEx.ImGuiLineCentered("AboutVersion", () =>
            {
                ImGui.Text("SimonSays - " + typeof(Potatoes).Assembly.GetName().Version?.ToString());
            });
            ImGui.Text(string.Empty);
            ImGuiEx.ImGuiLineCentered("AboutCreators", () =>
            {
                ImGui.Text("Created by Gerry of Ravine & Frdhog @ Triquetra Studios");
            });
            ImGui.Text(string.Empty);
            if (ConfigWindowHelpers.aboutImage != null)
            {
                ImGuiEx.ImGuiLineCentered("AboutImage", () =>
                {
                    if (ConfigWindowHelpers.aboutImage.TryGetWrap(out var texture, out _))
                    {
                        ImGui.Image(texture.Handle, new Vector2(300, 300));
                    }
                });
            }

            ImGui.Text(string.Empty);
            ImGuiEx.ImGuiLineCentered("AboutDiscord", () =>
            {
                ImGui.Text("Join our Discord to stay up to date with releases, updates & more!");
            });
            ImGui.Text(string.Empty);
            ImGuiEx.ImGuiLineCentered("AboutButtons", () =>
            {
                if (ImGui.Button("Discord"))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = Potatoes.DiscordURL,
                        UseShellExecute = true,
                    });
                }

                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                {
                    ImGui.SetTooltip("Join our Discord!");
                }

                ImGui.SameLine();
                if (ImGui.Button("Repo"))
                {
                    ImGui.SetClipboardText(Potatoes.Repo);
                }

                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                {
                    ImGui.SetTooltip("Copy repo to Clipboard.");
                }

                ImGui.SameLine();
                if (ImGui.Button("Source"))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = Potatoes.Source,
                        UseShellExecute = true,
                    });
                }

                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                {
                    ImGui.SetTooltip("Take a peak under the hood.");
                }

                ImGui.SameLine();
                if (ImGui.Button("Buy me a coffee!"))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = Potatoes.BuyMeACoffee,
                        UseShellExecute = true,
                    });
                }

                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                {
                    ImGui.SetTooltip("Only if you want to!");
                }
            });
        }

        ImGui.EndChild();
    }
}
