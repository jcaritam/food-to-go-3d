using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
      [SerializeField] private TextMeshProUGUI recipesDeliveredText;
      [SerializeField] private TextMeshProUGUI pointsText;
      [SerializeField] private Button menuButton;

  private void Awake()
  {
    menuButton.onClick.AddListener(() =>
    {
        Loader.Load(Loader.Scene.MenuScene);
    });
  }

  private void Start()
    {
        KitchenGameManager.Instance.OnStateChanged += KitchenGameManager_OnStateChanged;
        Hide();
    }

    private void KitchenGameManager_OnStateChanged(object sender, EventArgs e)
    {
        if (KitchenGameManager.Instance.IsGameOver())
        {
            Show();
             recipesDeliveredText.text = DeliveryManager.Instance.GetCompleteRecipeSOList().Count.ToString();
             pointsText.text = DeliveryManager.Instance.GetCompleteRecipeSOList().Sum(v => v.points).ToString();
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
