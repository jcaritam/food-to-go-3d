using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;

public class CloudProgressService : MonoBehaviour
{
    public static CloudProgressService Instance { get; private set; }

    public bool IsReady { get; private set; } = false;

    private const string KeyUnlockedLevels = "unlocked_levels";
    private const string KeySessionsPlayed = "sessions_played";
    private const string KeyLevelRecordsPrefix = "level_record_";

    private HashSet<int> unlockedLevels = new HashSet<int> { 1 };
    private int sessionsPlayed = 0;
    private Dictionary<int, LevelRecord> levelRecords = new Dictionary<int, LevelRecord>();

    [Serializable]
    public class LevelRecord
    {
        public int bestScore;
        public int stars;
        public int timesPlayed;
    }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private async void Start()
    {
        await InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            await LoadProgressAsync();
            sessionsPlayed++;
            await SaveBaseProgressAsync();
            IsReady = true;
            Debug.Log($"[CloudProgress] Listo. Player: {AuthenticationService.Instance.PlayerId} | Sesiones: {sessionsPlayed}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[CloudProgress] Fallo cloud, usando local. {e.Message}");
            LoadFromLocal();
            IsReady = true;
        }
    }

    private async Task LoadProgressAsync()
    {
        var keys = new HashSet<string> { KeyUnlockedLevels, KeySessionsPlayed };
        var result = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);

        if (result.TryGetValue(KeyUnlockedLevels, out var levelsItem))
            unlockedLevels = ParseLevels(levelsItem.Value.GetAs<string>());

        if (result.TryGetValue(KeySessionsPlayed, out var sessionsItem))
            sessionsPlayed = sessionsItem.Value.GetAs<int>();

        SyncBaseToLocal();

        var recordKeys = new HashSet<string>();
        foreach (var levelId in unlockedLevels)
            recordKeys.Add(KeyLevelRecordsPrefix + levelId);

        if (recordKeys.Count > 0)
        {
            var recordResult = await CloudSaveService.Instance.Data.Player.LoadAsync(recordKeys);
            foreach (var kvp in recordResult)
            {
                var json = kvp.Value.Value.GetAs<string>();
                if (!string.IsNullOrEmpty(json))
                {
                    var keyStr = kvp.Key.Replace(KeyLevelRecordsPrefix, "");
                    if (int.TryParse(keyStr, out int id))
                    {
                        var record = JsonUtility.FromJson<LevelRecord>(json);
                        levelRecords[id] = record;
                        PlayerPrefs.SetString(kvp.Key, json);
                    }
                }
            }
            PlayerPrefs.Save();
        }
    }

    private async Task SaveBaseProgressAsync()
    {
        var data = new Dictionary<string, object>
        {
            { KeyUnlockedLevels, SerializeLevels(unlockedLevels) },
            { KeySessionsPlayed, sessionsPlayed }
        };
        await CloudSaveService.Instance.Data.Player.SaveAsync(data);
        SyncBaseToLocal();
    }

    public bool IsLevelUnlocked(int levelIndex)
    {
        if (levelIndex == 1) return true;
        return unlockedLevels.Contains(levelIndex);
    }

    public async void UnlockLevel(int levelIndex)
    {
        if (unlockedLevels.Contains(levelIndex)) return;
        unlockedLevels.Add(levelIndex);

        try { await SaveBaseProgressAsync(); }
        catch (Exception e)
        {
            Debug.LogWarning($"[CloudProgress] No se pudo guardar en nube: {e.Message}");
            SyncBaseToLocal();
        }
    }

    public LevelRecord GetLevelRecord(int levelId)
    {
        if (levelRecords.TryGetValue(levelId, out var record))
            return record;
        return new LevelRecord();
    }

    public async void SaveLevelRecord(int levelId, int score, int stars)
    {
        if (!levelRecords.TryGetValue(levelId, out var record))
            record = new LevelRecord();

        record.timesPlayed++;
        if (score > record.bestScore)
        {
            record.bestScore = score;
            record.stars = stars;
        }
        levelRecords[levelId] = record;

        var key = KeyLevelRecordsPrefix + levelId;
        var json = JsonUtility.ToJson(record);
        PlayerPrefs.SetString(key, json);
        PlayerPrefs.Save();

        try
        {
            await CloudSaveService.Instance.Data.Player.SaveAsync(new Dictionary<string, object>
            {
                { key, json }
            });
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[CloudProgress] No se pudo guardar record nivel {levelId}: {e.Message}");
        }
    }

    public async Task LoadLevelRecordAsync(int levelId)
    {
        var key = KeyLevelRecordsPrefix + levelId;
        try
        {
            var result = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { key });
            if (result.TryGetValue(key, out var item))
            {
                var record = JsonUtility.FromJson<LevelRecord>(item.Value.GetAs<string>());
                levelRecords[levelId] = record;
                PlayerPrefs.SetString(key, JsonUtility.ToJson(record));
                PlayerPrefs.Save();
                return;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[CloudProgress] No se pudo cargar record nivel {levelId}: {e.Message}");
        }

        var localJson = PlayerPrefs.GetString(key, "");
        if (!string.IsNullOrEmpty(localJson))
            levelRecords[levelId] = JsonUtility.FromJson<LevelRecord>(localJson);
    }

    private void SyncBaseToLocal()
    {
        PlayerPrefs.SetString(KeyUnlockedLevels, SerializeLevels(unlockedLevels));
        PlayerPrefs.SetInt(KeySessionsPlayed, sessionsPlayed);
        PlayerPrefs.Save();
    }

    private void LoadFromLocal()
    {
        unlockedLevels = ParseLevels(PlayerPrefs.GetString(KeyUnlockedLevels, "1"));
        sessionsPlayed = PlayerPrefs.GetInt(KeySessionsPlayed, 0);

        foreach (var levelId in unlockedLevels)
        {
            var key = KeyLevelRecordsPrefix + levelId;
            var json = PlayerPrefs.GetString(key, "");
            if (!string.IsNullOrEmpty(json))
                levelRecords[levelId] = JsonUtility.FromJson<LevelRecord>(json);
        }
    }

    private static string SerializeLevels(HashSet<int> levels) => string.Join(",", levels);

    private static HashSet<int> ParseLevels(string csv)
    {
        var set = new HashSet<int> { 1 };
        if (string.IsNullOrEmpty(csv)) return set;
        foreach (var part in csv.Split(','))
            if (int.TryParse(part.Trim(), out int n))
                set.Add(n);
        return set;
    }
}
