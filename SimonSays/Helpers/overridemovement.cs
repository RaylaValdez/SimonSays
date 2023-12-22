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
            if (value)
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

    /// <summary>
    /// Initializes the OverrideMovement instance, sets up hooks, and logs their addresses.
    /// </summary>
    public OverrideMovement()
    {
        // Initialize hooks from attributes
        Service.Hook.InitializeFromAttributes(this);

        // Log the addresses of the walk and fly hooks
        Service.Log.Information($"RMIWalk address: 0x{_rmiWalkHook.Address:X}");
        Service.Log.Information($"RMIFly address: 0x{_rmiFlyHook.Address:X}");
    }


    /// <summary>
    /// Disposes of the hooks related to the walk and fly functions.
    /// </summary>
    public void Dispose()
    {
        // Dispose of the walk and fly hooks
        _rmiWalkHook.Dispose();
        _rmiFlyHook.Dispose();
    }


    /// <summary>
    /// Detour for the PlayerMoveControllerWalkInput function to adjust walk input.
    /// </summary>
    /// <param name="self">Pointer to the PlayerMoveController instance.</param>
    /// <param name="sumLeft">Pointer to the sum of left movement.</param>
    /// <param name="sumForward">Pointer to the sum of forward movement.</param>
    /// <param name="sumTurnLeft">Pointer to the sum of left turn movement.</param>
    /// <param name="haveBackwardOrStrafe">Pointer to a byte indicating backward or strafe movement.</param>
    /// <param name="a6">Pointer to a byte parameter.</param>
    /// <param name="bAdditiveUnk">Byte indicating whether additive unknown parameter is active.</param>
    private void RMIWalkDetour(void* self, float* sumLeft, float* sumForward, float* sumTurnLeft, byte* haveBackwardOrStrafe, byte* a6, byte bAdditiveUnk)
    {
        // Call the original method
        _rmiWalkHook.Original(self, sumLeft, sumForward, sumTurnLeft, haveBackwardOrStrafe, a6, bAdditiveUnk);

        // Return if soft disable is active
        if (SoftDisable)
            return;

        bool hasMoved = false;
        bool hasTurned = false;

        DateTime currTime = DateTime.Now;

        // TODO: Introduce additional checks similar to PlayerMoveController::readInput

        // Check conditions for adjusting walk input
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

            // Check if enough time has passed since the last movement
            if ((currTime - LastTimeMoved).TotalSeconds > 0.25f)
            {
                // If we haven't moved and we are told to turn (ShouldTurn) then rotate towards destination
                if (!hasMoved && ShouldTurn && RotationToDestination() is float relRot && relRot != 0)
                {
                    *sumTurnLeft = relRot;
                    hasTurned = true;
                    LastTimeTurned = currTime;
                }

                // Check if enough time has passed since the last rotation
                if ((currTime - LastTimeTurned).TotalSeconds > 0.25f)
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


    /// <summary>
    /// Detour for the PlayerMoveControllerFlyInput function to adjust fly input.
    /// </summary>
    /// <param name="self">Pointer to the PlayerMoveController instance.</param>
    /// <param name="result">Pointer to the PlayerMoveControllerFlyInput result.</param>
    private void RMIFlyDetour(void* self, PlayerMoveControllerFlyInput* result)
    {
        // Call the original method
        _rmiFlyHook.Original(self, result);

        // TODO: Introduce additional checks similar to PlayerMoveController::readInput

        // Fly towards destination if conditions are met
        if ((IgnoreUserInput || result->Forward == 0 && result->Left == 0 && result->Up == 0) && DirectionToDestination(true) is var relDir && relDir != null)
        {
            // Set fly input based on the calculated direction
            var dir = relDir.Value.h.ToDirection();
            result->Forward = dir.Y;
            result->Left = dir.X;
            result->Up = relDir.Value.v.Rad;
        }
    }


    /// <summary>
    /// Calculates the horizontal and vertical angles from the local player to the desired position.
    /// </summary>
    /// <param name="allowVertical">Flag indicating whether vertical angle calculation is allowed.</param>
    /// <returns>
    /// A tuple containing horizontal and vertical angles, or null if the player is null or already at the destination.
    /// </returns>
    private (Angle h, Angle v)? DirectionToDestination(bool allowVertical)
    {
        // Get the local player
        var player = Service.ClientState.LocalPlayer;

        // Return null if the player is null
        if (player == null)
            return null;

        // Calculate the vector from the player to the desired position
        var dist = DesiredPosition - player.Position;

        // Return null if the player is already at the destination
        if (dist.LengthSquared() <= Precision * Precision)
            return null;

        // Calculate the horizontal and vertical angles
        var dirH = Angle.FromDirection(dist.X, dist.Z);
        var dirV = allowVertical ? Angle.FromDirection(dist.Y, new Vector2(dist.X, dist.Z).Length()) : default;

        // Get the active camera and calculate the camera direction
        var camera = (CameraEx*)CameraManager.Instance()->GetActiveCamera();
        var cameraDir = camera->DirH.Radians() + 180.Degrees();

        // Return a tuple containing horizontal and vertical angles
        return (dirH - cameraDir, dirV);
    }


    /// <summary>
    /// Calculates the rotation needed for the player to align with the desired rotation.
    /// </summary>
    /// <returns>The calculated rotation in radians.</returns>
    private float RotationToDestination()
    {
        // Get the local player
        var player = Service.ClientState.LocalPlayer;

        // Return 0 if the player is null
        if (player == null)
            return 0;

        // Calculate the angular distance between the desired rotation and the player's current rotation
        var dist = new Angle(DesiredRotation) - new Angle(player.Rotation);
        dist = dist.Normalized();

        // Check if the angular distance is almost zero within the specified turn precision
        if (dist.AlmostEqual(new Angle(0.0f), AngleConversions.ToRad(TurnPrecision)))
            return 0;

        // Return the calculated rotation in radians
        return dist.Rad;
    }

}
