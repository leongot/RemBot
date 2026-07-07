using UnityEngine;

public static class LevelSaveSystem
{
    private const string LEVEL_UNLOCKED_KEY = "LEVEL_UNLOCKED_";
    private const string LEVEL_COMPLETED_KEY = "LEVEL_COMPLETED_";
    private const string LEVEL_RECORD_KEY = "LEVEL_RECORD_";
    public const int MaxLevels = 10;

    public static bool IsLevelCompleted(int levelNumber)
    {
        return PlayerPrefs.GetInt(LEVEL_COMPLETED_KEY + levelNumber, 0) == 1;
    }

    public static void MarkLevelCompleted(int levelNumber)
    {
        PlayerPrefs.SetInt(LEVEL_COMPLETED_KEY + levelNumber, 1);

        if (levelNumber + 1 <= MaxLevels)
            PlayerPrefs.SetInt(LEVEL_UNLOCKED_KEY + (levelNumber + 1), 1);

        PlayerPrefs.Save();
        Debug.Log("[LevelSaveSystem] Livello completato: " + levelNumber);
    }

    public static bool IsLevelUnlocked(int levelNumber)
    {
        if (levelNumber <= 1)
            return true;

        if (!IsLevelCompleted(levelNumber - 1))
            return false;

        return PlayerPrefs.GetInt(LEVEL_UNLOCKED_KEY + levelNumber, 0) == 1;
    }

    public static void UnlockLevel(int levelNumber)
    {
        PlayerPrefs.SetInt(LEVEL_UNLOCKED_KEY + levelNumber, 1);
        PlayerPrefs.Save();

        Debug.Log("[LevelSaveSystem] Livello sbloccato: " + levelNumber);
    }

    public static int GetRecordScore(int levelNumber)
    {
        return PlayerPrefs.GetInt(LEVEL_RECORD_KEY + levelNumber, 0);
    }

    public static void SaveRecordScore(int levelNumber, int score)
    {
        int currentRecord = GetRecordScore(levelNumber);

        if (score > currentRecord)
        {
            PlayerPrefs.SetInt(LEVEL_RECORD_KEY + levelNumber, score);
            PlayerPrefs.Save();

            Debug.Log("[LevelSaveSystem] Nuovo record livello " + levelNumber + ": " + score);
        }
    }

    public static void ResetAllProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Debug.Log("[LevelSaveSystem] Progressi resettati.");
    }
}