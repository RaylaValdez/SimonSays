using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using SimonSays;
using System;
using System.Runtime.InteropServices;

namespace SimonSays.Helpers;

[StructLayout(LayoutKind.Explicit, Size = 0x2B0)]
public unsafe struct CameraEx
{
    [FieldOffset(0x130)] public float DirH; // 0 is north, increases CW
    [FieldOffset(0x134)] public float DirV; // 0 is horizontal, positive is looking up, negative looking down
    [FieldOffset(0x138)] public float InputDeltaHAdjusted;
    [FieldOffset(0x13C)] public float InputDeltaVAdjusted;
    [FieldOffset(0x140)] public float InputDeltaH;
    [FieldOffset(0x144)] public float InputDeltaV;
    [FieldOffset(0x148)] public float DirVMin; // -85deg by default
    [FieldOffset(0x14C)] public float DirVMax; // +45deg by default
}

public unsafe class OverrideCamera : IDisposable
{
    public bool Enabled
    {
        get => rmiCameraHook.IsEnabled;
        set
        {
            if (value)
                rmiCameraHook.Enable();
            else
                rmiCameraHook.Disable();
        }
    }

    public bool IgnoreUserInput; // if true - override even if user tries to change camera orientation, otherwise override only if user does nothing
    public Angle DesiredAzimuth;
    public Angle DesiredAltitude;
    public Angle SpeedH = 360.Degrees(); // per second
    public Angle SpeedV = 360.Degrees(); // per second

    private delegate void RMICameraDelegate(CameraEx* self, int inputMode, float speedH, float speedV);
    [Signature("40 53 48 83 EC 70 44 0F 29 44 24 ?? 48 8B D9")]
    private readonly Hook<RMICameraDelegate> rmiCameraHook = null!;

    /// <summary>
    /// Initializes the OverrideCamera instance, sets up hooks, and logs the address of the RMICamera hook.
    /// </summary>
    public OverrideCamera()
    {
        // Initialize hooks from attributes
        Sausages.Hook.InitializeFromAttributes(this);

        // Log the address of the RMICamera hook
        Sausages.Log.Information($"RMICamera address: 0x{rmiCameraHook.Address:X}");
    }


    /// <summary>
    /// Disposes of the hook related to the CameraEx function.
    /// </summary>
    public void Dispose()
    {
        // Dispose of the camera hook
        rmiCameraHook.Dispose();
        GC.SuppressFinalize(this); 
    }


    /// <summary>
    /// Detour for the CameraEx function to adjust camera input.
    /// </summary>
    /// <param name="self">Pointer to the CameraEx instance.</param>
    /// <param name="inputMode">Input mode parameter.</param>
    /// <param name="speedH">Horizontal speed parameter.</param>
    /// <param name="speedV">Vertical speed parameter.</param>
    private void RMICameraDetour(CameraEx* self, int inputMode, float speedH, float speedV)
    {
        // Call the original method
        rmiCameraHook.Original(self, inputMode, speedH, speedV);

        // Adjust camera input if conditions are met
        if (IgnoreUserInput || inputMode == 0) // Let the user override...
        {
            // Calculate delta values for horizontal and vertical directions
            var dt = Framework.Instance()->FrameDeltaTime;
            var deltaH = (DesiredAzimuth - self->DirH.Radians()).Normalized();
            var deltaV = (DesiredAltitude - self->DirV.Radians()).Normalized();

            // Calculate maximum allowable changes based on speed and frame delta time
            var maxH = SpeedH.Rad * dt;
            var maxV = SpeedV.Rad * dt;

            // Clamp delta values to stay within the allowable range
            self->InputDeltaH = Math.Clamp(deltaH.Rad, -maxH, maxH);
            self->InputDeltaV = Math.Clamp(deltaV.Rad, -maxV, maxV);
        }
    }

}
