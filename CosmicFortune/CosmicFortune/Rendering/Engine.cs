using CosmicFortune.Common;

namespace CosmicFortune.Rendering;

internal class Canvas : Form {
    public Canvas() {
        DoubleBuffered = true;
    }
}

internal abstract class Engine {
    public Engine((int x, int y) windowSize, string title) {
        _title = title;

        var size = new Size(windowSize.x, windowSize.y);
        _canvas = new Canvas() {
            Size = size,
            StartPosition = FormStartPosition.CenterScreen,
            FormBorderStyle = FormBorderStyle.FixedSingle,
            ShowInTaskbar = true,
            Name = _title
        };
        _canvas.Paint += Paint;
        _mainThread = new Thread(MainLoop);
        _mainThread.SetApartmentState(ApartmentState.STA);

        WindowSize = size;
        WindowPosition = _canvas.Location;
        backgroundColor = Color.FromArgb(37, 37, 37);
    }

    private Color backgroundColor;
    public Color BackgroundColor {
        get => backgroundColor;
        set => backgroundColor = value;
    }

    public Size WindowSize {
        get => _canvas.Size;
        set => _canvas.Size = value;
    }

    public Point WindowPosition {
        get => _canvas.Location;
        set => _canvas.Location = value;
    }

    private readonly string _title;
    private readonly Canvas _canvas;
    private readonly Thread _mainThread;
    private Graphics? mainGraphics;

    public void Run() {
        _mainThread.Start();
        Application.Run(_canvas);
    }

    private void MainLoop() {
        Awake();

        while (true) {
            try {
                _canvas.BeginInvoke((MethodInvoker)delegate { _canvas.Refresh(); });
            }
            catch { } // ignore exception thrown for the first frame

            Thread.Sleep(1);
        }
    }

    private void Paint(object? sender, PaintEventArgs args) {
        mainGraphics = args.Graphics;

        mainGraphics.Clear(BackgroundColor);

        float delta = Time.DeltaTime;
        Update(mainGraphics, delta);

        // if it's lagging like crazy show the fps
        int fps = (int)(1000f / delta / 1000f);
        _canvas.Text = $"{_title}{(fps < 30 ? $" | FPS: {fps}" : string.Empty)}";
    }

    public abstract void Awake();
    public abstract void Update(in Graphics g, in float deltaTime);
}
