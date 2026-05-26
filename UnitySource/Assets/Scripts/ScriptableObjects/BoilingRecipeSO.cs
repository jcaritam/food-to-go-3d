using UnityEngine;

[CreateAssetMenu(fileName = "BoilingRecipeSO", menuName = "FoodToGo/BoilingRecipeSO")]
public class BoilingRecipeSO : ScriptableObject
{

    public KitchenObjectsSO input;
    public KitchenObjectsSO output;
    public float boilingTimerMax;
}
