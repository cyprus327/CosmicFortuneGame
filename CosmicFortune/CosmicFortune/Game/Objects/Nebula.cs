using CosmicFortune.Common;

namespace CosmicFortune.Game.Objects;

internal sealed class Nebula : GalacticBody {
    public Nebula(uint x, uint y, bool generateFullNebula = false) {
        _rand = new LehmerRand((x & 0xFFFF) << 16 | (y & 0xFFFF));

        Coords = (x, y);
        Clouds = new List<Cloud>(150);
        NebulaExists = _rand.Next(0, 20) == 1;
        if (!NebulaExists) return;
        
        Size = _rand.Next(100d, 400d);
        Type = (NebulaType)_rand.Next(0, 5);
        (Color s, Color e) cols = Type switch {
            NebulaType.Reflection => (Color.Blue, Color.Cyan),
            NebulaType.Dark => (Color.DarkViolet, Color.Red),
            NebulaType.Supernova => (Color.Cyan, Color.Yellow),
            NebulaType.Emission => (Color.Red, Color.Orange),
            NebulaType.Planetary => (Color.LightCoral, Color.DarkBlue),
            _ => (Color.HotPink, Color.HotPink)
        };
        OverallCol = _rand.Next(0, 2) == 1 ? cols.s : cols.e;

        if (!generateFullNebula) return;

        int cloudCount = _rand.Next(75, 150);
        for (int i = 0; i < cloudCount; i++) {
            Color col = Color.FromArgb(180, _rand.Next(0, 2) == 1 ? cols.s : cols.e);
            (int, int) pos = (_rand.Next(0, 180), _rand.Next(0, 180));
            (int, int) size = (_rand.Next(20, 40), _rand.Next(20, 40));
            Clouds.Add(new Cloud(col, pos, size));
        }
    }

    private readonly LehmerRand _rand;

    public bool NebulaExists { get; }
    public double Size { get; }
    public NebulaType Type { get; }
    public Color OverallCol { get; }
    public List<Cloud> Clouds { get; }
    public (uint x, uint y) Coords { get; }
}

internal sealed record class Cloud(Color Col, (int x, int y) Pos, (int w, int h) Size);

internal enum NebulaType {
    Reflection,
    Dark,
    Supernova,
    Emission,
    Planetary
}