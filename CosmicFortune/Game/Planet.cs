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
}