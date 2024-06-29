using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Dalamud.Game.Text;
using PunishLib.ImGuiMethods;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Textures;
using Vector2 = System.Numerics.Vector2;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using Vector4 = System.Numerics.Vector4;
using SimonSays.Helpers;
using Dalamud.Interface.ImGuiNotification;
using ImPlotNET;


namespace SimonSays.Windows;

/// <summary>
/// Represents a window for configuring settings.
/// </summary>
/// <remarks>
/// This class inherits from the Window class and implements the IDisposable interface.
/// </remarks>
public class ConfigWindow : Window, IDisposable
{
    private Configuration configuration;
    private string testText = "Simon Says : hum";
    private ISharedImmediateTexture aboutImage;

    private static readonly bool EnableDebug = false;

    private Vector2 contextMenuPosition;
    private Vector2 contextMenuSize = new Vector2(135, 34);
    private bool isFirstFrame = true;

    public static string selectedLayout = String.Empty;
    public static string selectedMember = String.Empty;
    public static bool namingWindowOpen = false;
    public static bool renamingWindowOpen = false;
    public static bool newMemberWindowOpen = false;
    public static bool contextPopupOpen = false;
    private const int BufferSize = 1024;
    public static string nameBuffer = "Change Me";
    public static string filterText = String.Empty;
    public static string renameBuffer = String.Empty;

    public static Vector4 oldTitleColorActive = Vector4.Zero;

    public static Preset? activePreset = null;

    public static List<Vector4> memberColors = new List<Vector4>
    {
        new Vector4(0.722f, 0.325f, 0.623f, 1.0f),
        new Vector4(0.7051666666666666f, 0.335625f, 0.605875f, 1.0f),
        new Vector4(0.6883333333333332f, 0.34625f, 0.5887499999999999f, 1.0f),
        new Vector4(0.6715f, 0.356875f, 0.5716249999999999f, 1.0f),
        new Vector4(0.6546666666666667f, 0.36750000000000005f, 0.5545f, 1.0f),
        new Vector4(0.6378333333333334f, 0.37812500000000004f, 0.537375f, 1.0f),
        new Vector4(0.621f, 0.38875f, 0.52025f, 1.0f),
        new Vector4(0.6041666666666666f, 0.399375f, 0.503125f, 1.0f),
        new Vector4(0.5873333333333334f, 0.41000000000000003f, 0.48600000000000004f, 1.0f),
        new Vector4(0.5705f, 0.42062499999999997f, 0.46887500000000004f, 1.0f),
        new Vector4(0.5536666666666665f, 0.43125f, 0.45174999999999993f, 1.0f),
        new Vector4(0.5368333333333333f, 0.44187499999999996f, 0.434625f, 1.0f),
        new Vector4(0.52f, 0.4525f, 0.4175f, 1.0f),
        new Vector4(0.5031666666666667f, 0.463125f, 0.40037500000000004f, 1.0f),
        new Vector4(0.4863333333333334f, 0.47374999999999995f, 0.38325000000000004f, 1.0f),
        new Vector4(0.46950000000000003f, 0.484375f, 0.36612500000000003f, 1.0f),
        new Vector4(0.4526666666666668f, 0.49499999999999994f, 0.3490000000000001f, 1.0f),
        new Vector4(0.4358333333333334f, 0.505625f, 0.33187500000000003f, 1.0f),
        new Vector4(0.4190000000000001f, 0.5162499999999999f, 0.3147500000000001f, 1.0f),
        new Vector4(0.4021666666666668f, 0.526875f, 0.29762500000000014f, 1.0f),
        new Vector4(0.3853333333333334f, 0.5374999999999999f, 0.28050000000000014f, 1.0f),
        new Vector4(0.3685000000000001f, 0.5481249999999999f, 0.26337500000000014f, 1.0f),
        new Vector4(0.3516666666666668f, 0.5587499999999999f, 0.24625000000000014f, 1.0f),
        new Vector4(0.3348333333333335f, 0.5693749999999999f, 0.22912500000000016f, 1.0f)

    };




    public ConfigWindow(Plugin plugin) : base(
        "SimonSays Settings",
        ImGuiWindowFlags.NoScrollbar)
    {

        this.Size = new System.Numerics.Vector2(500, 900);
        this.SizeCondition = ImGuiCond.FirstUseEver;

        this.configuration = Plugin.Configuration!;

        var imagePath = Path.Combine(Plugin.PluginInterfaceStatic!.AssemblyLocation.Directory?.FullName!, "ts500.png");
        aboutImage = Service.TextureProvider.GetFromFile(imagePath);
    }

    public void Dispose() { }

    /// <summary>
    /// Draws a checkbox for a chat channel at the specified index.
    /// </summary>
    /// <param name="index">The index of the chat channel.</param>
    private void DrawCheckbox(int index)
    {
        // Get the key and value of the chat channel at the specified index
        var key = ChatChannelTypes.ChatTypes.Keys.ToList()[index];
        var Value = ChatChannelTypes.ChatTypes[key];
        int channel = key;

        // Get the previous configuration value for the channel
        bool prevVal = false;
        configuration.EnabledChannels.TryGetValue(channel, out prevVal);

        // Draw the checkbox and update the configuration if the value changes
        bool newVal = prevVal;
        ImGui.TableNextColumn();
        if (ImGui.Checkbox(Value, ref newVal))
        {
            configuration.EnabledChannels[channel] = newVal;
            this.configuration.Save();
        }
    }

