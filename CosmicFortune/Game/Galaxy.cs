using CosmicFortune.Common;
using CosmicFortune.Rendering;

namespace CosmicFortune.Game;

internal sealed class Galaxy : Engine {
    public Galaxy((int x, int y) windowSize, string windowTitle) : base(windowSize, windowTitle) { }

    private const int SECTORSIZE = 16;

    private (int x, int y) selectedCoords = (0, 0);
    private SolarSystem? selectedSystem = null; 
    private int selectedPlanetInd = 0;

    private Planet? selectedPlanet = null;

    private (float x, float y) universeOffset = (0f, 0f);
    private (int x, int y) OffsetSelected => (selectedCoords.x + (int)universeOffset.x * SECTORSIZE, selectedCoords.y + (int)universeOffset.y * SECTORSIZE);

    private const float UNIVERSE_NAV_SPEED = 30f;
    private float moveCooldown = 0f;

    private readonly HashSet<char> _keysHeld = new HashSet<char>();

    private readonly Font _infoFont = new Font("Arial", 12);

    private readonly Bitmap _tiles = (Bitmap)Image.FromFile("isometricTiles.png");
    private readonly (int w, int h) _tileSize = (40, 20);
    private readonly (int x, int y) _origin = (5, 1);

    public override void Awake() {
        BackgroundColor = Color.Black;
    }

    public override void Update(in Graphics g, in float deltaTime) {
        UpdateKeysHeld();
        ApplyKeysHeld(deltaTime);

        selectedCoords.x = Math.Max(Math.Min(WindowSize.Width, selectedCoords.x), 0);
        selectedCoords.y = Math.Max(Math.Min(WindowSize.Height, selectedCoords.y), 0);

        int xSectors = WindowSize.Width / SECTORSIZE;
        int ySectors = WindowSize.Height / SECTORSIZE;

        (uint x, uint y) currentSector;
        for (currentSector.y = 0; currentSector.y < ySectors; currentSector.y++) {
            for (currentSector.x = 0; currentSector.x < xSectors; currentSector.x++) {
                var system = new SolarSystem(
                    currentSector.x + (uint)universeOffset.x, 
                    currentSector.y + (uint)universeOffset.y);

                if (!system.StarExists) continue;

                using var brush = new SolidBrush(system.StarCol);

                int starW = (int)system.StarDiameter / (SECTORSIZE / 2);
                int starH = (int)system.StarDiameter / (SECTORSIZE / 2);

                g.FillEllipse(brush, 
                    x: currentSector.x * SECTORSIZE + (SECTORSIZE / 2) - (starW / 2), 
                    y: currentSector.y * SECTORSIZE + (SECTORSIZE / 2) - (starH / 2),
                    width: starW, 
                    height: starH);

                if (!(selectedCoords.x / SECTORSIZE == currentSector.x && selectedCoords.y / SECTORSIZE == currentSector.y)) continue;

                g.DrawEllipse(Pens.Yellow,
                    x: currentSector.x * SECTORSIZE + (SECTORSIZE / 2) - ((SECTORSIZE - 4) / 2),
                    y: currentSector.y * SECTORSIZE + (SECTORSIZE / 2) - ((SECTORSIZE - 4) / 2),
                    width: SECTORSIZE - 4,
                    height: SECTORSIZE - 4);
            }
        }

        g.DrawRectangle(Pens.Red, selectedCoords.x, selectedCoords.y, SECTORSIZE, SECTORSIZE);

        g.DrawString($"{selectedSystem?.Planets.Count}", SystemFonts.DefaultFont, Brushes.White, 0f, 0f);

        if (_keysHeld.Contains(' ')) {
            UpdateSelectedSystem();
        }
        if (_keysHeld.Contains((char)13)) {
            UpdateSelectedPlanet();
        }
        RenderSelectedSystem(g);
        RenderSelectedPlanet(g);
    }

    private void UpdateSelectedSystem() {
        uint x = (uint)(OffsetSelected.x / SECTORSIZE), y = (uint)(OffsetSelected.y / SECTORSIZE);
        var system = new SolarSystem(x, y, false);
        
        selectedPlanetInd = 0;

        if (!system.StarExists) {
            selectedSystem = null;
            return;
        }

        selectedSystem = new SolarSystem(x, y, true);
    }

