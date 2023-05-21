using CosmicFortune.Common;

namespace CosmicFortune.Game;

internal sealed class SolarSystem {
    public SolarSystem(uint x, uint y, bool generateFullSystem = false) {
        Coords = (x, y);

        _rand = new LehmerRand((x & 0xFFFF) << 16 | (y & 0xFFFF));
        Planets = new List<Planet>();

        StarExists = _rand.Next(0, 20) == 1;
        if (!StarExists) return;

        StarDiameter = _rand.Next(10d, 40d);
        StarCol = Colors[_rand.Next(0, Colors.Length)];

        if (!generateFullSystem) return;

        double distFromStar = _rand.Next(40d, 180d);
        int planetCount = _rand.Next(0, 12);
        for (int i = 0; i < planetCount; i++) {
            // currently completely arbitrary numbers
            double temp = Math.Max(-200d, StarDiameter * 20d - distFromStar * 1.5d) - _rand.Next(0d, 20d);
            double dist = distFromStar += _rand.Next(30d, 200d);
            double diam = _rand.Next(4d, 20d);

            double water = temp >= -100 ? Math.Max(0, Math.Min(1, (temp + 100) / 100)) : 0;
            double foliage = Math.Min(1.0, _rand.Next(0.6d, 1.5d) * water);
            double minerals = temp >= 0 ? Math.Max(0, Math.Min(1, (_rand.Next(0d, 1d) * 0.5 + 0.3) + (temp < 50 ? 0 : 0.4))) : 0.15;
            double gases = Math.Min(1.0, _rand.Next(0.5d, 1.2d) * minerals);

            double pop = _rand.Next(0d, 100000000d);
            pop *= 1 + (temp / 100d);
            pop *= 1 + foliage;
            pop *= 1 + minerals;
            pop *= 1 + gases;
            pop *= 1 + water;
            pop *= Math.Sign(pop);
            pop *= Math.Sign(_rand.Next(-999, 2));
            pop = 0;//Math.Max(0, pop);

            bool hasRing = _rand.Next(0, 8) == 1;

            byte red = (byte)(gases * 190);
            byte green = (byte)(foliage * 200);
            byte blue = (byte)(water * 240);
            Color col = Color.FromArgb(red, green, blue);

            var p = new Planet() {
                Temp = temp,
                Dist = dist,
                Diameter = diam,
                Foliage = foliage,
                Water = water,
                Minerals = minerals,
                Gases = gases,
                Population = pop,
                HasRing = hasRing,
                Col = col
            };
            
            int moonCount = Math.Max(_rand.Next(-5, 5), 0);
            while (moonCount-- > 0) {
                p.Moons.Add(Math.Max(1d, diam - _rand.Next(1d, 15d)));
            }

            Planets.Add(p);
        }
    }

    private readonly LehmerRand _rand;

    public bool StarExists { get; }
    public double StarDiameter { get; } = 0d;
    public Color StarCol { get; }
    public List<Planet> Planets { get; }
    public (uint x, uint y) Coords { get; }

    public static Color[] Colors = {
        Color.White, Color.Yellow,
        Color.Red, Color.DarkRed, 
        Color.DarkBlue, Color.Cyan,
        Color.Magenta, Color.Purple,
        Color.OrangeRed, Color.Orchid,
        Color.PaleGoldenrod, Color.PaleGreen,
        Color.LightSkyBlue, Color.Orange,
        Color.LightGoldenrodYellow, Color.YellowGreen
    };
}