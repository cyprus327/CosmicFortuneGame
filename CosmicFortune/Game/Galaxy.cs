using CosmicFortune.Common;
using CosmicFortune.Rendering;
using CosmicFortune.Game.Objects;

namespace CosmicFortune.Game;

internal sealed class Galaxy : Engine {
    public Galaxy((int x, int y) windowSize, string windowTitle) : base(windowSize, windowTitle) { }

    private const int SECTORSIZE = 16;

    private (int x, int y) universeSelectedCoords = (0, 0);
    private (int x, int y) planetSelectedCoords = (0, 0);

    private GalacticBody? selectedBody = null; 
    private Planet? selectedPlanet = null;
    private int selectedPlanetInd = 0;

    private const float UNIVERSE_NAV_SPEED = 30f;
    private const float PLANET_NAV_SPEED = 12f;
    private (float x, float y) universeOffset = (0f, 0f);
    private (float x, float y) planetOffset = (10, 5);
    private (int x, int y) OffsetSelected => (universeSelectedCoords.x + (int)universeOffset.x * SECTORSIZE, universeSelectedCoords.y + (int)universeOffset.y * SECTORSIZE);

    private float moveCooldown = 0f;

    private readonly Font _infoFont = new Font("Arial", 12);
    private readonly Brush _whiteBrush = Brushes.White;

    private readonly (int w, int h) _tileSize = (40, 20);
    private readonly Bitmap _blankTile = (Bitmap)Image.FromFile($"Tiles{Path.DirectorySeparatorChar}blankTile.png");
    private readonly Bitmap _selectorTile = (Bitmap)Image.FromFile($"Tiles{Path.DirectorySeparatorChar}selectorTile.png");
    private readonly Bitmap[] _coloredTiles = {                                      // access indexes
        (Bitmap)Image.FromFile($"Tiles{Path.DirectorySeparatorChar}grassTile1.png"), // 1
        (Bitmap)Image.FromFile($"Tiles{Path.DirectorySeparatorChar}grassTile2.png"), // 2
        (Bitmap)Image.FromFile($"Tiles{Path.DirectorySeparatorChar}grassTile3.png"), // 3
        (Bitmap)Image.FromFile($"Tiles{Path.DirectorySeparatorChar}grassTile4.png"), // 4
        (Bitmap)Image.FromFile($"Tiles{Path.DirectorySeparatorChar}grassTile5.png"), // 5
        (Bitmap)Image.FromFile($"Tiles{Path.DirectorySeparatorChar}dirtTile1.png"),  // 6
        (Bitmap)Image.FromFile($"Tiles{Path.DirectorySeparatorChar}stoneTile1.png"), // 7
    };

    private readonly string _resourcesFile = $"SaveData{Path.DirectorySeparatorChar}resources.txt";
    private (double w, double f, double m, double g) totalResources;

    private bool uiEnabled = true;

    ~Galaxy() {        
        _infoFont.Dispose();
        _whiteBrush.Dispose();
        _blankTile.Dispose();
        _selectorTile.Dispose();
        foreach (var tile in _coloredTiles) {
            tile.Dispose();
        }
    }

    public override void Awake() {
        BackgroundColor = Color.Black;

        (double, double, double, double) parseResources() {
            string[] data = File.ReadAllText(_resourcesFile).Trim('(', ')').Split(", ");
            return (double.Parse(data[0]), double.Parse(data[1]), double.Parse(data[2]), double.Parse(data[3]));
        }

        totalResources = File.Exists(_resourcesFile) ? parseResources() : (0d, 0d, 0d, 0d);
    }

    public override void Update(in Graphics g, in float deltaTime) {
        HandleInput(deltaTime);

        universeSelectedCoords.x = Math.Clamp(universeSelectedCoords.x, 0, WindowSize.Width - SECTORSIZE * 3);
        universeSelectedCoords.y = Math.Clamp(universeSelectedCoords.y, 0, WindowSize.Height - SECTORSIZE * 3);

        if (selectedPlanet != null) {
            RenderSelectedPlanet(g);
        } else if (selectedBody != null) {
            RenderSelectedSystem(g);
        } else {
            RenderGalaxy(g);
        }

        RenderStats(g);

        if ((int)Time.ElapsedTime % 10 == 0) {
            File.WriteAllText(_resourcesFile, totalResources.ToString());
        }
    }

    private void RenderStats(in Graphics g) {
        if (!uiEnabled) return;

        int x = WindowSize.Width - 200;
        using var bgBrush = new SolidBrush(Color.FromArgb(180, Color.Black));
        g.FillRectangle(bgBrush, x, 10, 175, 500);
        g.DrawRectangle(Pens.White, x, 10, 175, 500);

        g.DrawString($"Resources:\n" +
            $" Water: {totalResources.w:F4}\n" +
            $" Foliage: {totalResources.f:F4}\n" +
            $" Minerals: {totalResources.m:F4}\n" +
            $" Gases: {totalResources.g:F4}",
            _infoFont, _whiteBrush, x + 8, 20);
    }

