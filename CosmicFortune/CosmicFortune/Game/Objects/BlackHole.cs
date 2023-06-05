namespace CosmicFortune.Game.Objects;

internal sealed class BlackHole : GalacticBody {
    public BlackHole(uint x, uint y) {
        Coords = (x, y);
    }

    public (uint x, uint y) Coords { get; }
}
