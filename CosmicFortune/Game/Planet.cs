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
    private readonly HashSet<(int i, PlanetChunk c)> _modifiedVals = new HashSet<(int i, PlanetChunk c)>();

    public void InitializeWorld() {
        uint seed = (uint)((Coords.x & 0xFFFF) << 32 | (Coords.y & 0xFFFF));
        var rand = new LehmerRand(seed);
        saveDataFilename = $"SaveData{Path.DirectorySeparatorChar}p_{seed}.txt";

        _world = new PlanetChunk[(int)Diameter * (int)Diameter];

        for (int i = 0; i < _world.Length; i++) {
            if (Foliage >= 0.7) {
                _world[i].TileInd = rand.Next(1, 7);
            } else if (Minerals >= 0.7) {
                _world[i].TileInd = rand.Next(6, 8);
            } else {
                _world[i].TileInd = rand.Next(1, 8);
            }

            _world[i].Water = rand.Next(0.1, 0.6) * Water;
            _world[i].Foliage = rand.Next(0.1, 0.6) * Foliage;
            _world[i].Minerals = rand.Next(0.1, 0.6) * Minerals;
            _world[i].Gases = rand.Next(0.1, 0.6) * Gases;
            _world[i].Total = _world[i].Water + 
                              _world[i].Foliage + 
                              _world[i].Minerals +
                              _world[i].Gases;
        }
    }

    public PlanetChunk this[int i] {
        get => _world != null ? _world[i] : new PlanetChunk();
    }

    public int World(int index) {
        if (_world == null) return 0;

        return _world[index].TileInd;
    }

    public (double w, double f, double m, double g) Harvest(int index) {
        if (_world == null) return (0d, 0d, 0d, 0d);

        double w = _world[index].Water * 0.1d;
        double f = _world[index].Foliage * 0.1d;
        double m = _world[index].Minerals * 0.1d;
        double g = _world[index].Gases * 0.1d;

        _world[index].Water -= w;
        _world[index].Foliage -= f;
        _world[index].Minerals -= m;
        _world[index].Gases -= g;

        if (_world[index].TileInd > 0 && _world[index].TileInd < 7) {
            if (_world[index].Water < 0.25 && _world[index].Foliage < 0.25) {
                _world[index].TileInd = 6;
            }
        } else if (_world[index].TileInd == 6) {
            if (_world[index].Minerals < 0.25 && _world[index].Gases < 0.25) {
                _world[index].TileInd = 7;
            }
        }

        _modifiedVals.Add((index, _world[index]));

        return (w, f, m, g);
    }

    public void SaveModifications() {
        string serializedData = string.Join('|', _modifiedVals.Select(x => $"({x.i},{x.c})"));
        File.WriteAllText(saveDataFilename, serializedData);
    }

    public void LoadModifications() {
        if (!File.Exists(saveDataFilename)) return;
        if (_world == null) return;

        _modifiedVals.Clear();
        string[] data = File.ReadAllText(saveDataFilename).Split('|');
        foreach (string item in data) {
            if (item == string.Empty) continue;
            string[] values = item.Trim('(', ')').Split(',');
            int i = int.Parse(values[0]);
            var modifiedChunk = new PlanetChunk {
                TileInd = int.Parse(values[1]),
                Water = double.Parse(values[2]),
                Foliage = double.Parse(values[3]),
                Minerals = double.Parse(values[4]),
                Gases = double.Parse(values[5]),
                Total = double.Parse(values[6])
            };
            _world[i] = modifiedChunk;
            _modifiedVals.Add((i, modifiedChunk));
        }
    }
}
internal struct PlanetChunk {
    public int TileInd;
    public double Water;
    public double Foliage;
    public double Minerals;
    public double Gases;
    public double Total;

    public override string ToString() {
        return $"{TileInd},{Water},{Foliage},{Minerals},{Gases},{Total}";
    }
}