    private void RenderSelectedSystem(in Graphics g) {
        if (selectedSystem == null) return;

        var bgCol = Color.FromArgb(180, Color.Black);
        using var bgBrush = new SolidBrush(bgCol);
        g.FillRectangle(bgBrush, 0, 0, WindowSize.Width, WindowSize.Height);

        string systemInfoStr =
            $"System Info:\n" +
            $" Star Size: {selectedSystem.StarDiameter:F2}\n" +
            $" Star Color: {selectedSystem.StarCol.Name}\n" +
            $" Planets: {selectedSystem.Planets.Count}";
        g.DrawString(systemInfoStr, _infoFont, Brushes.White, 10, 10);

        int planetCount = selectedSystem.Planets.Count;
        if (planetCount > 0) {
            Planet selectedPlanet = selectedSystem.Planets[selectedPlanetInd];
            string planetInfoStr =
                $"Selected Planet Info:\n" +
                $" Distance From Sun: {selectedPlanet.Dist:F4}\n" +
                $" Diameter: {selectedPlanet.Diameter:F4}\n" +
                $" Ambient Temperature (F): {selectedPlanet.Temp:F4}\n" +
                $" Water: {selectedPlanet.Water:F2}\n" +
                $" Foliage: {selectedPlanet.Foliage:F2}\n" +
                $" Minerals: {selectedPlanet.Minerals:F2}\n" +
                $" Gases: {selectedPlanet.Gases:F2}\n" +
                $" Population: {selectedPlanet.Population}\n" +
                $" Has a ring: {selectedPlanet.HasRing}\n" +
                $" Moons: {selectedPlanet.Moons.Count}";
            g.DrawString(planetInfoStr, _infoFont, Brushes.White, 260, 10);
        }

        using var starBrush = new SolidBrush(selectedSystem.StarCol);
        (float x, float y) body = (6f, 356f);
        float size = (float)(selectedSystem.StarDiameter * 4f);
        g.FillEllipse(starBrush, body.x, body.y - size / 2, size, size);
        body.x += size + 18f;

        for (int i = 0; i < planetCount; i++) {
            Planet planet = selectedSystem.Planets[i];
            float di = (float)planet.Diameter, di2 = di / 2;
            if (body.x >= WindowSize.Width - 32f - di) break;

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

    private void UpdateSelectedPlanet() {
        if (selectedSystem == null) return;
        
        selectedPlanet = selectedSystem.Planets[selectedPlanetInd];
    }

    private void RenderSelectedPlanet(in Graphics g) {
        if (selectedPlanet == null) return;

        g.Clear(Color.White);

        (int, int) toScreen(int x, int y) =>
            ((_origin.x * _tileSize.w) + (x - y) * (_tileSize.w / 2),
             (_origin.y * _tileSize.h) + (x + y) * (_tileSize.h / 2));

        (int x, int y) worldSize = ((int)selectedPlanet.Diameter, (int)selectedPlanet.Diameter);
        int[] world = new int[worldSize.x * worldSize.y];

        for (int y = 0; y < worldSize.y; y++) {
            for (int x = 0; x < worldSize.x; x++) {
                (int x, int y) sCoord = toScreen(x, y);

                switch (world[y * worldSize.x + x]) {
                    case 0: // invisible tile
                        g.DrawImage(_tiles, sCoord.x, sCoord.y, new Rectangle(40, 0, 40, 20), GraphicsUnit.Pixel);
                        break;
                }
            }
        }
    }

    private void UpdateKeysHeld() {
        _keysHeld.Clear();

        if (Input.GetKeyDown('W')) _keysHeld.Add('W');
        if (Input.GetKeyDown('A')) _keysHeld.Add('A');
        if (Input.GetKeyDown('S')) _keysHeld.Add('S');
        if (Input.GetKeyDown('D')) _keysHeld.Add('D');

        if (Input.GetKeyDown('I')) _keysHeld.Add('I');
        if (Input.GetKeyDown('J')) _keysHeld.Add('J');
        if (Input.GetKeyDown('K')) _keysHeld.Add('K');
        if (Input.GetKeyDown('L')) _keysHeld.Add('L');

        if (Input.GetKeyDown(' ')) _keysHeld.Add(' ');
        if (Input.GetKeyDown((char)27)) _keysHeld.Add((char)27); // escape
        if (Input.GetKeyDown((char)13)) _keysHeld.Add((char)13); // enter
    }

    private void ApplyKeysHeld(in float deltaTime) {
        if (_keysHeld.Contains('W')) universeOffset.y -= UNIVERSE_NAV_SPEED * deltaTime;
        if (_keysHeld.Contains('A')) universeOffset.x -= UNIVERSE_NAV_SPEED * deltaTime;
        if (_keysHeld.Contains('S')) universeOffset.y += UNIVERSE_NAV_SPEED * deltaTime;
        if (_keysHeld.Contains('D')) universeOffset.x += UNIVERSE_NAV_SPEED * deltaTime;

        
        moveCooldown += deltaTime;
        if (moveCooldown <= 0.15f) return;
        
        if (_keysHeld.Contains((char)27)) {
            if (selectedPlanet != null) {
                selectedPlanet = null;
            } else {
                selectedSystem = null; 
            }
        }
        
        // if a system is selected use ijkl (j and l) to navigate through planets
        if (selectedSystem != null) {
            if (_keysHeld.Contains('J')) selectedPlanetInd--;
            if (_keysHeld.Contains('L')) selectedPlanetInd++;
            int planetCount = selectedSystem.Planets.Count;
            selectedPlanetInd = 
                selectedPlanetInd < 0 ? planetCount - 1 : 
                planetCount > 0 ? selectedPlanetInd % planetCount : selectedPlanetInd;
        } else {
            if (_keysHeld.Contains('I')) selectedCoords.y -= SECTORSIZE;
            if (_keysHeld.Contains('J')) selectedCoords.x -= SECTORSIZE;
            if (_keysHeld.Contains('K')) selectedCoords.y += SECTORSIZE;
            if (_keysHeld.Contains('L')) selectedCoords.x += SECTORSIZE;
        }

        moveCooldown = 0f;
    }
}