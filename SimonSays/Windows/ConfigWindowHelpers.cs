using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using ImGuiNET;
using ImPlotNET;
using PunishLib.ImGuiMethods;
using SimonSays.Helpers;
using SimonSays.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

internal static class ConfigWindowHelpers
{

    public static string selectedLayout = String.Empty;
    public static string selectedMember = String.Empty;
    public static bool namingWindowOpen = false;
    public static bool renamingWindowOpen = false;
    public static bool newMemberWindowOpen = false;
    public static bool contextPopupOpen = false;
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
}