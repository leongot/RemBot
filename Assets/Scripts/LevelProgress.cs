using UnityEngine;

public static class LevelProgress
{
    public static bool IsLevelCompleted(int levelNumber) => LevelSaveSystem.IsLevelCompleted(levelNumber);

    public static void SetLevelCompleted(int levelNumber) => LevelSaveSystem.MarkLevelCompleted(levelNumber);

    public static bool IsLevelUnlocked(int levelNumber) => LevelSaveSystem.IsLevelUnlocked(levelNumber);

    public static void ResetProgress() => LevelSaveSystem.ResetAllProgress();
}
