using UnityEngine;

public class KitchenObject : MonoBehaviour
{
    [SerializeField] private KitchenObjectsSO kitchenObjectsSO;

    public KitchenObjectsSO GetKitChenObjectSO()
    {
        return kitchenObjectsSO;
    }
}
