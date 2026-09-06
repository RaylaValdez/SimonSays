using System;
using System.Linq;
using Dalamud.Game.Chat;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Common.Math;
using SimonSays.Helpers;
using ObjectKind = Dalamud.Game.ClientState.Objects.Enums.ObjectKind;
using Vector3 = System.Numerics.Vector3;

namespace SimonSays;

internal class Meat
{
    public static OverrideMovement? movement;

    /// <summary>
    /// Sets up character movement resources if they are not already initialized.
    /// </summary>
    public static void Setup()
    {
        if (movement == null)
        {
            movement = new OverrideMovement
            {
                Enabled = true,
            };
        }
    }

    /// <summary>
    /// Disposes of resources related to character movement and chat sending.
    /// </summary>
    public static void Dispose()
    {
        movement?.Dispose();
        movement = null;
        ChatSender.Instance.Dispose();
    }

    /// <summary>
    /// Cleans and sanitizes the emote string by removing specified characters and checks if it is a valid emote.
    /// </summary>
    /// <param name="emote">The emote string to sanitize.</param>
    /// <returns>True if the sanitized emote is valid; otherwise, false.</returns>
    public static bool SanitizeEmote(ref string emote)
    {
        emote = emote.Replace("(", string.Empty).Replace(")", string.Empty).Replace("/", string.Empty).Replace("[", string.Empty).Replace("]", string.Empty).Replace("{", string.Empty).Replace("}", string.Empty).ToLowerInvariant();

        return Sausages.Emotes.Contains("/" + emote);
    }

    /// <summary>
    /// Processes chat commands, executing emotes based on the configuration settings and message content.
    /// </summary>
    /// <param name="type">The type of chat message.</param>
    /// <param name="message">The content of the chat message.</param>
    /// <param name="forceForTesting">Flag to force command execution for testing purposes.</param>
    public static void ProccessChatCommands(XivChatType type, string message, bool forceForTesting = false)
    {
        if (Potatoes.Configuration == null)
        {
            return;
        }

        if (!Potatoes.Configuration.IsListening)
        {
            return;
        }

        Potatoes.Configuration.EnabledChannels.TryGetValue((int)type, out var enabled);
        if (!enabled && !forceForTesting)
        {
            return;
        }

        var catchPhrase = Potatoes.Configuration.CatchPhrase;

        if (!message.StartsWith(catchPhrase, StringComparison.Ordinal))
        {
            return;
        }

        var emote = message[catchPhrase.Length..].TrimStart();

        if (emote.StartsWith("pe:", StringComparison.Ordinal) || emote.StartsWith("pp:", StringComparison.Ordinal))
        {
            Gravy.HandlePartyChatPreset(emote);
            return;
        }

        if (!SanitizeEmote(ref emote))
        {
            Sausages.ChatGui.Print("You haven't specified a correct Emote.");
            return;
        }

        if (Potatoes.Configuration.MotionOnly)
        {
            emote += " motion";
        }

        Veggies.SendChatMessageAsIfPlayer("/" + emote);
    }

    /// <summary>
    /// Handles incoming chat messages and delegates them to the command processing method.
    /// </summary>
    /// <param name="chatMessage">The incoming chat message.</param>
    public static void OnChatMessage(IHandleableChatMessage chatMessage)
    {
        if (chatMessage.IsHandled)
        {
            return;
        }

        ProccessChatCommands(chatMessage.LogKind, chatMessage.Message.ToString());
    }

    /// <summary>
    /// Initiates character movement towards the current target or prints a message if no target is selected.
    /// </summary>
    /// <param name="offset">Optional positional offset relative to the target.</param>
    /// <param name="callback">Optional callback invoked when movement completes.</param>
    public static void StartScooch(Vector3? offset = null, OverrideMovement.OnCompleteDelegate? callback = null)
    {
        var target = Sausages.TargetManager.Target;

        if (movement == null)
        {
            Sausages.Log.Debug("movement is null");
            return;
        }

        movement.ClearCallback();

        if (target != null)
        {
            ScoochOnOver(offset, target);
        }
        else
        {
            movement.SoftDisable = true;
            Sausages.ChatGui.Print("You haven't got a Target, numpty");
        }

        if (callback != null)
        {
            movement.SetCallback(callback);
        }
    }

    /// <summary>
    /// Stops the character's movement initiated by the ScoochOnOver method.
    /// </summary>
    public static void StopScooch()
    {
        if (movement != null)
        {
            movement.SoftDisable = true;
        }
    }

