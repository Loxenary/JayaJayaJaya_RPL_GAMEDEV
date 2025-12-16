namespace Ambience
{
    /// <summary>
    /// Enum defining different types of music events that can trigger music changes
    /// </summary>
    public enum MusicEventType
    {
        None = 0,

        // Scene-based events
        SceneEnter = 10,
        SceneExit = 11,

        // Collectibels
        FirstCollectible = 20,
        SecondCollectible = 21,
        ThirdCollectible = 22,

        // Enemy events
        Enemy_Seen = 30,

        // Story events
        StoryBeat = 40,
        Cutscene = 41,
        DialogueStart = 42,
        DialogueEnd = 43,

        // Environment events
        SafeZone = 50,
        DangerZone = 51,
        PuzzleArea = 52,
        BossArea = 53,

        // Game state events
        GameOver = 60,
        Victory = 61,
        Pause = 62,
        MainMenu = 63,

        // Custom events
        Custom1 = 100,
        Custom2 = 101,
        Custom3 = 102,
        Custom4 = 103,
        Custom5 = 104
    }

}
