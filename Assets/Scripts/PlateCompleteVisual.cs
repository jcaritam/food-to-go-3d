using System;
using System.Collections.Generic;
using UnityEngine;

public class PlateCompleteVisual : MonoBehaviour
{
    [Serializable]
    public struct KitchenObjectsSO_GameObject
    {
        public KitchenObjectsSO kitchenObjectsSO;
        public GameObject gameObject;
    }
    [SerializeField] private PlateKitchenObject plateKitchenObject;
    [SerializeField] private List<KitchenObjectsSO_GameObject> kitchenObjectsSO_GameObjectList;

    private void Start()
    {

        foreach (var item in kitchenObjectsSO_GameObjectList)
        {

            item.gameObject.SetActive(false);

        }
        
        plateKitchenObject.OnIngredientAdded += PlateKitchenObject_OnIngredientAdded;
    }

    private void PlateKitchenObject_OnIngredientAdded(object sender, PlateKitchenObject.OnIngredientAddedEventArgs e)
    {
        foreach (KitchenObjectsSO_GameObject kitchenObjectsSOGameObject in kitchenObjectsSO_GameObjectList)
        {
            if (kitchenObjectsSOGameObject.kitchenObjectsSO == e.kitchenObjectsSO)
            {
                kitchenObjectsSOGameObject.gameObject.SetActive(true);
            }
        }
    }
}
