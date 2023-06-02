using CosmicFortune.Game.Objects;

namespace CosmicFortune.Rendering;

internal static class GalacticRenderer {
    public static void DrawStats(this Graphics g, in Size windowSize, in (double w, double f, double m, double g) stats) {
        int x = windowSize.Width - 200;
        using var bgBrush = new SolidBrush(Color.FromArgb(180, Color.Black));
        g.FillRectangle(bgBrush, x, 10, 175, 500);
        g.DrawRectangle(Materials.WhitePen, x, 10, 175, 500);

        g.DrawString($"Resources:\n" +
            $" Water: {stats.w:F4}\n" +
            $" Foliage: {stats.f:F4}\n" +
            $" Minerals: {stats.m:F4}\n" +
            $" Gases: {stats.g:F4}",
            Materials.InfoFont, Materials.WhiteBrush, x + 8, 20);
    }

    public static void DrawGalaxy(this Graphics g, in Size windowSize, in int sectorSize, in (float x, float y) offset, in (int x, int y) coords, in bool drawSelector = true) {
        int xSectors = windowSize.Width / sectorSize;
        int ySectors = windowSize.Height / sectorSize;

        (uint x, uint y) currentSector;
        for (currentSector.y = 0; currentSector.y < ySectors; currentSector.y++) {
            for (currentSector.x = 0; currentSector.x < xSectors; currentSector.x++) {
                var body = GalacticBody.At(
                    currentSector.x + (uint)offset.x,
                    currentSector.y + (uint)offset.y);

                if (body == null) continue;

                DrawBody(g, body, currentSector, sectorSize);

                if (!(coords.x / sectorSize == currentSector.x && coords.y / sectorSize == currentSector.y)) continue;
                if (!drawSelector) continue;

                g.DrawEllipse(Pens.Yellow,
                    x: currentSector.x * sectorSize + (sectorSize / 2) - ((sectorSize - 4) / 2),
                    y: currentSector.y * sectorSize + (sectorSize / 2) - ((sectorSize - 4) / 2),
                    width: sectorSize - 4,
                    height: sectorSize - 4);
            }
        }

        if (!drawSelector) return;
        g.DrawRectangle(Pens.Red, coords.x, coords.y, sectorSize, sectorSize);
    }

    private static void DrawBody(in Graphics g, in GalacticBody body, in (uint x, uint y) currentSector, in int sectorSize) {
        if (body == null) return;

        switch (body) {
            case StarSystem:
                DrawStarSystem(g, (StarSystem)body, currentSector, sectorSize);
                break;
            case Nebula:
                DrawNebula(g, (Nebula)body, currentSector, sectorSize);
                break;
            case BlackHole:
                DrawBlackHole(g, currentSector, sectorSize);
                break;
        }
    }

    private static void DrawStarSystem(in Graphics g, in StarSystem system, in (uint x, uint y) currentSector, in int sectorSize) {
        if (!system.StarExists) return;

        using var brush = new SolidBrush(system.StarCol);

        int starW = (int)system.StarDiameter / (int)(sectorSize / 2);
        int starH = (int)system.StarDiameter / (int)(sectorSize / 2);

        g.FillEllipse(brush,
            x: currentSector.x * sectorSize + (int)(sectorSize / 2) - (starW / 2),
            y: currentSector.y * sectorSize + (int)(sectorSize / 2) - (starH / 2),
            width: starW,
            height: starH);
    }

    private static void DrawNebula(in Graphics g, in Nebula nebula, in (uint x, uint y) currentSector, in int sectorSize) {
        if (!nebula.NebulaExists) return;

        using var brush = new SolidBrush(nebula.OverallCol);

        // triangle temporarily symbolizes a nebula
        g.FillPolygon(brush,
            new Point[] {
                new Point((int)(currentSector.x * sectorSize), (int)currentSector.y * sectorSize + sectorSize),
                new Point((int)(currentSector.x * sectorSize + sectorSize / 2), (int)currentSector.y * sectorSize),
                new Point((int)(currentSector.x * sectorSize + sectorSize), (int)(currentSector.y * sectorSize + sectorSize))
            }
        );
    }

    private static void DrawBlackHole(in Graphics g, in (uint x, uint y) currentSector, in int sectorSize) {
        using var pen = new Pen(Color.White);

        g.DrawEllipse(pen,
            (int)currentSector.x * sectorSize,
            (int)currentSector.y * sectorSize,
            sectorSize,
            sectorSize);
    }
}