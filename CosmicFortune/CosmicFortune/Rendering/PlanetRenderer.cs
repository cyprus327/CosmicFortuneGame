using CosmicFortune.Game;
using CosmicFortune.Game.Objects;

namespace CosmicFortune.Rendering;

internal static class PlanetRenderer {
    private static readonly(int w, int h) _tileSize = (40, 20);
    private static readonly Bitmap _blankTile = (Bitmap)Image.FromFile($"{Galaxy.TilesPath}blankTile.png");
    private static readonly Bitmap _selectorTile = (Bitmap)Image.FromFile($"{Galaxy.TilesPath}selectorTile.png");
    private static readonly Bitmap[] _coloredTiles = {                               // access indexes
        (Bitmap)Image.FromFile($"{Galaxy.TilesPath}grassTile1.png"), // 1
        (Bitmap)Image.FromFile($"{Galaxy.TilesPath}grassTile2.png"), // 2
        (Bitmap)Image.FromFile($"{Galaxy.TilesPath}grassTile3.png"), // 3
        (Bitmap)Image.FromFile($"{Galaxy.TilesPath}grassTile4.png"), // 4
        (Bitmap)Image.FromFile($"{Galaxy.TilesPath}grassTile5.png"), // 5
        (Bitmap)Image.FromFile($"{Galaxy.TilesPath}dirtTile1.png"),  // 6
        (Bitmap)Image.FromFile($"{Galaxy.TilesPath}stoneTile1.png"), // 7
    };

    public static void DrawPlanet(this Graphics g, in Planet selectedPlanet, (float x, float y) offset, in (int x, int y) coords) {
        if (selectedPlanet == null) return;

        g.Clear(selectedPlanet.Col);

        (int, int) toScreen(int x, int y) =>
            (((int)offset.x * _tileSize.w) + (x - y) * (_tileSize.w / 2),
             ((int)offset.y * _tileSize.h) + (x + y) * (_tileSize.h / 2));

        int worldSize = (int)selectedPlanet.Diameter;

        (int x, int y) selected = toScreen(coords.x, coords.y);

        for (int y = 0; y < worldSize; y++) {
            for (int x = 0; x < worldSize; x++) {
                (int x, int y) sCoord = toScreen(x, y);

                int ind = y * worldSize + x;
                switch (selectedPlanet[ind].TileInd) {
                    case 0:
                        g.DrawImageUnscaled(_blankTile, sCoord.x, sCoord.y);
                        break;
                    case 4:
                    case 5:
                        g.DrawImageUnscaled(_coloredTiles[selectedPlanet[ind].TileInd - 1], sCoord.x, sCoord.y - 20);
                        break;
                    default:
                        g.DrawImageUnscaled(_coloredTiles[selectedPlanet[ind].TileInd - 1], sCoord.x, sCoord.y);
                        break;
                }
            }
        }

        g.DrawImageUnscaled(_selectorTile, selected.x, selected.y);

        PlanetChunk ch = selectedPlanet[coords.y * worldSize + coords.x];

        double max = 4d;
        double total = Math.Clamp(ch.Water + ch.Foliage + ch.Minerals + ch.Gases, 0d, max);
        g.FillRectangle(Brushes.Red, 10, 10, (int)(max * 40d), (int)max * 4);
        g.FillRectangle(Brushes.Green, 10, 10, (int)(total * 40d), (int)max * 4);

        string planetInfo = $"Planet Info:\n" +
            $" Water: {ch.Water :F4}\n" +
            $" Foliage: {ch.Foliage :F4}\n" +
            $" Minerals: {ch.Minerals:F4}\n" +
            $" Gases: {ch.Gases:F4}";
        g.DrawString($"Planet Info:\n{planetInfo}", Materials.InfoFont, Materials.WhiteBrush, 10, (int)max * 4 + 20);
    }
}