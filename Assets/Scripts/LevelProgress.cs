using UnityEngine;

public static class LevelProgress
{
    private const string LevelCompletedKey = "LevelCompleted_";

    public static bool IsLevelCompleted(int levelNumber)
    {
        return PlayerPrefs.GetInt(LevelCompletedKey + levelNumber, 0) == 1;
    }

    public static void SetLevelCompleted(int levelNumber)
    {
        PlayerPrefs.SetInt(LevelCompletedKey + levelNumber, 1);
        PlayerPrefs.Save();
    }

    public static bool IsLevelUnlocked(int levelNumber)
    {
        if (levelNumber <= 1) return true;
        return IsLevelCompleted(levelNumber - 1);
    }

    public static void ResetProgress()
    {
        for (int i = 1; i <= 10; i++)
        {
            PlayerPrefs.DeleteKey(LevelCompletedKey + i);
        }
        PlayerPrefs.Save();
    }
}
