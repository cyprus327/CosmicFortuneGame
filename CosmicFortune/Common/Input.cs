using System.Runtime.InteropServices;

namespace CosmicFortune.Common;

internal static class Input {
    [DllImport("user32.dll")] static extern short GetAsyncKeyState(int key);

    public static bool GetKeyDown(char key) {
        return (GetAsyncKeyState(key) & 0x8000) != 0;
    }

    public static bool LMBDown() {
        return (GetAsyncKeyState(0x01) & 0x8000) != 0;
    }

    public static bool RMBDown() {
        return (GetAsyncKeyState(0x02) & 0x8000) != 0;
    }
}