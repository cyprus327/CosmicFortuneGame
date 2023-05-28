using CosmicFortune.Common;

namespace CosmicFortune.Game.Objects;

internal abstract class GalacticBody {
    public static GalacticBody? At(uint x, uint y) {
        var rand = new LehmerRand((x & 0xFFFF) << 16 | (y & 0xFFFF));

        if (rand.Next(0, 20) != 1) return null;

        return rand.Next(0, 500) switch {
            0 => new BlackHole(x, y),
            > 0 and <= 15 => new Nebula(x, y, false),
            _ => new StarSystem(x, y, false)
        };
    }
}