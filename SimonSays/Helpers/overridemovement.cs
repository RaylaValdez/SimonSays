using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using SimonSays;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;

namespace SimonSays.Helpers;

[StructLayout(LayoutKind.Explicit, Size = 0x18)]
public unsafe struct PlayerMoveControllerFlyInput
{
    [FieldOffset(0x0)] public float Forward;
    [FieldOffset(0x4)] public float Left;
    [FieldOffset(0x8)] public float Up;
    [FieldOffset(0xC)] public float Turn;
    [FieldOffset(0x10)] public float u10;
    [FieldOffset(0x14)] public byte DirMode;
    [FieldOffset(0x15)] public byte HaveBackwardOrStrafe;
}

public unsafe class OverrideMovement : IDisposable
{
    public bool Enabled
    {
        get => _rmiWalkHook.IsEnabled;
        set
        {
            if (Value)
            {
                _rmiWalkHook.Enable();
                _rmiFlyHook.Enable();
            }
            else
            {
                _rmiWalkHook.Disable();
                _rmiFlyHook.Disable();
            }
        }
    }

    public bool IgnoreUserInput; // if true - override even if user tries to change camera orientation, otherwise override only if user does nothing
    public Vector3 DesiredPosition; // Position to move towards
    public float DesiredRotation; // Rotation to rotate towards
    public float Precision = 0.075f; // Distance from Target to stop at
    public float TurnPrecision = 5.0f; // Angle in Degrees to stop rotating at
    public bool ShouldTurn = true; // Should rotate to face DesiredRotation?
    public bool AutoDisable = true; // Automatically disable if not moved and not rotated
    public bool SoftDisable = true; // Like Enabled but softer

    private DateTime LastTimeMoved;
    private DateTime LastTimeTurned;

    private delegate void RMIWalkDelegate(void* self, float* sumLeft, float* sumForward, float* sumTurnLeft, byte* haveBackwardOrStrafe, byte* a6, byte bAdditiveUnk);
    [Signature("E8 ?? ?? ?? ?? 80 7B 3E 00 48 8D 3D")]
    private Hook<RMIWalkDelegate> _rmiWalkHook = null!;

    private delegate void RMIFlyDelegate(void* self, PlayerMoveControllerFlyInput* result);
    [Signature("E8 ?? ?? ?? ?? 0F B6 0D ?? ?? ?? ?? B8")]
    private Hook<RMIFlyDelegate> _rmiFlyHook = null!;

    public OverrideMovement()
    {
        Service.Hook.InitializeFromAttributes(this);
        Service.Log.Information($"RMIWalk address: 0x{_rmiWalkHook.Address:X}");
        Service.Log.Information($"RMIFly address: 0x{_rmiFlyHook.Address:X}");
    }

    public void Dispose()
    {
        _rmiWalkHook.Dispose();
        _rmiFlyHook.Dispose();
    }

    private void RMIWalkDetour(void* self, float* sumLeft, float* sumForward, float* sumTurnLeft, byte* haveBackwardOrStrafe, byte* a6, byte bAdditiveUnk)
    {
        _rmiWalkHook.Original(self, sumLeft, sumForward, sumTurnLeft, haveBackwardOrStrafe, a6, bAdditiveUnk);

        if (SoftDisable)
            return;

        bool hasMoved = false;
        bool hasTurned = false;

        DateTime currTime = DateTime.Now;

        // TODO: we really need to introduce some extra checks that PlayerMoveController::readInput does - sometimes it skips reading input, and returning something non-zero breaks stuff...
        if (bAdditiveUnk == 0 && (IgnoreUserInput || *sumLeft == 0 && *sumForward == 0))
        {
            // Move towards destination
            if (DirectionToDestination(false) is var relDir && relDir != null)
            {
                var dir = relDir.Value.h.ToDirection();
                *sumLeft = dir.X;
                *sumForward = dir.Y;
                hasMoved = true;
                LastTimeMoved = currTime;
            }

            if ((currTime - LastTimeMoved).TotalSeconds > 0.25f)
            {
                // If we haven't moved and we are told to turn (ShouldTurn) then rotate towards destination
                if (!hasMoved && ShouldTurn && RotationToDestination() is float relRot && relRot != 0)
                {
                    *sumTurnLeft = relRot;
                    hasTurned = true;
                    LastTimeTurned = currTime;
                }

                if ((currTime - LastTimeTurned).TotalSeconds > 0.25f) // 0.25 seconds since last rotated
                {
                    // Disable once we are in range and are facing the right direction
                    if (/*!hasTurned && !hasMoved &&*/ AutoDisable)
                    {
                        SoftDisable = true;
                    }
                }
            }
        }
    }

    private void RMIFlyDetour(void* self, PlayerMoveControllerFlyInput* result)
    {
        _rmiFlyHook.Original(self, result);
        // TODO: we really need to introduce some extra checks that PlayerMoveController::readInput does - sometimes it skips reading input, and returning something non-zero breaks stuff...

        // Fly towards destination
        if ((IgnoreUserInput || result->Forward == 0 && result->Left == 0 && result->Up == 0) && DirectionToDestination(true) is var relDir && relDir != null)
        {
            var dir = relDir.Value.h.ToDirection();
            result->Forward = dir.Y;
            result->Left = dir.X;
            result->Up = relDir.Value.v.Rad;
        }
    }

    private (Angle h, Angle v)? DirectionToDestination(bool allowVertical)
    {
        var player = Service.ClientState.LocalPlayer;
        if (player == null)
            return null;

        var dist = DesiredPosition - player.Position;
        if (dist.LengthSquared() <= Precision * Precision)
            return null;

        var dirH = Angle.FromDirection(dist.X, dist.Z);
        var dirV = allowVertical ? Angle.FromDirection(dist.Y, new Vector2(dist.X, dist.Z).Length()) : default;

        var camera = (CameraEx*)CameraManager.Instance()->GetActiveCamera();
        var cameraDir = camera->DirH.Radians() + 180.Degrees();
        return (dirH - cameraDir, dirV);
    }

    private float RotationToDestination()
    {
        var player = Service.ClientState.LocalPlayer;
        if (player == null)
            return 0;

        var dist = new Angle(DesiredRotation) - new Angle(player.Rotation);
        dist = dist.Normalized();

        if (dist.AlmostEqual(new Angle(0.0f), Meat.ToRad(TurnPrecision)))
            return 0;

        return dist.Rad;
    }
}
