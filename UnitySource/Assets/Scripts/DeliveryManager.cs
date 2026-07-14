using System;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    public class WaitingRecipe
    {
        public RecipeSO recipeSO;
        public float timeRemaining;
        public float timeMax;
    }

    public class OnRecipeEventArgs : EventArgs
    {
        public WaitingRecipe waitingRecipe;
    }

    public event EventHandler<OnRecipeEventArgs> OnRecipeSpawned;
    public event EventHandler OnRecipeComplete;
    public event EventHandler<OnRecipeEventArgs> OnRecipeSuccess;
    public event EventHandler<OnRecipeEventArgs> OnRecipeFailed;
    public event EventHandler OnWrongDeliveryPenalty;

    public static DeliveryManager Instance { get; private set; }

    [SerializeField] private RecipeListSO recipeListSO;
    [SerializeField] private float recipeTimeMax = 60f;

    private List<WaitingRecipe> waitingRecipeList;
    private List<RecipeSO> completeRecipeSOList;
    private float spawnRecipeTimer;
    private float spawnRecipeTimerMax = 4f;
    private int waitingRecipeMax = 4;
    private int failedRecipeCount;
    private int wrongDeliveryCount;
    private int totalPenaltyPoints;
    private const int WRONG_DELIVERY_PENALTY = 20;

    private void Awake()
    {
        Instance = this;
        waitingRecipeList = new List<WaitingRecipe>();
        completeRecipeSOList = new List<RecipeSO>();
    }

    private void Update()
    {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;

        spawnRecipeTimer -= Time.deltaTime;
        if (spawnRecipeTimer <= 0f)
        {
            spawnRecipeTimer = spawnRecipeTimerMax;
            if (waitingRecipeList.Count < waitingRecipeMax)
            {
                RecipeSO recipeSO = recipeListSO.recipeSOList[UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count)];
                WaitingRecipe waitingRecipe = new WaitingRecipe { recipeSO = recipeSO, timeRemaining = recipeTimeMax, timeMax = recipeTimeMax };
                waitingRecipeList.Add(waitingRecipe);
                OnRecipeSpawned?.Invoke(this, new OnRecipeEventArgs { waitingRecipe = waitingRecipe });
            }
        }

        for (int i = waitingRecipeList.Count - 1; i >= 0; i--)
        {
            waitingRecipeList[i].timeRemaining -= Time.deltaTime;
            if (waitingRecipeList[i].timeRemaining <= 0f)
            {
                WaitingRecipe expiredRecipe = waitingRecipeList[i];
                waitingRecipeList.RemoveAt(i);
                failedRecipeCount++;
                OnRecipeFailed?.Invoke(this, new OnRecipeEventArgs { waitingRecipe = expiredRecipe });
                OnRecipeComplete?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public void DeliveryRecipe(PlateKitchenObject plateKitchenObject)
    {
        for (int i = 0; i < waitingRecipeList.Count; i++)
        {
            WaitingRecipe waitingRecipe = waitingRecipeList[i];
            RecipeSO waitingRecipeSO = waitingRecipe.recipeSO;

            if (waitingRecipeSO.kitchenObjectSOList.Count == plateKitchenObject.GetKitchenObjectSOList().Count)
            {
                bool plateContentsMatchesRecipe = true;
                foreach (KitchenObjectsSO recipeKitchenObjectSO in waitingRecipeSO.kitchenObjectSOList)
                {
                    bool ingredientFound = false;
                    foreach (KitchenObjectsSO plateKitchenObjectSO in plateKitchenObject.GetKitchenObjectSOList())
                    {
                        if (plateKitchenObjectSO == recipeKitchenObjectSO)
                        {
                            ingredientFound = true;
                            break;
                        }
                    }
                    if (!ingredientFound) plateContentsMatchesRecipe = false;
                }

                if (plateContentsMatchesRecipe)
                {
                    completeRecipeSOList.Add(waitingRecipeSO);
                    waitingRecipeList.RemoveAt(i);
                    OnRecipeComplete?.Invoke(this, EventArgs.Empty);
                    OnRecipeSuccess?.Invoke(this, new OnRecipeEventArgs { waitingRecipe = waitingRecipe });
                    return;
                }
            }
        }

        wrongDeliveryCount++;
        totalPenaltyPoints += WRONG_DELIVERY_PENALTY;
        OnWrongDeliveryPenalty?.Invoke(this, EventArgs.Empty);
        OnRecipeFailed?.Invoke(this, new OnRecipeEventArgs { waitingRecipe = null });
        OnRecipeComplete?.Invoke(this, EventArgs.Empty);
    }

    public List<WaitingRecipe> GetWaitingRecipeList() => waitingRecipeList;
    public List<RecipeSO> GetCompleteRecipeSOList() => completeRecipeSOList;
    public int GetFailedRecipeCount() => failedRecipeCount;
    public int GetWrongDeliveryCount() => wrongDeliveryCount;
    public int GetTotalPenaltyPoints() => totalPenaltyPoints;
}
