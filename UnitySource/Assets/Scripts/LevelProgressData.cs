using UnityEngine;

public static class LevelProgressData
{
    public static bool IsLevelUnlocked(int levelIndex)
    {
        if (levelIndex == 1) return true;
        if (CloudProgressService.Instance != null && CloudProgressService.Instance.IsReady)
            return CloudProgressService.Instance.IsLevelUnlocked(levelIndex);
        var csv = PlayerPrefs.GetString("unlocked_levels", "1");
        foreach (var part in csv.Split(','))
            if (int.TryParse(part.Trim(), out int n) && n == levelIndex)
                return true;
        return false;
    }

    public static void UnlockLevel(int levelIndex)
    {
        if (CloudProgressService.Instance != null)
        {
            CloudProgressService.Instance.UnlockLevel(levelIndex);
            return;
        }
        var csv = PlayerPrefs.GetString("unlocked_levels", "1");
        var parts = new System.Collections.Generic.HashSet<string>(csv.Split(','));
        parts.Add(levelIndex.ToString());
        PlayerPrefs.SetString("unlocked_levels", string.Join(",", parts));
        PlayerPrefs.Save();
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
