namespace CosmicFortune.Common;

internal sealed class LehmerRand {
    public LehmerRand(uint seed) {
        State = seed;
    }

    public uint State { get; private set; } = 0;

    public void Seed(uint seed) {
        State = seed;
    }

    public uint Next() {
        State += 0xE120FC15;
        ulong temp;
        temp = (ulong)State * 0x4A39B70D;
        uint m1 = (uint)((temp >> 32) ^ temp);
        temp = (ulong)m1 * 0x12FAD5C9;
        uint m2 = (uint)((temp >> 32) ^ temp);
        return m2;
    }

    public int Next(int min, int max) {
        return (int)(Next() % (max - min)) + min;
    }

    public double Next(double min, double max) {
        return ((double)Next() / (double)(0x7FFFFFFF)) * (max - min) + min;
    }
}