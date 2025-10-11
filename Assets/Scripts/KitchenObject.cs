using UnityEngine;

public class KitchenObject : MonoBehaviour
{
    [SerializeField] private KitchenObjectsSO kitchenObjectsSO;

    private IKitchenObjectParent kitchenObjectParent;

    public KitchenObjectsSO GetKitChenObjectSO()
    {
        return kitchenObjectsSO;
    }

    public void SetKitchenObjectParent(IKitchenObjectParent kitchenObjectParent)
    {
        if (this.kitchenObjectParent != null)
    {
            this.kitchenObjectParent.ClearKitchenObject();
    }
        this.kitchenObjectParent = kitchenObjectParent;

        if (kitchenObjectParent.HasKitchenObject())
        {
            Debug.Log("IKitchenObjectParent already has a kitchenObject");
        }

        kitchenObjectParent.SetKitchenObject(this);    
    
        transform.parent = kitchenObjectParent.GetKitchenObjectFollowTransform();
        transform.localPosition = Vector3.zero;
    }

    public IKitchenObjectParent GetKitchenObjectParent()
  {
        return kitchenObjectParent;
  }
}
