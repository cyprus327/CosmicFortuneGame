using CosmicFortune.Common;
using CosmicFortune.Rendering;
using CosmicFortune.Game.Objects;

namespace CosmicFortune.Game;

internal sealed class Galaxy : Engine {
    public Galaxy((int x, int y) windowSize, string windowTitle) : base(windowSize, windowTitle) { }

    private const int SECTORSIZE = 16;

    private (int x, int y) galaxySelectedCoords = (0, 0);
    private (int x, int y) planetSelectedCoords = (0, 0);

    private GalacticBody? selectedBody = null; 
    private Planet? selectedPlanet = null;
    private int selectedPlanetInd = 0;

    private const float GALAXY_NAV_SPEED = 30f;
    private const float PLANET_NAV_SPEED = 12f;
    private (float x, float y) galaxyOffset = (0f, 0f);
    private (float x, float y) planetOffset = (10, 5);
    private (int x, int y) OffsetSelected => (galaxySelectedCoords.x + (int)galaxyOffset.x * SECTORSIZE, galaxySelectedCoords.y + (int)galaxyOffset.y * SECTORSIZE);

    private float moveCooldown = 0f;

    private (double w, double f, double m, double g) totalResources;

    public static string ResourcesFile { get; } = $"SaveData{Path.DirectorySeparatorChar}resources.txt";

    public override void Awake() {
        BackgroundColor = Color.Black;

        static (double, double, double, double) parseResources() {
            string[] data = File.ReadAllText(ResourcesFile).Trim('(', ')').Split(", ");
            return (double.Parse(data[0]), double.Parse(data[1]), double.Parse(data[2]), double.Parse(data[3]));
        }

        totalResources = File.Exists(ResourcesFile) ? parseResources() : (0d, 0d, 0d, 0d);
    }

    public override void Update(in Graphics g, in float deltaTime) {
        HandleInput(deltaTime);

        galaxySelectedCoords.x = Math.Clamp(galaxySelectedCoords.x, 0, WindowSize.Width - SECTORSIZE * 3);
        galaxySelectedCoords.y = Math.Clamp(galaxySelectedCoords.y, 0, WindowSize.Height - SECTORSIZE * 3);

        if (selectedPlanet != null) {
            PlanetRenderer.DrawPlanet(g, selectedPlanet, planetOffset, planetSelectedCoords);
        } else if (selectedBody != null) {
            CloseRenderer.DrawBody(g, selectedBody, WindowSize, selectedPlanetInd);
        } else {
            DrawGalaxy(g);
        }

        GalacticRenderer.DrawStats(g, totalResources, WindowSize);

        if ((int)Time.ElapsedTime % 10 == 0) {
            File.WriteAllText(ResourcesFile, totalResources.ToString());
        }
    }

    // TODO move this to GalacticRenderer
    private void DrawGalaxy(in Graphics g) {
        if (selectedPlanet != null) return;

        int xSectors = WindowSize.Width / SECTORSIZE;
        int ySectors = WindowSize.Height / SECTORSIZE;

        (uint x, uint y) currentSector;
        for (currentSector.y = 0; currentSector.y < ySectors; currentSector.y++) {
            for (currentSector.x = 0; currentSector.x < xSectors; currentSector.x++) {
                var body = GalacticBody.At(
                    currentSector.x + (uint)galaxyOffset.x,
                    currentSector.y + (uint)galaxyOffset.y);

                if (body == null) continue;

                GalacticRenderer.DrawBody(g, body, currentSector, SECTORSIZE);
                
                if (!(galaxySelectedCoords.x / SECTORSIZE == currentSector.x && galaxySelectedCoords.y / SECTORSIZE == currentSector.y)) continue;

                g.DrawEllipse(Pens.Yellow,
                    x: currentSector.x * SECTORSIZE + (SECTORSIZE / 2) - ((SECTORSIZE - 4) / 2),
                    y: currentSector.y * SECTORSIZE + (SECTORSIZE / 2) - ((SECTORSIZE - 4) / 2),
                    width: SECTORSIZE - 4,
                    height: SECTORSIZE - 4);
            }
        }

        g.DrawRectangle(Pens.Red, galaxySelectedCoords.x, galaxySelectedCoords.y, SECTORSIZE, SECTORSIZE);
    }

    private void UpdateSelectedBody() {
        selectedPlanetInd = 0;

        uint x = (uint)(OffsetSelected.x / SECTORSIZE), y = (uint)(OffsetSelected.y / SECTORSIZE);
        var body = GalacticBody.At(x, y);

        if (body == null) {
            selectedBody = null;
            return;
        }

        if (body is StarSystem system) {
            if (!system.StarExists) {
                selectedBody = null;
                return;
            }

            selectedBody = new StarSystem(x, y, true);
            return;
        }

        if (body is Nebula nebula) {
            if (!nebula.NebulaExists) {
                selectedBody = null;
                return;
            }

            selectedBody = new Nebula(x, y, true);
            return;
        }

        if (body is BlackHole) {
            totalResources = (0d, 0d, 0d, 0d);

            selectedBody = new BlackHole(x, y);
            return;
        }
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
                UpdateSelectedBody();
            }
        }

        if (selectedPlanet != null) {
            if (Input.GetKeyDown('W')) planetOffset.y += PLANET_NAV_SPEED * deltaTime * 2;
            if (Input.GetKeyDown('A')) planetOffset.x += PLANET_NAV_SPEED * deltaTime;
            if (Input.GetKeyDown('S')) planetOffset.y -= PLANET_NAV_SPEED * deltaTime * 2;
            if (Input.GetKeyDown('D')) planetOffset.x -= PLANET_NAV_SPEED * deltaTime;
        } else {
            if (Input.GetKeyDown('W')) galaxyOffset.y -= GALAXY_NAV_SPEED * deltaTime;
            if (Input.GetKeyDown('A')) galaxyOffset.x -= GALAXY_NAV_SPEED * deltaTime;
            if (Input.GetKeyDown('S')) galaxyOffset.y += GALAXY_NAV_SPEED * deltaTime;
            if (Input.GetKeyDown('D')) galaxyOffset.x += GALAXY_NAV_SPEED * deltaTime;
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
            if (Input.GetKeyDown('I')) galaxySelectedCoords.y -= SECTORSIZE;
            if (Input.GetKeyDown('J')) galaxySelectedCoords.x -= SECTORSIZE;
            if (Input.GetKeyDown('K')) galaxySelectedCoords.y += SECTORSIZE;
            if (Input.GetKeyDown('L')) galaxySelectedCoords.x += SECTORSIZE;
        }

        moveCooldown = 0f;
    }
}