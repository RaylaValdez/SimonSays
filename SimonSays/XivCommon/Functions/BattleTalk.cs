using System;
using System.Runtime.InteropServices;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using Framework = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework;

namespace XivCommon.Functions;

/// <summary>
/// The class containing BattleTalk functionality
/// </summary>
public class BattleTalk : IDisposable {
    private bool HookEnabled { get; }

    /// <summary>
    /// The delegate for BattleTalk events.
    /// </summary>
    public delegate void BattleTalkEventDelegate(ref SeString Sender, ref SeString Message, ref BattleTalkOptions options, ref bool IsHandled);

    /// <summary>
    /// <para>
    /// The event that is fired when a BattleTalk window is shown.
    /// </para>
    /// <para>
    /// Requires the <see cref="Hooks.BattleTalk"/> hook to be enabled.
    /// </para>
    /// </summary>
    public event BattleTalkEventDelegate? OnBattleTalk;

    private delegate byte AddBattleTalkDelegate(IntPtr uiModule, IntPtr Sender, IntPtr Message, float duration, byte style);

    private AddBattleTalkDelegate? AddBattleTalk { get; }
    private Hook<AddBattleTalkDelegate>? AddBattleTalkHook { get; }

    internal unsafe BattleTalk(IGameInteropProvider interop, bool hook) {
        this.HookEnabled = hook;

        var addBattleTalkPtr = (IntPtr) Framework.Instance()->GetUiModule()->VTable->ShowBattleTalk;
        if (addBattleTalkPtr != IntPtr.Zero) {
            this.AddBattleTalk = Marshal.GetDelegateForFunctionPointer<AddBattleTalkDelegate>(addBattleTalkPtr);

            if (this.HookEnabled) {
                this.AddBattleTalkHook = interop.HookFromAddress<AddBattleTalkDelegate>(addBattleTalkPtr, this.AddBattleTalkDetour);
                this.AddBattleTalkHook.Enable();
            }
        }
    }

    /// <inheritdoc />
    public void Dispose() {
        this.AddBattleTalkHook?.Dispose();
    }

    private byte AddBattleTalkDetour(IntPtr uiModule, IntPtr SenderPtr, IntPtr MessagePtr, float duration, byte style) {
        if (this.OnBattleTalk == null) {
            goto Return;
        }

        try {
            return this.AddBattleTalkDetourInner(uiModule, SenderPtr, MessagePtr, duration, style);
        } catch (Exception ex) {
            Logger.Log.Error(ex, "Exception in BattleTalk detour");
        }

        Return:
        return this.AddBattleTalkHook!.Original(uiModule, SenderPtr, MessagePtr, duration, style);
    }

    private unsafe byte AddBattleTalkDetourInner(IntPtr uiModule, IntPtr SenderPtr, IntPtr MessagePtr, float duration, byte style) {
        var rawSender = Util.ReadTerminated(SenderPtr);
        var rawMessage = Util.ReadTerminated(MessagePtr);

        var Sender = SeString.Parse(rawSender);
        var Message = SeString.Parse(rawMessage);

        var options = new BattleTalkOptions {
            Duration = duration,
            Style = (BattleTalkStyle) style,
        };

        var handled = false;
        try {
            this.OnBattleTalk?.Invoke(ref Sender, ref Message, ref options, ref handled);
        } catch (Exception ex) {
            Logger.Log.Error(ex, "Exception in BattleTalk event");
        }

        if (handled) {
            return 0;
        }

        var finalSender = Sender.Encode().Terminate();
        var finalMessage = Message.Encode().Terminate();

        fixed (byte* fSenderPtr = finalSender, fMessagePtr = finalMessage) {
            return this.AddBattleTalkHook!.Original(uiModule, (IntPtr) fSenderPtr, (IntPtr) fMessagePtr, options.Duration, (byte) options.Style);
        }
    }

    /// <summary>
    /// Show a BattleTalk window with the given options.
    /// </summary>
    /// <param name="Sender">The name to attribute to the Message</param>
    /// <param name="Message">The Message to show in the window</param>
    /// <param name="options">Optional options for the window</param>
    /// <exception cref="ArgumentException">If Sender or Message are empty</exception>
    /// <exception cref="InvalidOperationException">If the signature for this function could not be found</exception>
    public void Show(SeString Sender, SeString Message, BattleTalkOptions? options = null) {
        this.Show(Sender.Encode(), Message.Encode(), options);
    }

    private unsafe void Show(byte[] Sender, byte[] Message, BattleTalkOptions? options) {
        if (Sender.Length == 0) {
            throw new ArgumentException("Sender cannot be empty", nameof(Sender));
        }

        if (Message.Length == 0) {
            throw new ArgumentException("Message cannot be empty", nameof(Message));
        }

        if (this.AddBattleTalk == null) {
            throw new InvalidOperationException("Signature for battle talk could not be found");
        }

        options ??= new BattleTalkOptions();

        var uiModule = (IntPtr) Framework.Instance()->GetUiModule();

        fixed (byte* SenderPtr = Sender.Terminate(), MessagePtr = Message.Terminate()) {
            if (this.HookEnabled) {
                this.AddBattleTalkDetour(uiModule, (IntPtr) SenderPtr, (IntPtr) MessagePtr, options.Duration, (byte) options.Style);
            } else {
                this.AddBattleTalk(uiModule, (IntPtr) SenderPtr, (IntPtr) MessagePtr, options.Duration, (byte) options.Style);
            }
        }
    }
}

/// <summary>
/// Options for displaying a BattleTalk window.
/// </summary>
public class BattleTalkOptions {
    /// <summary>
    /// Duration to display the window, in seconds.
    /// </summary>
    public float Duration { get; set; } = 5f;

    /// <summary>
    /// The style of the window.
    /// </summary>
    public BattleTalkStyle Style { get; set; } = BattleTalkStyle.Normal;
}

/// <summary>
/// BattleTalk window styles.
/// </summary>
public enum BattleTalkStyle : byte {
    /// <summary>
    /// A normal battle talk window with a white background.
    /// </summary>
    Normal = 0,

    /// <summary>
    /// A battle talk window with a blue background and styled edges.
    /// </summary>
    Aetherial = 6,

    /// <summary>
    /// A battle talk window styled similarly to a system text Message (black background).
    /// </summary>
    System = 7,

    /// <summary>
    /// <para>
    /// A battle talk window with a blue, computer-y background.
    /// </para>
    /// <para>
    /// Used by the Ultima Weapons (Ruby, Emerald, etc.).
    /// </para>
    /// </summary>
    Blue = 9,
}
