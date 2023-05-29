using CosmicFortune.Game.Objects;

namespace CosmicFortune.Rendering;

internal static class GalacticRenderer {
    public static void DrawStats(in Graphics g, in (double w, double f, double m, double g) stats, in Size windowSize) {
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

    public static void DrawBody(in Graphics g, in GalacticBody body, in (uint x, uint y) currentSector, in int sectorSize) {
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
            });
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