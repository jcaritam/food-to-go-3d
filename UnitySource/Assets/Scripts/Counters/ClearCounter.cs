using UnityEngine;

public class ClearCounter : BaseCounter
{
      [SerializeField] private KitchenObjectsSO kitchenObjectsSO;


      public override void Interact(Player player)
      {
            if (!HasKitchenObject())
            {
                  if (player.HasKitchenObject())
                  {
                        player.GetKitchenObject().SetKitchenObjectParent(this);
                  }
                  else
                  {

                  }
            }
            else
            {
                  if (player.HasKitchenObject())
                  {
                        if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                        {
                              if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitChenObjectSO()))
                              {

                                    GetKitchenObject().DestroySelf();
                              }
                        }
                        else
                        {
                              if (GetKitchenObject().TryGetPlate(out plateKitchenObject))
                              {
                                    if (plateKitchenObject.TryAddIngredient(player.GetKitchenObject().GetKitChenObjectSO()))
                                    {
                                          player.GetKitchenObject().DestroySelf();
                                    }
                              }
                        }
                  }
                  else
                  {
                        GetKitchenObject().SetKitchenObjectParent(player);
                  }
            }
      }



}
