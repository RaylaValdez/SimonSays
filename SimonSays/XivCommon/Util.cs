using System;
using System.Collections.Generic;
using Dalamud.Game.Text.SeStringHandling;

namespace XivCommon; 

internal static class Util {
    internal static byte[] Terminate(this byte[] array) {
        var terminated = new byte[array.Length + 1];
        Array.Copy(array, terminated, array.Length);
        terminated[^1] = 0;

        return terminated;
    }

    internal static unsafe byte[] ReadTerminated(IntPtr memory) {
        if (memory == IntPtr.Zero) {
            return Array.Empty<byte>();
        }

        var buf = new List<byte>();

        var ptr = (byte*) memory;
        while (*ptr != 0) {
            buf.Add(*ptr);
            ptr += 1;
        }

        return buf.ToArray();
    }

    internal static SeString ReadSeString(IntPtr memory) {
        var terminated = ReadTerminated(memory);
        return SeString.Parse(terminated);
    }

    internal static void PrintMissingSig(string name) {
        Logger.Log.Warning($"Could not find signature for {name}. This functionality will be disabled.");
    }

    internal static unsafe IntPtr FollowPointerChain(IntPtr start, IEnumerable<int> offsets) {
        if (start == IntPtr.Zero) {
            return IntPtr.Zero;
        }

        foreach (var offset in offsets) {
            start = *(IntPtr*) (start + offset);
            if (start == IntPtr.Zero) {
                return IntPtr.Zero;
            }
        }

        return start;
    }
}