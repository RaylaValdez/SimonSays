using Dalamud.Game;
using Dalamud.IoC;
using Dalamud.Plugin.Services;

namespace XivCommon;

internal class Services {
    [PluginService]
    internal IPluginLog? Log { get; private set; }

    [PluginService]
    internal IFramework? Framework { get; private set; }

    [PluginService]
    internal IGameGui? GameGui { get; private set; }

    [PluginService]
    internal IGameInteropProvider? GameInteropProvider { get; private set; }

    [PluginService]
    internal IObjectTable? ObjectTable { get; private set; }

    [PluginService]
    internal IPartyFinderGui? PartyFinderGui { get; private set; }

    [PluginService]
    internal ISigScanner? SigScanner { get; private set; }
}
