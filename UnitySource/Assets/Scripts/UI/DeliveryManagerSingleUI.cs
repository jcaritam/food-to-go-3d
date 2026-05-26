using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeliveryManagerSingleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI recipeNameText;
    [SerializeField] private Transform iconContainer;
    [SerializeField] private Transform iconTemplate;
    [SerializeField] private Image finalDishImage;
    [SerializeField] private Image colorBar;

    private static readonly Color ColorGreen  = new Color(0.10f, 0.42f, 0.10f);
    private static readonly Color ColorYellow = new Color(1f,    0.80f, 0f);
    private static readonly Color ColorRed    = new Color(0.85f, 0.15f, 0.10f);

    private DeliveryManager.WaitingRecipe waitingRecipe;

    private void Awake()
    {
        iconTemplate.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (waitingRecipe == null || colorBar == null) return;

        float normalized = waitingRecipe.timeRemaining / waitingRecipe.timeMax;
        colorBar.fillAmount = normalized;

        if (normalized > 0.5f)
            colorBar.color = ColorGreen;
        else if (normalized > 0.25f)
            colorBar.color = ColorYellow;
        else
            colorBar.color = ColorRed;
    }

    public void SetRecipe(DeliveryManager.WaitingRecipe waitingRecipe)
    {
        this.waitingRecipe = waitingRecipe;
        RecipeSO recipeSO = waitingRecipe.recipeSO;

        recipeNameText.text = recipeSO.recipeName;

        if (finalDishImage != null && recipeSO.finalDishSprite != null)
            finalDishImage.sprite = recipeSO.finalDishSprite;

        foreach (Transform child in iconContainer)
        {
            if (child == iconTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach (KitchenObjectsSO kitchenObjectsSO in recipeSO.kitchenObjectSOList)
        {
            Transform iconTransform = Instantiate(iconTemplate, iconContainer);
            iconTransform.gameObject.SetActive(true);
            iconTransform.Find("IconImage").GetComponent<Image>().sprite = kitchenObjectsSO.sprite;
        }
    }
}
