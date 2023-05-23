using CosmicFortune.Common;
using CosmicFortune.Game;

namespace CosmicFortune;

internal static class Program {
    private static void Main() {
        LehmerRand.Seed = 44;
        var game = new Galaxy((720, 720), "test");
        game.Run();
    }
}