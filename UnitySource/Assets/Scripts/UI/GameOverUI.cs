using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
      [SerializeField] private TextMeshProUGUI recipesDeliveredText;
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
        Debug.Log(KitchenGameManager.Instance.IsGameOver()); 
        if (KitchenGameManager.Instance.IsGameOver())
        {
            Show();
             recipesDeliveredText.text = DeliveryManager.Instance.GetCompleteRecipeSOList().Count.ToString();
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
