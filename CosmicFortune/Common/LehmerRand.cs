namespace CosmicFortune.Common;

internal sealed class LehmerRand {
    public LehmerRand(uint seed) {
        state = seed;
    }

    private static uint seed = 1;
    public static uint Seed { 
        get => seed;
        set => seed = value == 0 ? 1 : value;
    }

    private uint state = 0;

    public uint Next() {
        state += 0xE120FC15 * seed;
        ulong temp;
        temp = (ulong)state * 0x4A39B70D;
        uint m1 = (uint)((temp >> 32) ^ temp);
        temp = (ulong)m1 * 0x12FAD5C9;
        uint m2 = (uint)((temp >> 32) ^ temp);
        return m2;
    }

    public int Next(int min, int max) {
        return (int)(Next() % (max - min)) + min;
    }

    public double Next(double min, double max) {
        return ((double)Next() / (double)(uint.MaxValue)) * (max - min) + min;
    }
}