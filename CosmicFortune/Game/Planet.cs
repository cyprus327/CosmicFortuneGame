using CosmicFortune.Common;

namespace CosmicFortune.Game;

internal sealed class Planet {
    public double Dist = 0d;
    public double Diameter = 0d;
    public double Foliage = 0d;
    public double Minerals = 0d;
    public double Water = 0d;
    public double Gases = 0d;
    public double Temp = 0d;
    public double Population = 0d;
    public bool HasRing = false;
    public List<double> Moons = new List<double>();
    public Color Col = Color.HotPink;

    public (int x, int y) Coords;
    public int[]? World = null;

    public void InitializeWorld() {
        var rand = new LehmerRand((uint)((Coords.x & 0xFFFF) << 32 | (Coords.y & 0xFFFF)));

        World = new int[(int)Diameter * (int)Diameter];

        for (int i = 0; i < World.Length; i++) {
            if (Foliage >= 0.7) {
                World[i] = rand.Next(1, 7); // only foliage tiles
            } else if (Minerals >= 0.7) {
                rand.Next(1, 10); // still generate a random number
                World[i] = 7; // stone tile
            } else {
                World[i] = rand.Next(1, 8);
            }
        }
    }
}