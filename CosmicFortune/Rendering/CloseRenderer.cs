using CosmicFortune.Game.Objects;

namespace CosmicFortune.Rendering;

internal static class CloseRenderer {
    public static void DrawBody(this Graphics g, in Size windowSize, in GalacticBody? body, in int selectedPlanetInd = 0) {
        if (body == null) return;

        switch (body) {
            case StarSystem:
                DrawStarSystem(g, windowSize, (StarSystem)body, selectedPlanetInd);
                break;
            case Nebula:
                DrawNebula(g, windowSize, (Nebula)body);
                break;
            case BlackHole:
                DrawBlackHole(g, windowSize, (BlackHole)body);
                break;
        }
    }

    private static void DrawStarSystem(in Graphics g, in Size windowSize, in StarSystem system, in int selectedPlanetInd) {
        if (!system.StarExists) return;

        int planetCount = system.Planets.Count;
        var bgCol = Color.FromArgb(220, Color.Black);
        using var bgBrush = new SolidBrush(bgCol);
        g.FillRectangle(bgBrush, 0, 0, windowSize.Width, windowSize.Height);

        string systemInfoStr =
            $"System Info:\n" +
            $" Star Size: {system.StarDiameter:F2}\n" +
            $" Star Color: {system.StarCol.Name}\n" +
            $" Planets: {system.Planets.Count}";
        g.DrawString(systemInfoStr, Materials.InfoFont, Materials.WhiteBrush, 10, 10);

        if (planetCount > 0) {
            Planet selectedPlanet = system.Planets[selectedPlanetInd];
            string planetInfoStr =
                $"Selected Planet Info:\n" +
                $" Distance From Sun: {selectedPlanet.Dist:F4}\n" +
                $" Diameter: {selectedPlanet.Diameter:F4}\n" +
                $" Water: {selectedPlanet.Water:F2}\n" +
                $" Foliage: {selectedPlanet.Foliage:F2}\n" +
                $" Minerals: {selectedPlanet.Minerals:F2}\n" +
                $" Gases: {selectedPlanet.Gases:F2}\n" +
                $" Has a ring: {selectedPlanet.HasRing}\n" +
                $" Moons: {selectedPlanet.Moons.Count}";
            g.DrawString(planetInfoStr, Materials.InfoFont, Materials.WhiteBrush, 260, 10);
        }

        using var starBrush = new SolidBrush(system.StarCol);
        (float x, float y) body = (6f, 356f);
        float size = (float)(system.StarDiameter * 4f);
        g.FillEllipse(starBrush, body.x, body.y - size / 2, size, size);
        body.x += size + 18f;

        for (int i = 0; i < planetCount; i++) {
            Planet planet = system.Planets[i];
            float di = (float)planet.Diameter, di2 = di / 2;
            if (body.x >= windowSize.Width - 32f - di) break;

            body.x += di;

            if (planet.HasRing) {
                float ringSize = di * 1.5f;
                float diff = (ringSize - di) / 2;
                g.DrawEllipse(Pens.Gray, body.x - di2 - diff, body.y - di2 - diff, ringSize, ringSize);
            }

            using var planetBrush = new SolidBrush(planet.Col);
            g.FillEllipse(planetBrush, body.x - di2, body.y - di2, di, di);
            (float x, float y) moonCoords = body;
            moonCoords.y += di + 2f;
            foreach (var moon in planet.Moons) {
                moonCoords.y += (float)moon;
                g.FillEllipse(Brushes.DarkGray,
                    moonCoords.x - (float)moon / 2f,
                    moonCoords.y - (float)moon / 2f,
                    (float)moon,
                    (float)moon);
                moonCoords.y += (float)moon + 1f;
            }

            if (i == selectedPlanetInd) {
                g.DrawEllipse(Pens.Red, body.x - di2, body.y - di2, di, di);
            }

            body.x += di;
        }
    }

    private static void DrawNebula(in Graphics g, in Size windowSize, in Nebula nebula) {
        var bgCol = Color.FromArgb(220, Color.Black);
        using var bgBrush = new SolidBrush(bgCol);
        g.FillRectangle(bgBrush, 0, 0, windowSize.Width, windowSize.Height);

        (float x, float y) zero = (windowSize.Width / 2 - 180, windowSize.Height / 2 - 180);
        foreach (var cloud in nebula.Clouds) {
            using var brush = new SolidBrush(cloud.Col);
            g.FillEllipse(brush, zero.x + cloud.Pos.x, zero.y + cloud.Pos.y, cloud.Size.w, cloud.Size.h);
        }
    }

    private static void DrawBlackHole(in Graphics g, in Size windowSize, in BlackHole bh) {
        g.DrawEllipse(Materials.WhitePen,
            windowSize.Width / 2 - 240,
            windowSize.Height / 2 - 240,
            480, 480);
    }
}
