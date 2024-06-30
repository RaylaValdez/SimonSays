using ImGuiNET;
using PunishLib.ImGuiMethods;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimonSays;
namespace SimonSays.Windows.Tabs
{
    internal class About
    {
        public static void Draw()
        {
            var WindowWidth = ImGui.GetWindowWidth();
            var WindowHeight = ImGui.GetWindowHeight();
            var AboutSize = new System.Numerics.Vector2(WindowWidth / 1.015f, WindowHeight / 1.2f);
            ImGui.NewLine();
            if (ImGui.BeginChild("AboutBox", AboutSize, true, ImGuiWindowFlags.NoCollapse))
            {
                ImGui.Text("");
                ImGuiEx.ImGuiLineCentered("AboutVersion", () =>
                {
                    ImGui.Text("SimonSays - " + typeof(Potatoes).Assembly.GetName().Version?.ToString());
                });
                ImGui.Text("");
                ImGuiEx.ImGuiLineCentered("AboutCreators", () =>
                {
                    ImGui.Text("Created by Rayla & Frdhog @ Triquetra Studios");
                });
                ImGui.Text("");
                if (ConfigWindowHelpers.aboutImage != null)
                {
                    ImGuiEx.ImGuiLineCentered("AboutImage", () =>
                    {
                        if (ConfigWindowHelpers.aboutImage.TryGetWrap(out var texture, out var exception))
                        {
                            ImGui.Image(texture.ImGuiHandle, new System.Numerics.Vector2(300, 300));
                        }

                    });
                }
                ImGui.Text("");
                ImGuiEx.ImGuiLineCentered("AboutDiscord", () =>
                {
                    ImGui.Text("Join our Discord to stay up to date with releases, updates & more!");
                });
                ImGui.Text("");
                ImGuiEx.ImGuiLineCentered("AboutButtons", () =>
                {
                    if (ImGui.Button("Discord"))
                    {
                        Process.Start(new ProcessStartInfo()
                        {
                            FileName = Potatoes.DiscordURL,
                            UseShellExecute = true
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

                        //Process.Start(new ProcessStartInfo()
                        //{
                        //    FileName = Plugin.Repo,
                        //    UseShellExecute = true
                        //});
                    }
                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                    {
                        ImGui.SetTooltip("Copy repo to Clipboard.");
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Source"))
                    {
                        Process.Start(new ProcessStartInfo()
                        {
                            FileName = Potatoes.Source,
                            UseShellExecute = true
                        });
                    }
                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                    {
                        ImGui.SetTooltip("Take a peak under the hood.");
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Buy me a coffee!"))
                    {
                        Process.Start(new ProcessStartInfo()
                        {
                            FileName = Potatoes.BuyMeACoffee,
                            UseShellExecute = true
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
}
