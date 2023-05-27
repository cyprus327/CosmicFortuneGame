using CosmicFortune.Common;
using System.Collections.Generic;

namespace CosmicFortune.Game.Objects;

internal sealed class StarSystem : GalacticBody {
    public StarSystem(uint x, uint y, bool generateFullSystem = false) {
        _rand = new LehmerRand((x & 0xFFFF) << 16 | (y & 0xFFFF));

        Coords = (x, y);
        Planets = new List<Planet>(12);
        StarExists = _rand.Next(0, 20) == 1;
        if (!StarExists) return;

        StarDiameter = _rand.Next(10d, 40d);
        StarCol = Colors[_rand.Next(0, Colors.Length)];

        if (!generateFullSystem) return;

        double normalize(double val, double min, double max) =>
            Math.Clamp((val - min) / (max - min), 0, 1);

        double distFromStar = _rand.Next(40d, 180d);
        int planetCount = _rand.Next(0, 12);
        for (int i = 0; i < planetCount; i++) {
            // currently completely arbitrary numbers
            double temp = Math.Max(-190d, (StarDiameter * 20 - distFromStar * 1.5d) / 3) - _rand.Next(0d, 20d);
            double dist = distFromStar += _rand.Next(30d, 200d);
            double diam = _rand.Next(4d, 20d);

            double water = 1 / (temp * Math.Sign(temp) == -1 ? -1.5 : 1) * 720 * _rand.Next(0.0, 1.0);
            water = normalize(water, -200, 2000);

            double foliage = temp < 50 ? _rand.Next(0.001, 0.1) : normalize(water * (0.8 + 0.4 * _rand.Next(0.0, 1.0)), -1, 2);

            double minerals = _rand.Next(0.01, 0.99);
            minerals = normalize(minerals, 0.01, 1);

            double gases = _rand.Next(0.0, 1.0) * water;

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