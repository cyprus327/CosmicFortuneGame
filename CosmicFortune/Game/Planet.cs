using CosmicFortune.Common;

namespace CosmicFortune.Game;

internal sealed class Planet {
    public double Dist = 0d;
    public double Diameter = 0d;
    public double Foliage = 0d;
    public double Minerals = 0d;
    public double Water = 0d;
    public double Gases = 0d;
    public double Temp = 0d;
    public double Population = 0d;
    public bool HasRing = false;
    public List<double> Moons = new List<double>();
    public Color Col = Color.HotPink;
    public (int x, int y) Coords;
    
    private PlanetChunk[]? _world = null;
    private string saveDataFilename = string.Empty;
    private readonly HashSet<(int i, int v)> _modifiedVals = new HashSet<(int i, int v)>();

    public void InitializeWorld() {
        uint seed = (uint)((Coords.x & 0xFFFF) << 32 | (Coords.y & 0xFFFF));
        var rand = new LehmerRand(seed);
        saveDataFilename = $"SaveData{Path.DirectorySeparatorChar}pm_{seed}.txt";

        _world = new PlanetChunk[(int)Diameter * (int)Diameter];

        for (int i = 0; i < _world.Length; i++) {
            if (Foliage >= 0.7) {
                _world[i].Val = rand.Next(1, 7); // only foliage tiles
            } else if (Minerals >= 0.7) {
                _world[i].Val = rand.Next(6, 8);
            } else {
                _world[i].Val = rand.Next(1, 8);
            }
        }
    }

    public int ValAt(int index) {
        if (_world == null) return 0;

        return _world[index].Val;
    }

    public void PlusPlus(int index) {
        if (_world == null) return;

        _world[index].Val++;
        _modifiedVals.Add((index, _world[index].Val));
    }

    public void ModEq(int index, int value) {
        if (_world == null) return;

        _world[index].Val %= value;
        _modifiedVals.Add((index, _world[index].Val));
    }

    public void SaveModifications() {
        string serializedData = string.Join('|', _modifiedVals.Select(x => x));
        File.WriteAllText(saveDataFilename, serializedData);
    }

    public void LoadModifications() {
        if (!File.Exists(saveDataFilename)) return;
        if (_world == null) return;

        string[] data = File.ReadAllText(saveDataFilename).Split('|');
        foreach (string item in data) {
            string[] values = item.Trim('(', ')').Split(", ");
            _world[int.Parse(values[0])].Val = int.Parse(values[1]);
        }
        _modifiedVals.Clear();
    }
}
internal struct PlanetChunk {
    public int Val;
}