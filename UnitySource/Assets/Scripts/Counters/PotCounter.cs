using System;
using UnityEngine;

public class PotCounter : BaseCounter, IHasProgress
{
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;

    public class OnStateChangedEventArgs : EventArgs
    {
        public State state;
    }

    public enum State
    {
        Idle,
        Boiling,
        Boiled,
    }

    [SerializeField] private BoilingRecipeSO[] boilingRecipeSOArray;

    private State state;
    private float boilingTimer;
    private BoilingRecipeSO boilingRecipeSO;

    private void Start()
    {
        state = State.Idle;
    }

    private void Update()
    {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;

        if (HasKitchenObject())
        {
            switch (state)
            {
                case State.Idle:
                    BoilingRecipeSO recipe = GetBoilingRecipeSOWithInput(GetKitchenObject().GetKitChenObjectSO());
                    if (recipe != null)
                    {
                        boilingRecipeSO = recipe;
                        boilingTimer = 0f;
                        state = State.Boiling;
                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { state = state });
                        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs { progressNormalized = 0f });
                    }
                    break;
                case State.Boiling:
                    boilingTimer += Time.deltaTime;

                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                    {
                        progressNormalized = boilingTimer / boilingRecipeSO.boilingTimerMax
                    });

                    if (boilingTimer > boilingRecipeSO.boilingTimerMax)
                    {
                        GetKitchenObject().DestroySelf();

                        KitchenObject.SpawnKitchenObject(boilingRecipeSO.output, this);

                        state = State.Boiled;

                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                        {
                            state = state,
                        });

                        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                        {
                            progressNormalized = 0f
                        });
                    }
                    break;
                case State.Boiled:
                    break;
            }
        }
    }

    public override void Interact(Player player)
    {
        if (!HasKitchenObject())
        {
            if (player.HasKitchenObject())
            {
                if (HasRecipeWithInput(player.GetKitchenObject().GetKitChenObjectSO()))
                {
                    player.GetKitchenObject().SetKitchenObjectParent(this);
                    boilingRecipeSO = GetBoilingRecipeSOWithInput(GetKitchenObject().GetKitChenObjectSO());
                    state = State.Boiling;
                    boilingTimer = 0f;

                    OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                    {
                        state = state,
                    });

                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                    {
                        progressNormalized = boilingTimer / boilingRecipeSO.boilingTimerMax
                    });
                }
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

                        state = State.Idle;

                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                        {
                            state = state,
                        });
                        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                        {
                            progressNormalized = 0f
                        });
                    }
                }
            }
            else
            {
                GetKitchenObject().SetKitchenObjectParent(player);

                state = State.Idle;

                OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                {
                    state = state,
                });
                OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                {
                    progressNormalized = 0f
                });
            }
        }
    }

    public bool CanAcceptThrownObject(KitchenObjectsSO kitchenObjectsSO)
    {
        return !HasKitchenObject() && HasRecipeWithInput(kitchenObjectsSO);
    }

    private bool HasRecipeWithInput(KitchenObjectsSO inputKitchenObjectSO)
    {
        BoilingRecipeSO boilingRecipeSO = GetBoilingRecipeSOWithInput(inputKitchenObjectSO);
        return boilingRecipeSO != null;
    }

    private KitchenObjectsSO GetOutputForInput(KitchenObjectsSO inputKitchenObjectSO)
    {
        BoilingRecipeSO boilingRecipeSO = GetBoilingRecipeSOWithInput(inputKitchenObjectSO);
        if (boilingRecipeSO != null)
        {
            return boilingRecipeSO.output;
        }
        return null;
    }

    private BoilingRecipeSO GetBoilingRecipeSOWithInput(KitchenObjectsSO inputKitchenObjectsSO)
    {
        foreach (BoilingRecipeSO boilingRecipeSO in boilingRecipeSOArray)
        {
            if (boilingRecipeSO.input == inputKitchenObjectsSO)
            {
                return boilingRecipeSO;
            }
        }
        return null;
    }
}
