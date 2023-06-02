using CosmicFortune.Common;
using CosmicFortune.Game;

namespace CosmicFortune;

internal static class Program {
    private static void Main() {
        if (File.Exists(Galaxy.SeedFile)) {
            LehmerRand.Seed = uint.Parse(File.ReadAllText(Galaxy.SeedFile));
        }

        var game = new Galaxy((1280, 720), "test");
        game.Run();
    }
}