    /// <summary>
    /// Moves the local player to a preset offset relative to the given target.
    /// </summary>
    /// <param name="offset">The positional offset relative to the target.</param>
    /// <param name="target">The target game object to move relative to.</param>
    /// <param name="rotation">The rotation offset in radians.</param>
    public static void ScoochPresetOffset(Vector3? offset, IGameObject target, float rotation)
    {
        var character = Sausages.ObjectTable.LocalPlayer;

        if (character == null)
        {
            return;
        }

        offset ??= Vector3.Zero;

        var desiredPosition = target.Position;
        var anchorRotation = target.Rotation;

        // When target is not a player, use the DefaultRotation
        if (target is not IPlayerCharacter && target.Address != nint.Zero)
        {
            var useDefaultRotation = target.ObjectKind switch
            {
                ObjectKind.Companion => false,
                ObjectKind.Aetheryte => false,
                ObjectKind.EventNpc => false,
                _ => true,
            };

            unsafe
            {
                if (useDefaultRotation)
                {
                    var clientStructsObject = (GameObject*)target.Address;
                    var defaultRotation = clientStructsObject->DefaultRotation;
                    Sausages.Log.Debug($"Got anchor as FFXIVClientStructs object, with a default rotation of {defaultRotation} rad, {defaultRotation / (MathF.PI / 180f)} deg. Original rotation was {anchorRotation} rad, {anchorRotation / (MathF.PI / 180f)} deg");
                    anchorRotation = defaultRotation;
                }
            }
        }

        var desiredRotation = (anchorRotation - rotation) % MathF.Tau;

        if (float.IsNaN(desiredRotation) || float.IsInfinity(desiredRotation))
        {
            desiredRotation = character.Rotation;
        }

        var cRot = MathF.Cos(anchorRotation);
        var sRot = MathF.Sin(anchorRotation);
        var offsetX = (offset.Value.X * cRot) - (offset.Value.Z * sRot);
        var offsetZ = (offset.Value.Z * cRot) + (offset.Value.X * sRot);
        Sausages.Log.Debug($"Rotation {anchorRotation} (deg: {anchorRotation / (MathF.PI / 180)}), <{offsetX},{offsetZ}>");

        desiredPosition.X += -offsetX;
        desiredPosition.Z += offsetZ;

        if (movement != null)
        {
            movement.DesiredPosition = desiredPosition;
            movement.DesiredRotation = desiredRotation;

            unsafe
            {
                var playerController = Control.Instance();
                if (playerController != null)
                {
                    playerController->IsWalking = true;
                }
            }

            movement.SoftDisable = false;

            movement.SetCallback(() =>
            {
                unsafe
                {
                    var playerController = Control.Instance();
                    if (playerController != null)
                    {
                        Sausages.Log.Debug("Resetting walking state to false");
                        playerController->IsWalking = false;
                    }
                }
            });
        }
    }

    /// <summary>
    /// Moves the local player's character towards the specified target GameObject.
    /// </summary>
    /// <param name="offset">The positional offset relative to the target.</param>
    /// <param name="target">The target GameObject to move towards.</param>
    public static void ScoochOnOver(Vector3? offset, IGameObject target)
    {
        var character = Sausages.ObjectTable.LocalPlayer;

        if (character == null)
        {
            return;
        }

        offset ??= Vector3.Zero;

        var tarPos = target.Position;
        var tarRot = target.Rotation;

        if (float.IsNaN(tarRot) || float.IsInfinity(tarRot))
        {
            tarRot = character.Rotation;
        }

        var offsetX = (-offset.Value.X * MathF.Cos(tarRot)) - (offset.Value.Z * MathF.Sin(tarRot));
        var offsetZ = (-offset.Value.X * MathF.Sin(tarRot)) + (offset.Value.Z * MathF.Cos(tarRot));

        tarPos.X += offsetX;
        tarPos.Z += offsetZ;

        tarRot += offset.Value.Y.Degrees().Rad;

        if (movement != null)
        {
            movement.DesiredPosition = tarPos;
            movement.DesiredRotation = tarRot;

            movement.SoftDisable = false;
        }
    }

    /// <summary>
    /// Executes the specified emote with optional position synchronization.
    /// </summary>
    /// <param name="emote">The emote to execute.</param>
    /// <param name="otherEmote">The emote the target should execute.</param>
    /// <param name="shouldSyncPosition">Flag indicating whether to synchronize positions.</param>
    public static void SimonSays(string emote, string otherEmote, bool shouldSyncPosition)
    {
        if (!SanitizeEmote(ref emote))
        {
            Sausages.ChatGui.Print("You have not specified a valid emote");
            return;
        }

        if (!SanitizeEmote(ref otherEmote))
        {
            Sausages.ChatGui.Print("You have not specified a valid other emote");
            return;
        }

        var target = Sausages.TargetManager.Target;

        if (shouldSyncPosition)
        {
            var tempOffset = EmoteHasOffset(emote);

            // Start scooching with the callback that executes the emote once we have arrived at our destination
            StartScooch(tempOffset, () =>
            {
                if (target != null)
                {
                    Sausages.Log.Information("Telling target to do emote " + otherEmote);
                    Veggies.SendChatMessageAsIfPlayer("/tell <t> " + Potatoes.Configuration!.CatchPhrase + " " + otherEmote);
                }

                Veggies.SendChatMessageAsIfPlayer("/" + emote);
            });
        }
        else
        {
            if (target != null)
            {
                Sausages.Log.Information("Telling target to do emote " + otherEmote);
                Veggies.SendChatMessageAsIfPlayer("/tell <t> " + Potatoes.Configuration!.CatchPhrase + " " + otherEmote);
            }

            Veggies.SendChatMessageAsIfPlayer("/" + emote);
        }
    }

    /// <summary>
    /// Checks if an emote has an offset and returns the offset as a Vector3 object.
    /// </summary>
    /// <param name="emote">The emote string to check for an offset.</param>
    /// <returns>The offset as a Vector3 object. If the emote is null or empty, returns a Vector3 with all values set to 0.</returns>
    public static Vector3 EmoteHasOffset(string emote)
    {
        var offset = Vector3.Zero;

        if (string.IsNullOrEmpty(emote))
        {
            return offset;
        }

        var emoteOffset = Potatoes.Configuration!.EmoteOffsets.FirstOrDefault((o) => o.Enabled && o.Emote == emote);

        if (emoteOffset != null)
        {
            Sausages.Log.Information($"Using emote offset {emoteOffset.Label} for emote {emote}");

            offset.X = emoteOffset.X;
            offset.Y = emoteOffset.R;
            offset.Z = emoteOffset.Z;
        }

        return offset;
    }
}