    // Method to draw the "Enabled" checkbox
    /// <summary>
    /// Draws the "Enabled" checkbox and updates the IsListening property in the configuration based on the user's selection.
    /// </summary>
    private void DrawEnabled()
    {
        // Get the current value of the IsListening property from the configuration
        bool isListening = configuration.IsListening;

        // Draw the "Enabled" checkbox and update the isListening variable with the new value
        if (ImGui.Checkbox("Enabled", ref isListening))
        {
            // Update the IsListening property in the configuration with the new value
            configuration.IsListening = isListening;

            // Save the updated configuration
            this.configuration.Save();
        }
    }

    /// <summary>
    /// Draws checkboxes for each channel in a table format.
    /// </summary>
    private void DrawCheckboxes()
    {
        // Begin the table with a unique identifier and specify the number of columns and table flags
        ImGui.BeginTable("simonsaystable", 3, ImGuiTableFlags.Borders);

        // Set up the column headers
        ImGui.TableSetupColumn("Channels");
        ImGui.TableSetupColumn("");
        ImGui.TableSetupColumn("");
        ImGui.TableHeadersRow();

        // Get the list of chat types
        var ChatTypes = ChatChannelTypes.ChatTypes;

        // Calculate the maximum number of rows in the table
        int max = (int)Math.Ceiling(ChatTypes.Count / 3d);

        // Loop through each row in the table
        for (int i = 0; i < Math.Ceiling(ChatTypes.Count / 3d); i++) // row loop
        {
            // Loop through each column in the table
            for (int j = 0; j < 3; j++) // column loop
            {
                // Check if the current index is out of bounds
                if ((j * max) + i > ChatTypes.Count - 1)
                    continue; // Move to the next iteration

                // Calculate the index of the current checkbox
                int index = (j * max) + i;

                // Draw the checkbox for the current channel
                DrawCheckbox(index);
            }
        }

        // End the table
        ImGui.EndTable();
    }

    /// <summary>
    /// Draws a text box for the catch phrase and allows the user to modify it. 
    /// The catch phrase is retrieved from the configuration and updated if modified.
    /// The updated configuration is then saved.
    /// </summary>
    private void DrawCatchPhBox()
    {
        // Get the current catch phrase from the configuration
        string inputText = configuration.CatchPhrase;

        // Draw the input text box and check if the text has been modified
        if (ImGui.InputText("", ref inputText, 500U))
        {
            // Trim the input text and update the catch phrase in the configuration
            configuration.CatchPhrase = inputText.Trim();

            // Save the updated configuration
            this.configuration.Save();
        }
    }

    /// <summary>
    /// Draws miscellaneous options in the ImGui window, including a checkbox for motion only, an input text field, and a send button.
    /// </summary>
    private void DrawMiscOptions()
    {
        // Get the value of the MotionOnly property from the configuration object
        bool motionOnly = configuration.MotionOnly;

        // Display a checkbox in the ImGui window with the label "Motion Only" and bind it to the motionOnly variable
        // If the checkbox value is changed, the motionOnly variable will be updated
        if (ImGui.Checkbox("Motion Only", ref motionOnly))
        {
            // Update the MotionOnly property in the configuration object with the new value
            configuration.MotionOnly = motionOnly;

            // Save the updated configuration
            this.configuration.Save();
        }

        // Display an input text field in the ImGui window with the label "Test Input" and bind it to the testText variable
        ImGui.InputText("Test Input", ref testText, 500U);

        // Display a button in the ImGui window with the label "Send"
        // If the button is clicked, execute the following code
        if (ImGui.Button("Send"))
        {
            // Call the Meat.Command method with the XivChatType.None parameter, the value of the testText variable, and ForceForTesting set to true
            Meat.Command(XivChatType.None, testText, ForceForTesting: true);
        }
    }

    /// <summary>
    /// Draws checkboxes for the experiment.
    /// </summary>
    private void DrawExperimentCheckboxes()
    {
        // Get the value of the PosSync property from the configuration object
        bool posSync = configuration.PosSync;

        // Display a checkbox labeled "Positional Sync" and bind its value to the posSync variable
        if (ImGui.Checkbox("Positional Sync", ref posSync))
        {
            // If the checkbox value is changed, update the PosSync property in the configuration object
            configuration.PosSync = posSync;
            // Save the updated configuration
            this.configuration.Save();
        }

        // Display a colored text indicating that Position Sync requires a Target
        ImGui.TextColored(new System.Numerics.Vector4(160, 160, 160, 0.8f), "Position Sync requires a Target.");

        // If PosSync is true, display buttons for syncing position and stopping sync
        if (configuration.PosSync)
        {
            if (ImGui.Button("Sync Position"))
            {
                // Call the StartScooch method to sync the position
                Meat.StartScooch();
            }
            if (ImGui.Button("Stop Sync"))
            {
                // Call the StopScooch method to stop syncing the position
                Meat.StopScooch();
            }
        }
    }

