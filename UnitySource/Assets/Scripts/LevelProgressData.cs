using UnityEngine;

public static class LevelProgressData
{
    public static bool IsLevelUnlocked(int levelIndex)
    {
        if (levelIndex == 1) return true;
        if (CloudProgressService.Instance != null)
            return CloudProgressService.Instance.IsLevelUnlocked(levelIndex);
        return PlayerPrefs.GetInt("level_unlocked_" + levelIndex, 0) == 1;
    }

    public static void UnlockLevel(int levelIndex)
    {
        if (CloudProgressService.Instance != null)
            CloudProgressService.Instance.UnlockLevel(levelIndex);
        else
        {
            PlayerPrefs.SetInt("level_unlocked_" + levelIndex, 1);
            PlayerPrefs.Save();
        }
    }

    public static CloudProgressService.LevelRecord GetLevelRecord(int levelId)
    {
        if (CloudProgressService.Instance != null)
            return CloudProgressService.Instance.GetLevelRecord(levelId);
        var key = "level_record_" + levelId;
        var json = PlayerPrefs.GetString(key, "");
        if (!string.IsNullOrEmpty(json))
            return JsonUtility.FromJson<CloudProgressService.LevelRecord>(json);
        return new CloudProgressService.LevelRecord();
    }

    public static void SaveLevelRecord(int levelId, int score, int stars)
    {
        if (CloudProgressService.Instance != null)
            CloudProgressService.Instance.SaveLevelRecord(levelId, score, stars);
        else
        {
            var key = "level_record_" + levelId;
            var existing = GetLevelRecord(levelId);
            existing.timesPlayed++;
            if (score > existing.bestScore)
            {
                existing.bestScore = score;
                existing.stars = stars;
            }
            PlayerPrefs.SetString(key, JsonUtility.ToJson(existing));
            PlayerPrefs.Save();
        }
    }
}
