using CosmicFortune.Game;

namespace CosmicFortune;

internal static class Program {
    private static void Main() {
        var game = new Galaxy((1280, 720), "Cosmic Fortune");
        game.Run();
    }
}