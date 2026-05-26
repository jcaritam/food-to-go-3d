using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI gameOverTitleText;
    [SerializeField] private TextMeshProUGUI recipesDeliveredText;
    [SerializeField] private TextMeshProUGUI pointsText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI errorsText;
    [SerializeField] private TextMeshProUGUI unlockText;
    [SerializeField] private Image[] starImages;
    [SerializeField] private Sprite starGoldSprite;
    [SerializeField] private Sprite starEmptySprite;
    [SerializeField] private Button menuButton;
    [SerializeField] private Button retryButton;

    private void Awake()
    {
        menuButton.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            Loader.Load(Loader.Scene.LevelSelectScene);
        });
        if (retryButton != null)
            retryButton.onClick.AddListener(() =>
            {
                Time.timeScale = 1f;
                int id = KitchenGameManager.Instance != null ? KitchenGameManager.Instance.levelId : 1;
                Loader.Scene scene = id == 2 ? Loader.Scene.HuariqueScene : Loader.Scene.GameScene;
                Loader.Load(scene);
            });
    }

    private void Start()
    {
        KitchenGameManager.Instance.OnStateChanged += KitchenGameManager_OnStateChanged;
        Hide();
    }

    private void KitchenGameManager_OnStateChanged(object sender, EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGameOver())
        {
            Hide();
            return;
        }

        var manager = KitchenGameManager.Instance;
        int score = Mathf.Max(0, DeliveryManager.Instance.GetCompleteRecipeSOList().Sum(r => r.points)
            - DeliveryManager.Instance.GetTotalPenaltyPoints());
        int stars = manager.levelConfig != null ? manager.levelConfig.CalculateStars(score) : 0;

        LevelProgressData.SaveLevelRecord(manager.levelId, score, stars);

        int required = manager.levelConfig != null ? manager.levelConfig.requiredStarsToUnlock : 1;
        bool levelComplete = stars >= required;

        if (levelComplete)
        {
            int nextLevel = manager.levelId + 1;
            LevelProgressData.UnlockLevel(nextLevel);
        }

        if (gameOverTitleText != null)
            gameOverTitleText.text = levelComplete ? "¡Nivel Completo!" : "¡Nivel Incompleto!";

        if (unlockText != null)
            unlockText.gameObject.SetActive(levelComplete);

        recipesDeliveredText.text = DeliveryManager.Instance.GetCompleteRecipeSOList().Count.ToString();
        pointsText.text = score.ToString();

        if (timeText != null)
        {
            float elapsed = Mathf.Max(0f, KitchenGameManager.Instance.GetGamePlayingTimerMax()
                - KitchenGameManager.Instance.GetGamePlayingTimer());
            int minutes = Mathf.FloorToInt(elapsed / 60f);
            int seconds = Mathf.FloorToInt(elapsed % 60f);
            timeText.text = $"{minutes}:{seconds:00}";
        }

        if (errorsText != null)
            errorsText.text = DeliveryManager.Instance.GetFailedRecipeCount().ToString();

        UpdateStarImages(stars);

        Show();
    }

    private void UpdateStarImages(int stars)
    {
        if (starImages == null) return;
        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] == null) continue;
            bool earned = i < stars;
            if (starGoldSprite != null && starEmptySprite != null)
                starImages[i].sprite = earned ? starGoldSprite : starEmptySprite;
            else
            {
                Color c = starImages[i].color;
                c.a = earned ? 1f : 0.25f;
                starImages[i].color = c;
            }
        }
    }

    private void Show() => gameObject.SetActive(true);
    private void Hide() => gameObject.SetActive(false);
}
