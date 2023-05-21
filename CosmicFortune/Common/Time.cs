namespace CosmicFortune.Common;

internal static class Time {
    static Time() {
        startTime = DateTime.UtcNow;
        currentTime = startTime;
    }
    
    private static readonly DateTime startTime;
    private static DateTime currentTime;
    private static float timeScale = 1.0f;

    public static float ElapsedTime {
        get => (float)(DateTime.UtcNow - startTime).TotalSeconds * timeScale;
    }

    public static float DeltaTime {
        get {
            float output = (float)(DateTime.UtcNow - currentTime).TotalSeconds * timeScale;
            currentTime = DateTime.UtcNow;
            return output;
        }
    }

    public static float TimeScale {
        get => timeScale;
        set => timeScale = value;
    }
}