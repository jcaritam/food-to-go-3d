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

                  }
                  else
                  {
                        GetKitchenObject().SetKitchenObjectParent(player);
                  }
            }
      }



}
