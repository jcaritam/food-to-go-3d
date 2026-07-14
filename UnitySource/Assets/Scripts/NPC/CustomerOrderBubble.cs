using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Burbuja de pedido sobre la cabeza del comensal: muestra el plato deseado
/// (RecipeSO.finalDishSprite) y una barra de paciencia con el mismo esquema de
/// color que DeliveryManagerSingleUI. Se coloca en un Canvas en World Space,
/// hijo del NPC; usar LookAtCamera (CameraForward) en el Canvas para que mire
/// siempre a camara.
/// </summary>
public class CustomerOrderBubble : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private Image dishImage;
    [SerializeField] private Image patienceBar;
    [SerializeField] private GameObject happyIcon;
    [SerializeField] private GameObject angryIcon;
    [SerializeField] private float reactionDuration = 1.5f;

    private static readonly Color ColorGreen = new Color(0.10f, 0.42f, 0.10f);
    private static readonly Color ColorYellow = new Color(1f, 0.80f, 0f);
    private static readonly Color ColorRed = new Color(0.85f, 0.15f, 0.10f);

    public void Hide()
    {
        if (root != null) root.SetActive(false);
    }

    public void Show(RecipeSO recipeSO)
    {
        if (root != null) root.SetActive(true);
        if (happyIcon != null) happyIcon.SetActive(false);
        if (angryIcon != null) angryIcon.SetActive(false);
        if (dishImage != null)
        {
            dishImage.gameObject.SetActive(true);
            dishImage.sprite = recipeSO != null ? recipeSO.finalDishSprite : null;
        }
        if (patienceBar != null) patienceBar.gameObject.SetActive(true);
    }

    public void UpdatePatience(float normalized)
    {
        if (patienceBar == null) return;

        patienceBar.fillAmount = Mathf.Clamp01(normalized);
        if (normalized > 0.5f) patienceBar.color = ColorGreen;
        else if (normalized > 0.25f) patienceBar.color = ColorYellow;
        else patienceBar.color = ColorRed;
    }

    public void ShowReaction(bool happy)
    {
        if (dishImage != null) dishImage.gameObject.SetActive(false);
        if (patienceBar != null) patienceBar.gameObject.SetActive(false);
        if (happyIcon != null) happyIcon.SetActive(happy);
        if (angryIcon != null) angryIcon.SetActive(!happy);

        if (reactionDuration > 0f) Invoke(nameof(Hide), reactionDuration);
    }
}
