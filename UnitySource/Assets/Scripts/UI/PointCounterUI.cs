using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PointCounterUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI pointerText;

    private Color _originalColor;

    private void Start()
    {
        _originalColor = pointerText.color;
        pointerText.text = 0.ToString();
        DeliveryManager.Instance.OnRecipeComplete += DeliveryManager_OnRecipeComplete;
        DeliveryManager.Instance.OnWrongDeliveryPenalty += DeliveryManager_OnWrongDeliveryPenalty;
    }

    private void DeliveryManager_OnRecipeComplete(object sender, EventArgs e)
    {
        RefreshScore();
    }

    private void DeliveryManager_OnWrongDeliveryPenalty(object sender, EventArgs e)
    {
        StopAllCoroutines();
        StartCoroutine(FlashRed());
    }

    private void RefreshScore()
    {
        int score = Mathf.Max(0, DeliveryManager.Instance.GetCompleteRecipeSOList().Sum(r => r.points)
            - DeliveryManager.Instance.GetTotalPenaltyPoints());
        pointerText.text = score.ToString();
    }

    private IEnumerator FlashRed()
    {
        pointerText.color = Color.red;
        float elapsed = 0f;
        while (elapsed < 0.4f)
        {
            elapsed += Time.deltaTime;
            pointerText.color = Color.Lerp(Color.red, _originalColor, elapsed / 0.4f);
            yield return null;
        }
        pointerText.color = _originalColor;
    }
}