    /// <summary>
    /// Draws debug positional information on the screen.
    /// </summary>
    private void DrawDebugPositionalInformation()
    {
        // Check if the movement object is null
        if (Meat.movement == null)
            return;

        // Display whether SoftDisable is true or false
        ImGui.Text($"SoftDisable: {(Meat.movement.SoftDisable ? "True" : "False")}");

        // Display the distance to the target
        ImGui.Text($"Distance to target: {MathF.Sqrt(Meat.movement.DistanceSquared)}");

        // Display the rotation distance in degrees
        ImGui.Text($"Rotation distance (deg): {Meat.movement.RotationDistance}");

        // Get the current time
        DateTime currTime = DateTime.Now;

        // Display the time since the last movement
        ImGui.Text($"Time since last moved: {(currTime - Meat.movement.LastTimeMoved).TotalSeconds}");

        // Display the time since the last turn
        ImGui.Text($"Time since last turned: {(currTime - Meat.movement.LastTimeTurned).TotalSeconds}");

        // Add spacing between sections
        ImGui.Spacing();

        // Get the target position and rotation
        var tarPos = Meat.movement.DesiredPosition;
        float tarRot = Meat.movement.DesiredRotation;

        // Convert the rotation to degrees
        tarRot = Helpers.AngleConversions.ToDeg(tarRot);

        // Display the target destination
        ImGui.Text("Target Destination:");
        ImGui.Text($"X: {tarPos.X} Y: {tarPos.Y} Z: {tarPos.Z}");
        ImGui.Text($"Angle: {tarRot}");

        // Add spacing between sections
        ImGui.Spacing();

        // Display the last forward, left, and turn left values
        ImGui.Text($"Last Forward: {Meat.movement.LastForward}");
        ImGui.Text($"Last Left: {Meat.movement.LastLeft}");
        ImGui.Text($"Last Turn Left: {Meat.movement.LastTurnLeft}");
    }

    public string SearchedEmoteFilter = string.Empty;

    public override void PreDraw()
    {
        ImGuiStylePtr StylePtr = ImGui.GetStyle();
        oldTitleColorActive = StylePtr.Colors[(int)ImGuiCol.TitleBgActive];

        StylePtr.Colors[(int)ImGuiCol.TitleBgActive] = (new Vector4(081, 054, 148, 211) / 255f);

        base.PreDraw();
    }

