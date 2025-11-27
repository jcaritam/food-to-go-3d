using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class PointCounterUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI pointerText;

    private void Start()
    {
        pointerText.text = 0.ToString();
        DeliveryManager.Instance.OnRecipeComplete += DeliveryManager_OnRecipeComplete;
    }

    private void DeliveryManager_OnRecipeComplete(object sender, EventArgs e)
    {
        int totalScore = DeliveryManager.Instance.GetCompleteRecipeSOList().Sum(
            recipeSo => recipeSo.points
        );
        pointerText.text = totalScore.ToString();
    }

}
