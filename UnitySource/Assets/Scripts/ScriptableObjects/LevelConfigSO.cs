using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "FoodToGo/LevelConfig")]
public class LevelConfigSO : ScriptableObject
{
    public int levelId;
    public int starThreshold1;
    public int starThreshold2;
    public int starThreshold3;
    public int requiredStarsToUnlock;

    public int CalculateStars(int score)
    {
        if (score >= starThreshold3) return 3;
        if (score >= starThreshold2) return 2;
        if (score >= starThreshold1) return 1;
        return 0;
    }
}
