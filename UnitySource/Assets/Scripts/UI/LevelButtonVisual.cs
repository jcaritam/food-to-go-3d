using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class LevelButtonVisual : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelNumberText;
    [SerializeField] private GameObject lockIcon;
    [SerializeField] private Image previewImage;
    [SerializeField] private Image[] starImages;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void SetLocked(bool locked)
    {
        button.interactable = !locked;

        if (lockIcon != null)
            lockIcon.SetActive(locked);

        if (levelNumberText != null)
            levelNumberText.gameObject.SetActive(!locked);

        if (previewImage != null)
        {
            Color imgColor = previewImage.color;
            imgColor.a = locked ? 0.35f : 1f;
            previewImage.color = imgColor;
        }

        if (locked)
            SetStars(0);
    }

    public void SetStars(int stars)
    {
        if (starImages == null) return;
        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] == null) continue;
            Color c = starImages[i].color;
            c.a = i < stars ? 1f : 0.2f;
            starImages[i].color = c;
        }
    }
}
