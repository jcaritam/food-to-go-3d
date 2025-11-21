using System;
using UnityEngine;

public class PlatesCounter : BaseCounter
{
    public event EventHandler OnPlateSpawned;
        public event EventHandler OnPlateRemove;
    [SerializeField] private KitchenObjectsSO plateKitchenObjectSO;

    private float spawnPlateTimer;
    private float spawnPlateTImerMax = 4f;
    private int platesSpawnedAmount;
    private int platesSpawnedAmountMax = 4;

    private void Update()
    {
        spawnPlateTimer += Time.deltaTime;

        if (spawnPlateTimer > spawnPlateTImerMax)
        {
            spawnPlateTimer = 0f;

            if (platesSpawnedAmount < platesSpawnedAmountMax)
            {
                platesSpawnedAmount++;
                OnPlateSpawned?.Invoke(this, EventArgs.Empty);
            }
            // KitchenObject.SpawnKitchenObject(plateKitchenObjectSO, this);
        }
    }

  public override void Interact(Player player)
  {
    if (!player.HasKitchenObject())
    {
      if (platesSpawnedAmount > 0)
      {
                platesSpawnedAmount--;
                KitchenObject.SpawnKitchenObject(plateKitchenObjectSO, player);
                OnPlateRemove?.Invoke(this, EventArgs.Empty);
      }
    }
  }
}
