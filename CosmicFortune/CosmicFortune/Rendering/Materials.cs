namespace CosmicFortune.Rendering;

internal static class Materials {
    public static Font InfoFont => _infoFont;
    private static readonly Font _infoFont = new Font("Arial", 12);

    public static Brush WhiteBrush => _whiteBrush;
    private static readonly Brush _whiteBrush = new SolidBrush(Color.White);

    public static Pen WhitePen => _whitePen;
    private static readonly Pen _whitePen = new Pen(_whiteBrush);
}
