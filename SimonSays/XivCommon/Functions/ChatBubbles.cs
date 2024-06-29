using System;
using System.Runtime.InteropServices;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;

namespace XivCommon.Functions;

/// <summary>
/// Class containing chat bubble events and functions
/// </summary>
public class ChatBubbles : IDisposable {
    private static class Signatures {
        internal const string ChatBubbleOpen = "E8 ?? ?? ?? ?? F6 86 ?? ?? ?? ?? ?? C7 46";
        internal const string ChatBubbleUpdate = "48 85 D2 0F 84 ?? ?? ?? ?? 48 89 6C 24 ?? 56 48 83 EC 30";
    }

    private IObjectTable ObjectTable { get; }

    private delegate void OpenChatBubbleDelegate(IntPtr agent, IntPtr @object, IntPtr text, nint a4, byte a5);

    private delegate void UpdateChatBubbleDelegate(IntPtr bubblePtr, IntPtr @object);

    private Hook<OpenChatBubbleDelegate>? OpenChatBubbleHook { get; }

    private Hook<UpdateChatBubbleDelegate>? UpdateChatBubbleHook { get; }

    /// <summary>
    /// The delegate for chat bubble events.
    /// </summary>
    public delegate void OnChatBubbleDelegate(ref IGameObject @object, ref SeString text);

    /// <summary>
    /// The delegate for chat bubble update events.
    /// </summary>
    public delegate void OnUpdateChatBubbleDelegate(ref IGameObject @object);

    /// <summary>
    /// <para>
    /// The event that is fired when a chat bubble is shown.
    /// </para>
    /// <para>
    /// Requires the <see cref="Hooks.ChatBubbles"/> hook to be enabled.
    /// </para>
    /// </summary>
    public event OnChatBubbleDelegate? OnChatBubble;

    /// <summary>
    /// <para>
    /// The event that is fired when a chat bubble is updated.
    /// </para>
    /// <para>
    /// Requires the <see cref="Hooks.ChatBubbles"/> hook to be enabled.
    /// </para>
    /// </summary>
    public event OnUpdateChatBubbleDelegate? OnUpdateBubble;

    internal ChatBubbles(IObjectTable objectTable, ISigScanner scanner, IGameInteropProvider interop, bool hookEnabled) {
        this.ObjectTable = objectTable;

        if (!hookEnabled) {
            return;
        }

        if (scanner.TryScanText(Signatures.ChatBubbleOpen, out var openPtr, "chat bubbles open")) {
            this.OpenChatBubbleHook = interop.HookFromAddress<OpenChatBubbleDelegate>(openPtr, this.OpenChatBubbleDetour);
            this.OpenChatBubbleHook.Enable();
        }

        if (scanner.TryScanText(Signatures.ChatBubbleUpdate, out var updatePtr, "chat bubbles update")) {
            this.UpdateChatBubbleHook = interop.HookFromAddress<UpdateChatBubbleDelegate>(updatePtr + 9, this.UpdateChatBubbleDetour);
            this.UpdateChatBubbleHook.Enable();
        }
    }

    /// <inheritdoc />
    public void Dispose() {
        this.OpenChatBubbleHook?.Dispose();
        this.UpdateChatBubbleHook?.Dispose();
    }

    private void OpenChatBubbleDetour(IntPtr manager, IntPtr @object, IntPtr text, nint a4, byte a5) {
        try {
            this.OpenChatBubbleDetourInner(manager, @object, text, a4, a5);
        } catch (Exception ex) {
            Logger.Log.Error(ex, "Exception in chat bubble detour");
            this.OpenChatBubbleHook!.Original(manager, @object, text, a4, a5);
        }
    }

    private void OpenChatBubbleDetourInner(IntPtr manager, IntPtr objectPtr, IntPtr textPtr, nint a4, byte a5) {
        var @object = this.ObjectTable.CreateObjectReference(objectPtr);
        if (@object == null) {
            return;
        }

        var text = Util.ReadSeString(textPtr);

        try {
            this.OnChatBubble?.Invoke(ref @object, ref text);
        } catch (Exception ex) {
            Logger.Log.Error(ex, "Exception in chat bubble event");
        }

        var newText = text.Encode().Terminate();

        unsafe {
            fixed (byte* newTextPtr = newText) {
                this.OpenChatBubbleHook!.Original(manager, @object.Address, (IntPtr) newTextPtr, a4, a5);
            }
        }
    }

    private void UpdateChatBubbleDetour(IntPtr bubblePtr, IntPtr @object) {
        try {
            this.UpdateChatBubbleDetourInner(bubblePtr, @object);
        } catch (Exception ex) {
            Logger.Log.Error(ex, "Exception in update chat bubble detour");
            this.UpdateChatBubbleHook!.Original(bubblePtr, @object);
        }
    }

    private void UpdateChatBubbleDetourInner(IntPtr bubblePtr, IntPtr objectPtr) {
        // var bubble = (ChatBubble*) bubblePtr;
        var @object = this.ObjectTable.CreateObjectReference(objectPtr);
        if (@object == null) {
            return;
        }

        try {
            this.OnUpdateBubble?.Invoke(ref @object);
        } catch (Exception ex) {
            Logger.Log.Error(ex, "Exception in chat bubble update event");
        }

        this.UpdateChatBubbleHook!.Original(bubblePtr, @object.Address);
    }
}

[StructLayout(LayoutKind.Explicit, Size = 0x80)]
internal unsafe struct ChatBubble {
    [FieldOffset(0x0)]
    internal readonly uint Id;

    [FieldOffset(0x4)]
    internal float Timer;

    [FieldOffset(0x8)]
    internal readonly uint Unk_8; // enum probably

    [FieldOffset(0xC)]
    internal ChatBubbleStatus Status; // state of the bubble

    [FieldOffset(0x10)]
    internal readonly byte* Text;

    [FieldOffset(0x78)]
    internal readonly ulong Unk_78; // check whats in memory here
}

internal enum ChatBubbleStatus : uint {
    GetData = 0,
    On = 1,
    Init = 2,
    Off = 3,
}