    /// <summary>
    /// Overrides the Draw method from the base class to define custom drawing behavior.
    /// </summary>
    public override void Draw()
    {
        // Custom Style
        // Variables
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10, 10));
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(4, 4));
        ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(4, 4));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4, 4));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemInnerSpacing, new Vector2(4, 4));
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 21f);
        ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarSize, 21f);
        ImGui.PushStyleVar(ImGuiStyleVar.GrabMinSize, 21f);
        // Borders
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, 1f);
        ImGui.PushStyleVar(ImGuiStyleVar.PopupBorderSize, 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0f);
        // Rounding
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 6f);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 4f);
        ImGui.PushStyleVar(ImGuiStyleVar.PopupRounding, 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarRounding, 9f);
        ImGui.PushStyleVar(ImGuiStyleVar.GrabRounding, 3f);
        ImGui.PushStyleVar(ImGuiStyleVar.TabRounding, 4f);
        // Alignment
        ImGui.PushStyleVar(ImGuiStyleVar.WindowTitleAlign, new Vector2(0.0f, 0.50f));
        ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new Vector2(0.5f, 0.5f));
        ImGui.PushStyleVar(ImGuiStyleVar.SelectableTextAlign, new Vector2(0, 0));
        // Colors
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, (new Vector4(081, 054, 148, 211)) / 255f);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, (new Vector4(130, 068, 153, 241)) / 255f);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, (new Vector4(184, 083, 159, 241)) / 255f);
        ImGui.PushStyleColor(ImGuiCol.Separator, (new Vector4(130, 068, 153, 241)) / 255f);
        ImGui.PushStyleColor(ImGuiCol.SeparatorHovered, (new Vector4(130, 068, 153, 200)) / 255f);
        ImGui.PushStyleColor(ImGuiCol.SeparatorActive, (new Vector4(184, 083, 159, 241)) / 255f);
        ImGui.PushStyleColor(ImGuiCol.ResizeGripActive, (new Vector4(184, 083, 159, 241)) / 255f);
        ImGui.PushStyleColor(ImGuiCol.TabHovered, (new Vector4(130, 068, 153, 200)) / 255f);
        ImGui.PushStyleColor(ImGuiCol.TabActive, (new Vector4(184, 083, 159, 241)) / 255f);
        ImGui.PushStyleColor(ImGuiCol.Border, (new Vector4(0.35f, 0.35f, 0.35f, 0.75f)));
        ImGui.PushStyleColor(ImGuiCol.FrameBg, (new Vector4(50, 46, 51, 122) / 255f));



        OpenReNamingWindow();
        OpenNamingWindow();
        ContextPopup();
        ImGui.Text("Enable or Disable listening to channels.");
        DrawEnabled();
        if (ImGui.BeginTabBar("Tabs"))
        {
            if (ImGui.BeginTabItem("Settings"))
            {
                ImGui.Text("");
                ImGui.Text("Catchphrase - This is what SimonSays listens to.");
                DrawCatchPhBox();
                ImGui.TextColored(new System.Numerics.Vector4(160, 160, 160, 0.8f), "Your Catchprase IS your security, change this to control who can command you.");
                ImGui.TextColored(new System.Numerics.Vector4(160, 160, 160, 0.8f), "Only share this to people you want to be able to command you.");
                ImGui.Text("");
                ImGui.Text("Channels - These are the channels SimonSays will listen to, waiting for your catchphrase.");
                DrawCheckboxes();
                ImGui.Text("");
                ImGui.Separator();
                ImGui.Text("");
                ImGui.Text("Misc Options");
                DrawMiscOptions();
                ImGui.Text("");
                ImGui.Separator();
                ImGui.Text("");
                ImGui.Text("Experimental Features");
                DrawExperimentCheckboxes();
                ImGui.EndTabItem();
            }
            if (configuration.PosSync && ImGui.BeginTabItem("Positional Presets"))
            {
                OffsetsTab();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Usage"))
            {
                ImGui.Text("");
                ImGui.Text("Commands :");
                ImGui.Text("");
                ImGui.Text("/simonsaysconfig - Will open this window.");
                ImGui.Text("");
                ImGui.Text("/sync - Will attempt to sync positions with your Target, only works if Position Syncing is enabled in Experimental Features.");
                ImGui.Text("");
                ImGui.Text("/simonsays <TheirEmote> <YourEmote> <ShouldSync> - There are multiple ways to use this command.");
                ImGui.Text("Example 1 - '/simonsays hum' - This will make you and the person you're targetting hum.");
                ImGui.Text("Example 2 - '/simonsays hum dance' - This will make the person you're targetting hum and make yourself dance.");
                ImGui.Text("Example 3 - '/simonsays hum dance true' - This will attempt to sync your positions prior to making your target hum and yourself dance.");
                ImGui.Text("ShouldSync being 'true' in this case can be used on all previous examples.");
                ImGui.Text("");
                ImGui.Separator();
                ImGui.Text("");
                ImGui.Text("Macros :");
                ImGui.Text("");
                ImGui.Text("Macros can be used to initiate a single Emote or chain together multiple Emotes.");
                ImGui.Text("");
                ImGui.Text("Single Emote Example :");
                ImGui.Text("");
                string example =
                    "/micon hum Emote\n" +                    "/tell <t> Simon Says : hum\n" +                    "/hum\n";
                ImGui.InputTextMultiline("##Example", ref example, 200, new(200, 75), ImGuiInputTextFlags.ReadOnly);
                ImGui.Text("");
                ImGui.Text("Multiple Emote Example :");
                ImGui.Text("");
                string multiexample =
                    "/micon hum Emote\n" +                    "/tell <t> Simon Says : hum\n" +                    "/hum\n" +
                    "/wait 3\n" +
                    "/tell <t> Simon Says : dance\n" +
                    "/dance\n";
                ImGui.InputTextMultiline("##MultiExample", ref multiexample, 200, new(200, 125), ImGuiInputTextFlags.ReadOnly);
                ImGui.Text("");
                ImGui.Separator();
                ImGui.Text("");
                ImGui.Text("Experimental Features :");
                ImGui.Text("");
                ImGui.Text("Positional Syncing : ");
                ImGui.Text("");
                ImGui.Text("Positional Syncing is a way to align perfectly with your Target.");
                ImGui.Text("Enable it in the Settings Tab.");
                ImGui.Text("Simply Target the person you wish to sync positions with and use the /sync Command.");
                ImGui.Text("Should you fail to sync and end up running around your Target, use the stop sync button in the Settings tab.");
                ImGui.EndTabItem();
            }

            if (EnableDebug && ImGui.BeginTabItem("Debug"))
            {
                DrawDebugPositionalInformation();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("About"))
            {
                ImGui.Text("");
                ImGuiEx.ImGuiLineCentered("AboutVersion", () =>
                {
                    ImGui.Text("SimonSays - " + typeof(Plugin).Assembly.GetName().Version?.ToString());
                });
                ImGui.Text("");
                ImGuiEx.ImGuiLineCentered("AboutCreators", () =>
                {
                    ImGui.Text("Created by Rayla & Frdhog @ Triquetra Studios");
                });
                ImGui.Text("");
                if (aboutImage != null)
                {
                    ImGuiEx.ImGuiLineCentered("AboutImage", () =>
                    {
                        if (aboutImage.TryGetWrap(out var texture,out var exception))
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
                            FileName = Plugin.DiscordURL,
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
                        ImGui.SetClipboardText(Plugin.Repo);

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
                            FileName = Plugin.Source,
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
                            FileName = Plugin.BuyMeACoffee,
                            UseShellExecute = true
                        });
                    }
                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                    {
                        ImGui.SetTooltip("Only if you want to!");
                    }
                });


                ImGui.EndTabItem();
            }
            ImGui.EndTabBar();
        }

        ImGui.PopStyleVar(22);
        ImGui.PopStyleColor(9);
    }

    public override void PostDraw()
    {
        ImGuiStylePtr StylePtr = ImGui.GetStyle();

        StylePtr.Colors[(int)ImGuiCol.TitleBgActive] = oldTitleColorActive;


        base.PostDraw();
    }

    public static bool IconButtonWithText(FontAwesomeIcon icon, string text, string tooltip)
    {
        ImGui.PushID(text);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0.0f));
        bool selected = ImGuiComponents.IconButton(icon); // <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< Imgui icon button here
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(tooltip);
        }

        if (!text.IsNullOrEmpty())
        {
            ImGui.SameLine();
            selected |= ImGui.Button(text);
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(tooltip);
            }
        }
        ImGui.PopStyleVar();
        ImGui.PopID();

        return selected;
    }

    public static void OffsetsTab()
    {
        float WindowWidth = ImGui.GetWindowWidth();
        float WindowHeight = ImGui.GetWindowHeight();

        Vector2 LeftSize = new Vector2(WindowWidth / 4.3f, WindowHeight / 1.5f);
        Vector2 LayoutsChildSize = new Vector2(-1, LeftSize.Y / 2.5f);
        Vector2 LayoutsSize = new Vector2(-1, -1);
        Vector2 MembersSize = new Vector2(-1, -1);
        Vector2 MiddleSize = new Vector2(WindowWidth / 2, WindowHeight / 1.5f);
        Vector2 RightSize = new Vector2(WindowWidth / 4.3f, WindowHeight / 1.5f);

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
                        if (IconButtonWithText(FontAwesomeIcon.Plus, "", "Create new Preset"))
                        {
                            ImGui.SetNextWindowPos(new Vector2(10.0f, 100.0f));
                            ImGui.SetNextWindowSize(new Vector2(200.0f, 60.0f));
                            namingWindowOpen = true;
                        }

                        ImGui.SameLine();
                        ImGui.Dummy(new Vector2(1, 0));
                        ImGui.SameLine();
                        if (IconButtonWithText(FontAwesomeIcon.Minus, "", "Delete selected Preset"))
                        {
                            File.Delete(Plugin.PresetDirectory + "/" + selectedLayout + ".json");
                            selectedLayout = String.Empty;
                            activePreset = null;
                        }
                        ImGui.SameLine();
                        ImGui.Dummy(new Vector2(1, 0));
                        ImGui.SameLine();
                        if (IconButtonWithText(FontAwesomeIcon.Clipboard, "", "Copy Preset to Clipboard"))
                        {
                            if (!string.IsNullOrEmpty(selectedLayout))
                            {
                                var presetContent = File.ReadAllLines(Plugin.PresetDirectory + "/" + selectedLayout + ".json");
                                string contentToCopy = string.Join(Environment.NewLine, presetContent);

                                // Copy to clipboard
                                ImGui.SetClipboardText(contentToCopy);

                                Notification clipboardsuccess = new Notification();
                                clipboardsuccess.Content = "Preset copied to Clipboard.";

                                Service.NotificationManager.AddNotification(clipboardsuccess);
                            }
                            else
                            {
                                Notification clipboardfailure = new Notification();
                                clipboardfailure.Content = "Select a preset to copy to Clipboard.";

                                Service.NotificationManager.AddNotification(clipboardfailure);
                            }
                        }
                        ImGui.SameLine();
                        ImGui.Dummy(new Vector2(1, 0));
                        ImGui.SameLine();
                        bool importInProgress = false; // Flag to track import progress

                        if (IconButtonWithText(FontAwesomeIcon.FileImport, "", "Import Preset from Clipboard"))
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

                                    // Deserialize preset content
                                    activePreset = JsonSerializer.Deserialize<Preset>(presetContent) ?? new Preset();
                                    selectedLayout = activePreset.PresetName;

                                    // Initialize Members if null
                                    if (activePreset.Members == null)
                                    {
                                        activePreset.Members = [];
                                    }

                                    // Check if PresetName is valid
                                    if (!string.IsNullOrEmpty(activePreset.PresetName))
                                    {
                                        // Serialize and write to file
                                        var jsonString = JsonSerializer.Serialize(activePreset, new JsonSerializerOptions { WriteIndented = true });
                                        File.WriteAllText(Plugin.PresetDirectory + "/" + activePreset.PresetName + ".json", jsonString);
                                        
                                    }
                                    else
                                    {
                                        throw new Exception("PresetName is null or empty");
                                    }
                                }
                                catch (Exception)
                                {
                                    Notification importFailure = new Notification();
                                    importFailure.Content = "You can only import valid Presets";
                                    Service.NotificationManager.AddNotification(importFailure);
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

                using (var Layouts = ImRaii.ListBox("##", LayoutsSize))
                {
                    if (Layouts.Success)
                    {
                        foreach (var file in Directory.EnumerateFiles(Plugin.PresetDirectory))
                        {
                            var fileName = Path.GetFileNameWithoutExtension(file);
                            if (ImGui.Selectable(fileName, selectedLayout == fileName))
                            {
                                selectedLayout = fileName;

                                var presetContent = File.ReadAllLines(Plugin.PresetDirectory + "/" + selectedLayout + ".json");
                                string jsonString = string.Join(Environment.NewLine, presetContent);



                                activePreset = JsonSerializer.Deserialize<Preset>(jsonString) ?? new Preset();
                                if (activePreset.Members == null)
                                {
                                    activePreset.Members = new();
                                }

                            }

                            if (selectedLayout == fileName)
                            {
                                if (ImGui.IsItemHovered())
                                {
                                    if (ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                                    {
                                        Service.Log.Debug("Opening context popup");

                                        contextPopupOpen = true;
                                    }
                                }
                            }
                        }

                    }

                }
            }
            ImGui.EndChild();
            if (ImGui.BeginChild("members", MembersSize, true, ImGuiWindowFlags.NoCollapse))
            {
                if (!selectedLayout.IsNullOrEmpty())
                {
                    if (ImGui.BeginChild("memberbuttons", new Vector2(-1, ImGui.GetTextLineHeight() * 2.3f), false, ImGuiWindowFlags.NoCollapse))
                    {
                        ImGui.Text("Members");
                        ImGui.SameLine();
                        ImGuiEx.ImGuiLineRightAlign("MembersButtons", () =>
                        {
                            if (IconButtonWithText(FontAwesomeIcon.StreetView, "", "Add targetted Character."))
                            {
                                IGameObject? target = Service.TargetManager.Target;
                                if (target != null)
                                {
                                    if (activePreset != null)
                                    {
                                        activePreset.Members!.Add(new PresetMember(target.Name.ToString()));
                                        string jsonString = JsonSerializer.Serialize(activePreset, new JsonSerializerOptions { WriteIndented = true });
                                        File.WriteAllText(Plugin.PresetDirectory + "/" + selectedLayout + ".json", jsonString);
                                    }
                                }

                            }
                            ImGui.SameLine();
                            ImGui.Dummy(new Vector2(1, 0));
                            ImGui.SameLine();

                            if (IconButtonWithText(FontAwesomeIcon.Minus, "", "Remove Selected Member"))
                            {
                                if (activePreset != null)
                                {
                                    // activePreset.Members.Remove(selectedMember);
                                    activePreset.Members = activePreset.Members.Where((member) => member.CharacterName != selectedMember).ToList(); // select every member that doesn't have the same character name
                                    string jsonString = JsonSerializer.Serialize(activePreset, new JsonSerializerOptions { WriteIndented = true });
                                    File.WriteAllText(Plugin.PresetDirectory + "/" + selectedLayout + ".json", jsonString);
                                }
                            }
                        });
                    }
                    ImGui.EndChild();

                    using (var Members = ImRaii.ListBox("##", MembersSize))
                    {
                        if (Members.Success)
                        {
                            if (activePreset != null)
                            {
                                if (activePreset.Members != null)
                                {
                                    foreach (var member in activePreset.Members.ToList())
                                    {
                                        if (ImGui.Selectable(member.CharacterName, selectedMember == member.CharacterName))
                                        {
                                            selectedMember = member.CharacterName;
                                        }
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
            if (selectedLayout != null)
            {
                double minX = -10;
                double maxX = 10;
                double minY = -10;
                double maxY = 10;
                ImPlot.SetNextAxesLimits(minX, maxX, minY, maxY, ImPlotCond.Always);
                if (ImPlot.BeginPlot(selectedLayout, new Vector2(-1, -1), ImPlotFlags.None | ImPlotFlags.NoLegend))
                {
                    if (activePreset != null)
                    {
                        if (activePreset.Members != null)
                        {
                            ImPlotPoint mousePos = ImPlot.GetPlotMousePos();


                            int colorIndex = 0;
                            foreach (var member in activePreset.Members.ToList())
                            {
                                double X = member.X;
                                double Y = member.Y;
                                float rot = member.ROT;

                                // Ensure colorIndex does not exceed the bounds of memberColors
                                if (colorIndex >= memberColors.Count)
                                {
                                    colorIndex = 0; // Reset colorIndex if it exceeds the list size
                                }

                                if (ImPlot.DragPoint((int)ImGui.GetID(member.CharacterName), ref X, ref Y, memberColors[colorIndex], 15f))
                                {
                                    selectedMember = member.CharacterName;
                                }
                                if (ImGui.IsItemHovered() && ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                                {
                                    // where a is [x,y] and b is [mouseX, mouseY] : (a squared) + (y squared) = vector magnitude aka distance
                                    double distance = ((X - mousePos.x) * (X - mousePos.x)) + ((Y - mousePos.y) * (Y - mousePos.y));

                                    if (distance < 0.15f)
                                    {
                                        selectedMember = member.CharacterName;
                                        SendNotification("Selected : " + selectedMember);
                                    }
                                }
                                string initials = string.Join("", member.CharacterName.Split(" ").SelectMany(s => s.FirstOrDefault().ToString()));

                                ImPlot.PlotText(initials, X, Y);

                                // Calculate the end point of the arrow based on the rotation angle
                                float arrowLength = 1f; // Length of the arrow
                                float rotOffset = -rot + MathF.PI / 2;
                                float endX = (float)(X + arrowLength * Math.Cos(rotOffset));
                                float endY = (float)(Y + arrowLength * Math.Sin(rotOffset));

                                // Prepare the points for PlotLine
                                float[] xs = new float[] { (float)X, endX };
                                float[] ys = new float[] { (float)Y, endY };

                                ImPlot.SetupLegend(ImPlotLocation.NorthWest, ImPlotLegendFlags.NoMenus);
                                // Plot the arrow to show the rotation
                                ImPlot.SetNextLineStyle(memberColors[colorIndex]);
                                ImPlot.PlotLine(member.CharacterName, ref xs[0], ref ys[0], 2);

                                // Calculate the points for the arrowhead
                                float arrowheadLength = 0.25f; // Length of the arrowhead lines
                                float angleOffset = (125f).Degrees().Rad;

                                float leftX = (float)(endX + arrowheadLength * Math.Cos(rotOffset + angleOffset));
                                float leftY = (float)(endY + arrowheadLength * Math.Sin(rotOffset + angleOffset));

                                float rightX = (float)(endX + arrowheadLength * Math.Cos(rotOffset - angleOffset));
                                float rightY = (float)(endY + arrowheadLength * Math.Sin(rotOffset - angleOffset));

                                float[] leftArrowXs = new float[] { endX, leftX };
                                float[] leftArrowYs = new float[] { endY, leftY };

                                float[] rightArrowXs = new float[] { endX, rightX };
                                float[] rightArrowYs = new float[] { endY, rightY };

                                // Plot the arrowhead
                                ImPlot.SetNextLineStyle(memberColors[colorIndex]);
                                ImPlot.PlotLine(member.CharacterName + "_leftArrowhead", ref leftArrowXs[0], ref leftArrowYs[0], 2);
                                ImPlot.SetNextLineStyle(memberColors[colorIndex]);
                                ImPlot.PlotLine(member.CharacterName + "_rightArrowhead", ref rightArrowXs[0], ref rightArrowYs[0], 2);

                                member.X = X;
                                member.Y = Y;

                                string jsonString = JsonSerializer.Serialize(activePreset, new JsonSerializerOptions { WriteIndented = true });
                                File.WriteAllText(Plugin.PresetDirectory + "/" + selectedLayout + ".json", jsonString);

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
            if (ImGui.BeginChild("Properties", new Vector2(-1,ImGui.GetWindowHeight()-RightSize.Y / 2.29f), true, ImGuiWindowFlags.NoCollapse))
            {
                if (!selectedMember.IsNullOrEmpty())
                {
                    PresetMember member = activePreset?.Members?.FirstOrDefault(((m) => m.CharacterName == selectedMember)) ?? new PresetMember();


                    float x = (float)member.X;
                    float y = (float)member.Y;
                    float rot = member.ROT.Radians().Deg;
                    bool isAnchor = member.isAnchor;
                    string emote = member.emote;

                    bool xchanged = false;
                    bool ychanged = false;
                    bool rotchanged = false;
                    bool anchorchanged = false;
                    bool emotechanged = false;

                    ImGui.Text(selectedMember.ToString() + " Properties");
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
                        xchanged = ImGui.DragFloat("##x", ref x, 0.05f);
                        ImGui.SameLine();
                        ImGui.Text("  ");
                    });
                    ImGui.Dummy(new Vector2(0, 10));
                    ImGui.Text("Offset Y");
                    ImGui.SameLine();
                    ImGuiEx.ImGuiLineRightAlign("Offsety", () =>
                    {
                        ychanged = ImGui.DragFloat("##y", ref y, 0.05f);
                        ImGui.SameLine();
                        ImGui.Text("  ");
                    });
                    ImGui.Dummy(new Vector2(0, 10));
                    ImGui.Text("Rotation");
                    ImGui.SameLine();
                    ImGuiEx.ImGuiLineRightAlign("Offsetrot", () =>
                    {
                        rotchanged = ImGui.DragFloat("##rot", ref rot, 0.05f);
                        ImGui.SameLine();
                        ImGui.Text("  ");
                    });
                    
                    ImGui.Dummy(new Vector2(0, 10));
                    ImGui.Text("Emote");
                    ImGui.SameLine();
                    ImGuiEx.ImGuiLineRightAlign("Offsetcombo", () =>
                    {
                        if (ImGui.BeginCombo("##EmoteCombo", emote))
                        {
                            ImGui.InputText("##Search", ref filterText, BufferSize);
                            foreach (var j in Service.Emotes)
                            {
                                var i = j.Replace("/", "");
                                if (!filterText.IsNullOrEmpty() && !i.Contains(filterText))
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
                            ImGui.EndCombo();
                        }
                        

                        ImGui.SameLine();
                        ImGui.Text("  ");
                    });



                    member.X = Math.Clamp(x,-10,10);
                    member.Y = Math.Clamp(y, -10, 10);
                    member.ROT = Math.Clamp(rot, -180, 180).Degrees().Rad;
                    member.isAnchor = isAnchor;
                    member.emote = emote;

                    if (xchanged || ychanged || rotchanged || anchorchanged || emotechanged)
                    {
                        string jsonString = JsonSerializer.Serialize(activePreset, new JsonSerializerOptions { WriteIndented = true });
                        File.WriteAllText(Plugin.PresetDirectory + "/" + selectedLayout + ".json", jsonString);
                    }

                }
                else
                {
                    ImGui.TextWrapped("Select a Preset Member to begin editing their properties.");
                }
            }
            ImGui.EndChild();
            if (ImGui.BeginChild("Actions", LayoutsChildSize, true, ImGuiWindowFlags.NoCollapse))
            {

            }
            ImGui.EndChild();
        }
        ImGui.EndChild();
        ImGui.NewLine();
        ImGui.Separator();


    }

    public void ContextPopup()
    {
        if (contextPopupOpen)
        {
            // Set window size on the first frame
            if (isFirstFrame)
            {
                contextMenuPosition = ImGui.GetMousePos();
                isFirstFrame = false;
            }

            ImGui.SetNextWindowSize(contextMenuSize);
            ImGui.SetNextWindowPos(contextMenuPosition);

            if (ImGui.Begin("Contextual Menu", ref contextPopupOpen, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar))
            {
                Service.Log.Debug("You should be seeing a context menu right about now");

                if (ImGui.Selectable("Rename Preset"))
                {
                    ImGui.SetNextWindowPos(new Vector2(10.0f, 100.0f));
                    ImGui.SetNextWindowSize(new Vector2(200.0f, 60.0f));
                    renamingWindowOpen = true;
                    contextPopupOpen = false;
                    renameBuffer = activePreset?.PresetName ?? "";
                }
            }
            ImGui.End();
            if (!ImGui.IsItemHovered() && ImGui.IsMouseReleased(ImGuiMouseButton.Left))
            {
                contextPopupOpen = false;
            }

            // Reset isFirstFrame if contextPopupOpen is false
            if (!contextPopupOpen)
            {
                isFirstFrame = true;
            }


        }
    }

    public void OpenNamingWindow()
    {
        if (namingWindowOpen)
        {

            if (ImGui.Begin("Preset Name", ref namingWindowOpen))
            {
                if (ImGui.BeginChild("##", new Vector2(-1, -1), true))
                {
                    ImGui.Text("Name");
                    ImGui.SameLine();
                    ImGui.InputText("##", ref nameBuffer, (uint)BufferSize);
                    ImGui.SameLine();
                    if (IconButtonWithText(FontAwesomeIcon.Save, "", "Save"))
                    {
                        var preset = new Preset
                        {
                            PresetName = nameBuffer,
                        };

                        string jsonString = JsonSerializer.Serialize(preset, new JsonSerializerOptions { WriteIndented = true });
                        File.WriteAllText(Plugin.PresetDirectory + "/" + nameBuffer + ".json", jsonString);

                        namingWindowOpen = false;

                    }
                }
                ImGui.EndChild();
            }
            ImGui.End();
        }
    }

    public void OpenReNamingWindow()
    {
        if (renamingWindowOpen)
        {

            if (ImGui.Begin("Rename a preset", ref renamingWindowOpen))
            {
                if (ImGui.BeginChild("##", new Vector2(-1, -1), true))
                {
                    ImGui.Text("Name");
                    ImGui.SameLine();
                    ImGui.InputText("##", ref renameBuffer, (uint)BufferSize);
                    ImGui.SameLine();
                    if (IconButtonWithText(FontAwesomeIcon.Save, "", "Save"))
                    {
                        if (activePreset == null)
                        {
                            return;
                        }
                        var prevName = activePreset.PresetName;
                        activePreset.PresetName = renameBuffer;

                        string jsonString = JsonSerializer.Serialize(activePreset, new JsonSerializerOptions { WriteIndented = true });
                        File.WriteAllText(Plugin.PresetDirectory + "/" + renameBuffer + ".json", jsonString);
                        File.Delete(Plugin.PresetDirectory + "/" + prevName + ".json");

                        renamingWindowOpen = false;

                    }
                }
                ImGui.EndChild();
            }
            ImGui.End();
        }
    }

    public void OpenNewMemberWindow()
    {
        if (newMemberWindowOpen)
        {

            if (ImGui.Begin("Member's Character Name", ref newMemberWindowOpen))
            {
                if (ImGui.BeginChild("##", new Vector2(-1, -1), true))
                {
                    ImGui.Text("Character Name");
                    ImGui.SameLine();
                    ImGui.InputText("##", ref nameBuffer, (uint)BufferSize);
                    ImGui.SameLine();
                    if (IconButtonWithText(FontAwesomeIcon.Save, "", "Save"))
                    {
                        var preset = new Preset
                        {
                            PresetName = nameBuffer,
                        };

                        string jsonString = JsonSerializer.Serialize(preset);
                        File.WriteAllText(Plugin.PresetDirectory + "/" + nameBuffer + ".json", jsonString);

                        newMemberWindowOpen = false;

                    }
                }
                ImGui.EndChild();
            }
            ImGui.End();
        }
    }

    public static void SendNotification(string message)
    {
        Notification notif = new Notification();
        notif.Content = message;
        Service.NotificationManager.AddNotification(notif);
    }
}