    private void RenderGalaxy(in Graphics g) {
        if (selectedPlanet != null) return;

        int xSectors = WindowSize.Width / SECTORSIZE;
        int ySectors = WindowSize.Height / SECTORSIZE;

        (uint x, uint y) currentSector;
        for (currentSector.y = 0; currentSector.y < ySectors; currentSector.y++) {
            for (currentSector.x = 0; currentSector.x < xSectors; currentSector.x++) {
                var system = new StarSystem(
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

                if (!(universeSelectedCoords.x / SECTORSIZE == currentSector.x && universeSelectedCoords.y / SECTORSIZE == currentSector.y)) continue;
                if (!uiEnabled) continue;

                g.DrawEllipse(Pens.Yellow,
                    x: currentSector.x * SECTORSIZE + (SECTORSIZE / 2) - ((SECTORSIZE - 4) / 2),
                    y: currentSector.y * SECTORSIZE + (SECTORSIZE / 2) - ((SECTORSIZE - 4) / 2),
                    width: SECTORSIZE - 4,
                    height: SECTORSIZE - 4);
            }
        }

        if (!uiEnabled) return;

        g.DrawRectangle(Pens.Red, universeSelectedCoords.x, universeSelectedCoords.y, SECTORSIZE, SECTORSIZE);
    }

    private void RenderSelectedSystem(in Graphics g) {
        if (selectedBody == null) return;
        if (selectedBody is not StarSystem system) return;

        int planetCount = system.Planets.Count;
        if (uiEnabled) {
            var bgCol = Color.FromArgb(180, Color.Black);
            using var bgBrush = new SolidBrush(bgCol);
            g.FillRectangle(bgBrush, 0, 0, WindowSize.Width, WindowSize.Height);

            string systemInfoStr =
                $"System Info:\n" +
                $" Star Size: {system.StarDiameter:F2}\n" +
                $" Star Color: {system.StarCol.Name}\n" +
                $" Planets: {system.Planets.Count}";
            g.DrawString(systemInfoStr, _infoFont, _whiteBrush, 10, 10);

            if (planetCount > 0) {
                Planet selectedPlanet = system.Planets[selectedPlanetInd];
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
                g.DrawString(planetInfoStr, _infoFont, _whiteBrush, 260, 10);
            }
        }

        using var starBrush = new SolidBrush(system.StarCol);
        (float x, float y) body = (6f, 356f);
        float size = (float)(system.StarDiameter * 4f);
        g.FillEllipse(starBrush, body.x, body.y - size / 2, size, size);
        body.x += size + 18f;

        for (int i = 0; i < planetCount; i++) {
            Planet planet = system.Planets[i];
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
    
    private void RenderSelectedPlanet(in Graphics g) {
        if (selectedPlanet == null) return;

        g.Clear(selectedPlanet.Col);

        (int, int) toScreen(int x, int y) =>
            (((int)planetOffset.x * _tileSize.w) + (x - y) * (_tileSize.w / 2),
             ((int)planetOffset.y * _tileSize.h) + (x + y) * (_tileSize.h / 2));

        int worldSize = (int)selectedPlanet.Diameter;

        (int x, int y) selected = toScreen(planetSelectedCoords.x, planetSelectedCoords.y);

        for (int y = 0; y < worldSize; y++) {
            for (int x = 0; x < worldSize; x++) {
                (int x, int y) sCoord = toScreen(x, y);

                int ind = y * worldSize + x;
                switch (selectedPlanet[ind].TileInd) {
                    case 0:
                        g.DrawImageUnscaled(_blankTile, sCoord.x, sCoord.y);
                        break;
                    case 3:
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

        if (!uiEnabled) return;

        g.DrawImageUnscaled(_selectorTile, selected.x, selected.y);

        PlanetChunk ch = selectedPlanet[planetSelectedCoords.y * worldSize + planetSelectedCoords.x];

        double max = 4d;
        double total = Math.Clamp(ch.Water + ch.Foliage + ch.Minerals + ch.Gases, 0d, max);
        g.FillRectangle(Brushes.Red, 10, 10, (int)(max * 40d), (int)max * 4);
        g.FillRectangle(Brushes.Green, 10, 10, (int)(total * 40d), (int)max * 4);

        string planetInfo = $"Planet Info:\n" +
            $" Water: {ch.Water :F4}\n" +
            $" Foliage: {ch.Foliage :F4}\n" +
            $" Minerals: {ch.Minerals:F4}\n" +
            $" Gases: {ch.Gases:F4}";
        g.DrawString($"Planet Info:\n{planetInfo}", _infoFont, _whiteBrush, 10, (int)max * 4 + 20);
    }

    private void UpdateSelectedSystem() {
        uint x = (uint)(OffsetSelected.x / SECTORSIZE), y = (uint)(OffsetSelected.y / SECTORSIZE);
        var system = new StarSystem(x, y, false);
        
        selectedPlanetInd = 0;

        if (!system.StarExists) {
            selectedBody = null;
            return;
        }

        selectedBody = new StarSystem(x, y, true);
    }

    private void UpdateSelectedPlanet() {
        if (selectedBody == null) return;
        if (selectedBody is not StarSystem system) return;
        if (system.Planets.Count == 0) return;

        selectedPlanet = system.Planets[selectedPlanetInd];
        selectedPlanet.InitializeWorld();
        selectedPlanet.LoadModifications();
        planetSelectedCoords = (0, 0);
        planetOffset = (8f, 9f);
    }

    private void HandleInput(in float deltaTime) {
        if (Input.GetKeyUp(' ')) {
            if (selectedPlanet != null) {
                var (w, f, m, g) = selectedPlanet.Harvest(planetSelectedCoords.y * (int)selectedPlanet.Diameter + planetSelectedCoords.x);
                totalResources.w += w;
                totalResources.f += f;
                totalResources.m += m;
                totalResources.g += g;
            } else if (selectedBody != null) {
                UpdateSelectedPlanet();
            } else {
                UpdateSelectedSystem();
            }
        }

        if (Input.GetKeyUp((char)9)) uiEnabled = !uiEnabled;

        if (selectedPlanet != null) {
            if (Input.GetKeyDown('W')) planetOffset.y += PLANET_NAV_SPEED * deltaTime * 2;
            if (Input.GetKeyDown('A')) planetOffset.x += PLANET_NAV_SPEED * deltaTime;
            if (Input.GetKeyDown('S')) planetOffset.y -= PLANET_NAV_SPEED * deltaTime * 2;
            if (Input.GetKeyDown('D')) planetOffset.x -= PLANET_NAV_SPEED * deltaTime;
        } else {
            if (Input.GetKeyDown('W')) universeOffset.y -= UNIVERSE_NAV_SPEED * deltaTime;
            if (Input.GetKeyDown('A')) universeOffset.x -= UNIVERSE_NAV_SPEED * deltaTime;
            if (Input.GetKeyDown('S')) universeOffset.y += UNIVERSE_NAV_SPEED * deltaTime;
            if (Input.GetKeyDown('D')) universeOffset.x += UNIVERSE_NAV_SPEED * deltaTime;
        }

        if (Input.GetKeyUp((char)27)) {
            if (selectedPlanet != null) {
                selectedPlanet.SaveModifications();
                selectedPlanet = null;
            } else {
                selectedBody = null; 
            }
        }
        
        moveCooldown += deltaTime;
        if (moveCooldown <= 0.15f) return;
        
        if (selectedPlanet != null) {
            if (Input.GetKeyDown('I')) planetSelectedCoords.y--;
            if (Input.GetKeyDown('J')) planetSelectedCoords.x--;
            if (Input.GetKeyDown('K')) planetSelectedCoords.y++;
            if (Input.GetKeyDown('L')) planetSelectedCoords.x++;
            planetSelectedCoords.x = Math.Max(0, Math.Min((int)selectedPlanet.Diameter - 1, planetSelectedCoords.x));
            planetSelectedCoords.y = Math.Max(0, Math.Min((int)selectedPlanet.Diameter - 1, planetSelectedCoords.y));
        } else if (selectedBody != null && selectedBody is StarSystem system) {
            if (Input.GetKeyDown('J')) selectedPlanetInd--;
            if (Input.GetKeyDown('L')) selectedPlanetInd++;
            int planetCount = system.Planets.Count;
            selectedPlanetInd = 
                selectedPlanetInd < 0 ? planetCount - 1 : 
                planetCount > 0 ? selectedPlanetInd % planetCount : selectedPlanetInd;
        } else {
            if (Input.GetKeyDown('I')) universeSelectedCoords.y -= SECTORSIZE;
            if (Input.GetKeyDown('J')) universeSelectedCoords.x -= SECTORSIZE;
            if (Input.GetKeyDown('K')) universeSelectedCoords.y += SECTORSIZE;
            if (Input.GetKeyDown('L')) universeSelectedCoords.x += SECTORSIZE;
        }

        moveCooldown = 0f;
    }
}