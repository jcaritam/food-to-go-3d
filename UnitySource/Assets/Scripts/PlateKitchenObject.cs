using System;
using System.Collections.Generic;
using UnityEngine;

public class PlateKitchenObject : KitchenObject
{
    public event EventHandler<OnIngredientAddedEventArgs> OnIngredientAdded;
    public static event EventHandler OnAnyIngredientRejected;

    public class OnIngredientAddedEventArgs : EventArgs
    {
        public KitchenObjectsSO kitchenObjectsSO;
    }

    [SerializeField] private List<KitchenObjectsSO> validKitchenObjectSOList;

    private List<KitchenObjectsSO> kitchenObjectsSOList;

    private void Awake()
    {
        kitchenObjectsSOList = new List<KitchenObjectsSO>();
    }

    public bool TryAddIngredient(KitchenObjectsSO kitchenObjectSO)
    {
        if (!validKitchenObjectSOList.Contains(kitchenObjectSO))
        {
            OnAnyIngredientRejected?.Invoke(this, EventArgs.Empty);
            return false;
        }

        if (kitchenObjectsSOList.Contains(kitchenObjectSO))
        {
            OnAnyIngredientRejected?.Invoke(this, EventArgs.Empty);
            return false;
        }
        else
        {
            kitchenObjectsSOList.Add(kitchenObjectSO);
            OnIngredientAdded?.Invoke(this, new OnIngredientAddedEventArgs
            {
                kitchenObjectsSO = kitchenObjectSO,
            });
            return true;
        }
    }

    public List<KitchenObjectsSO> GetKitchenObjectSOList()
  {
        return kitchenObjectsSOList;
  }
}
