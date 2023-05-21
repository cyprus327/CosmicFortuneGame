using System.Runtime.InteropServices;

namespace CosmicFortune.Common;

internal static class Input {
    [DllImport("user32.dll")] static extern short GetAsyncKeyState(int key);

    private static Dictionary<char, bool> previousKeyStates = new Dictionary<char, bool>();

    public static bool GetKeyDown(char key) {
        return (GetAsyncKeyState(key) & 0x8000) != 0;
    }

    public static bool GetKeyUp(char key) {
        bool isKeyDown = GetKeyDown(key);
        bool wasKeyDown = previousKeyStates.ContainsKey(key) && previousKeyStates[key];

        previousKeyStates[key] = isKeyDown;

        return wasKeyDown && !isKeyDown;
    }

    public static bool LMBDown() {
        return (GetAsyncKeyState(0x01) & 0x8000) != 0;
    }

    public static bool RMBDown() {
        return (GetAsyncKeyState(0x02) & 0x8000) != 0;
    }
}