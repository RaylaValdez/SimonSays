using Dalamud.Plugin.Services;

namespace XivCommon;

internal static class Logger {
    internal static IPluginLog Log { get; set; } = null!;
}
