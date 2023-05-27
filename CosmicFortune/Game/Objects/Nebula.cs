using CosmicFortune.Common;

namespace CosmicFortune.Game.Objects;

internal sealed class Nebula : GalacticBody {
    public Nebula(uint x, uint y, bool generateFullNebula = false) {
        _rand = new LehmerRand((x & 0xFFFF) << 16 | (y & 0xFFFF));

        Coords = (x, y);
        Clouds = new List<Cloud>(75);
        Type = (NebulaType)_rand.Next(0, 5);

        if (!generateFullNebula) return;

        int cloudCount = _rand.Next(50, 75);
        for (int i = 0; i < cloudCount; i++) {
            Color col = Color.FromArgb(180, StarSystem.Colors[_rand.Next(0, StarSystem.Colors.Length)]);
            (int, int) pos = (_rand.Next(10, 60), _rand.Next(10, 60));
            (int, int) size = (_rand.Next(5, 25), _rand.Next(5, 25));
            var cloud = new Cloud(col, pos, size);
        }
    }

    private readonly LehmerRand _rand;

    public NebulaType Type { get; }

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