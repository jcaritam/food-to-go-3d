using UnityEngine;

[CreateAssetMenu(menuName = "FoodToGo/VFXPrefabRefsSO")]
public class VFXPrefabRefsSO : ScriptableObject
{
    public ParticleSystem deliverySuccess; // confeti dorado
    public ParticleSystem cut;             // chispas de comida
    public ParticleSystem burned;          // humo gris (stove)
}
