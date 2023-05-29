using CosmicFortune.Common;

namespace CosmicFortune.Game.Objects;

internal sealed class StarSystem : GalacticBody {
    public StarSystem(uint x, uint y, bool generateFullSystem = false) {
        _rand = new LehmerRand((x & 0xFFFF) << 16 | (y & 0xFFFF));

        Coords = (x, y);
        Planets = new List<Planet>(12);
        
        StarExists = _rand.Next(0, 20) == 1;
        if (!StarExists) return;

        StarDiameter = _rand.Next(20d, 60d);
        StarCol = Colors[_rand.Next(0, Colors.Length)];

        if (!generateFullSystem) return;

        double distFromStar = _rand.Next(40d, 180d);
        int planetCount = _rand.Next(0, 12);
        for (int i = 0; i < planetCount; i++) {
            // currently completely arbitrary numbers
            double dist = distFromStar += _rand.Next(30d, 200d);
            double diam = _rand.Next(4d, 20d);

            double water = Math.Max(0, _rand.Next(0d, 5d) - 4d);
            double foliage = _rand.Next(0d, 1d) * water;
            double minerals = _rand.Next(0.2, 1d);
            double gases = Math.Clamp(_rand.Next(0d, 1d) + water * 0.5, 0d, 1d);

            bool hasRing = _rand.Next(0, 8) == 1;

            (double r, double g, double b) rands = (
                Math.Min(1, _rand.Next(0d, 1.5d)),
                Math.Min(1, _rand.Next(0d, 1.5d)),
                Math.Min(1, _rand.Next(0d, 1.5d)));
            byte red = (byte)((gases + minerals) / 2 * 190 * rands.r);
            byte green = (byte)(foliage * 240 * rands.g);
            byte blue = (byte)(water * 240 * rands.b);
            Color col = (water + foliage) switch {
                > 0.4 => Color.FromArgb(red / 2, green, blue),
                _ => Color.FromArgb((byte)(rands.r * 100d), (byte)(rands.g * 100d), (byte)(rands.b * 180d))
            };

            var p = new Planet() {
                Dist = dist,
                Diameter = diam,
                Foliage = foliage,
                Water = water,
                Minerals = minerals,
                Gases = gases,
                HasRing = hasRing,
                Col = col,
                Coords = ((int)(Coords.x + dist), (int)(Coords.y + dist))
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