using System;
using Unity.VisualScripting;
using UnityEngine;

public class BaseCounter : MonoBehaviour, IKitchenObjectParent
{
    public static event EventHandler OnAnyObjectPlacedHere;

    [SerializeField] private Transform counterTopPoint;

    private KitchenObject kitchenObject;

    // Si el counter/mesa desaparece mientras sostiene un ingrediente, este
    // no debe destruirse en cascada junto con el Transform padre: se
    // desprende y se deja caer al suelo para que el jugador pueda recogerlo.
    private void OnDestroy()
    {
        if (kitchenObject != null)
        {
            kitchenObject.DropToGround();
        }
    }

    public virtual void Interact(Player player)
    {
        Debug.Log("BAseCounter.Interact");
    }

    public virtual void InteractAlternate(Player player)
    {
        // Debug.Log("BAseCounter.InteractAlternate");
    }

    public Transform GetKitchenObjectFollowTransform()
    {
        return counterTopPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.kitchenObject = kitchenObject;
        if (kitchenObject != null)
        {
            OnAnyObjectPlacedHere?.Invoke(this, EventArgs.Empty);
        }
    }

    public KitchenObject GetKitchenObject()
    {
        return kitchenObject;
    }

    public void ClearKitchenObject()
    {
        kitchenObject = null;
    }

    public bool HasKitchenObject()
    {
        return kitchenObject != null;
    }
}
