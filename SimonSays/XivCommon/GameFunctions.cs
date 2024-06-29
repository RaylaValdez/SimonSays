using System;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using XivCommon.Functions;
using XivCommon.Functions.FriendList;
using XivCommon.Functions.NamePlates;
using XivCommon.Functions.Tooltips;
using Framework = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework;

namespace XivCommon;

/// <summary>
/// A class containing game functions
/// </summary>
public class GameFunctions : IDisposable {
    private IGameGui GameGui { get; }

    private IFramework Framework { get; }

    internal UiAlloc UiAlloc { get; }

    /// <summary>
    /// Chat functions
    /// </summary>
    public Chat Chat { get; }

    /// <summary>
    /// Party Finder functions and events
    /// </summary>
    public PartyFinder PartyFinder { get; }

    /// <summary>
    /// BattleTalk functions and events
    /// </summary>
    public BattleTalk BattleTalk { get; }

    /// <summary>
    /// Examine functions
    /// </summary>
    public Examine Examine { get; }

    /// <summary>
    /// Talk events
    /// </summary>
    public Talk Talk { get; }

    /// <summary>
    /// Chat bubble functions and events
    /// </summary>
    public ChatBubbles ChatBubbles { get; }

    /// <summary>
    /// Tooltip events
    /// </summary>
    public Tooltips Tooltips { get; }

    /// <summary>
    /// Name plate tools and events
    /// </summary>
    public NamePlates NamePlates { get; }

    /// <summary>
    /// Duty Finder functions
    /// </summary>
    public DutyFinder DutyFinder { get; }

    /// <summary>
    /// Friend list functions
    /// </summary>
    public FriendList FriendList { get; }

    /// <summary>
    /// Journal functions
    /// </summary>
    public Journal Journal { get; }

    internal GameFunctions(IDalamudPluginInterface @interface, Hooks hooks) {
        var services = @interface.Create<Services>();
        if (services == null) {
            throw new Exception("could not create services");
        }

        Logger.Log = services.Log!;

        this.Framework = services.Framework!;
        this.GameGui = services.GameGui!;

        var interop = services.GameInteropProvider!;
        var objectTable = services.ObjectTable!;
        var partyFinderGui = services.PartyFinderGui!;
        var scanner = services.SigScanner!;

        this.UiAlloc = new UiAlloc(scanner);
        this.Chat = new Chat(scanner);
        this.PartyFinder = new PartyFinder(scanner, partyFinderGui, interop, hooks);
        this.BattleTalk = new BattleTalk(interop, hooks.HasFlag(Hooks.BattleTalk));
        this.Examine = new Examine(scanner);
        this.Talk = new Talk(scanner, interop, hooks.HasFlag(Hooks.Talk));
        this.ChatBubbles = new ChatBubbles(objectTable, scanner, interop, hooks.HasFlag(Hooks.ChatBubbles));
        this.Tooltips = new Tooltips(scanner, this.GameGui, interop, hooks.HasFlag(Hooks.Tooltips));
        this.NamePlates = new NamePlates(this, scanner, interop, hooks.HasFlag(Hooks.NamePlates));
        this.DutyFinder = new DutyFinder(scanner);
        this.Journal = new Journal(scanner);
        this.FriendList = new FriendList();
    }

    /// <inheritdoc />
    public void Dispose() {
        this.NamePlates.Dispose();
        this.Tooltips.Dispose();
        this.ChatBubbles.Dispose();
        this.Talk.Dispose();
        this.BattleTalk.Dispose();
        this.PartyFinder.Dispose();
    }

    /// <summary>
    /// Convenience method to get a pointer to <see cref="Framework"/>.
    /// </summary>
    /// <returns>pointer to struct</returns>
    [Obsolete("Use Framework.Instance()")]
    public unsafe Framework* GetFramework() {
        return FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance();
    }

    /// <summary>
    /// Gets the pointer to the UI module
    /// </summary>
    /// <returns>Pointer</returns>
    [Obsolete("Use Framework.Instance()->GetUiModule()")]
    public unsafe IntPtr GetUiModule() {
        return (IntPtr) this.GetFramework()->GetUIModule();
    }

    /// <summary>
    /// Gets the pointer to the RaptureAtkModule
    /// </summary>
    /// <returns>Pointer</returns>
    [Obsolete("Use Framework.Instance()->GetUiModule()->GetRaptureAtkModule()")]
    public unsafe IntPtr GetAtkModule() {
        return (IntPtr) this.GetFramework()->GetUIModule()->GetRaptureAtkModule();
    }

    /// <summary>
    /// Gets the pointer to the agent module
    /// </summary>
    /// <returns>Pointer</returns>
    [Obsolete("Use Framework.Instance()->GetUiModule()->GetAgentModule()")]
    public unsafe IntPtr GetAgentModule() {
        return (IntPtr) this.GetFramework()->GetUIModule()->GetAgentModule();
    }

    /// <summary>
    /// Gets the pointer to an agent from its internal ID.
    /// </summary>
    /// <param name="id">internal id of agent</param>
    /// <returns>Pointer</returns>
    /// <exception cref="InvalidOperationException">if the signature for the function could not be found</exception>
    [Obsolete("Use Framework.Instance()->GetUiModule()->GetAgentModule()->GetAgentByInternalId(AgentId)")]
    public unsafe IntPtr GetAgentByInternalId(uint id) {
        return (IntPtr) this.GetFramework()->GetUIModule()->GetAgentModule()->GetAgentByInternalId((AgentId) id);
    }

    /// <summary>
    /// Gets the pointer to the AtkStage singleton
    /// </summary>
    /// <returns>Pointer</returns>
    /// <exception cref="InvalidOperationException">if the signature for the function could not be found</exception>
    [Obsolete("Use AtkStage.GetSingleton()")]
    public unsafe IntPtr GetAtkStageSingleton() {
        return (IntPtr) AtkStage.Instance();
    }
}
