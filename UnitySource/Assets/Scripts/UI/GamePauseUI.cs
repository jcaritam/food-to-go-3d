using System;
using UnityEngine;
using UnityEngine.UI;

public class GamePauseUI : MonoBehaviour
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;

    private void Awake()
    {
        resumeButton.onClick.AddListener(() => KitchenGameManager.Instance.TogglePause());
        mainMenuButton.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            Loader.Load(Loader.Scene.LevelSelectScene);
        });
    }

    private void Start()
    {
        KitchenGameManager.Instance.OnGamePaused += KitchenGameManager_OnGamePaused;
        KitchenGameManager.Instance.OnGameUnpaused += KitchenGameManager_OnGameUnpaused;
        Hide();
    }

    private void KitchenGameManager_OnGamePaused(object sender, EventArgs e) => Show();
    private void KitchenGameManager_OnGameUnpaused(object sender, EventArgs e) => Hide();

    private void Show() => gameObject.SetActive(true);
    private void Hide() => gameObject.SetActive(false);
}
