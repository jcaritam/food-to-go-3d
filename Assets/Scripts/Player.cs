using System;
using System.Net.NetworkInformation;
using UnityEngine;

public class Player : MonoBehaviour, IKitchenObjectParent
{
  // TODO: look ar the camera 5:06
  public static Player Instance { get; private set; }

  public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
  public class OnSelectedCounterChangedEventArgs : EventArgs
  {
    public BaseCounter selectedCounter;
  }

  [SerializeField] private float moveSpeed = 7f;
  [SerializeField] private float rotateSpeed = 10f;
  [SerializeField] private GameInput gameInput;
  [SerializeField] private LayerMask countersLayerMask;
  [SerializeField] private Transform KitchenObjectHoldPoint;

  private Vector3 lastInteractDir;
  private BaseCounter selectedCounter;
  private KitchenObject kitchenObject;


  private void Awake()
  {
    if (Instance != null)
    {
      Debug.LogError("instance error - null");
    }
    Instance = this;
  }


  private void Start()
  {
    gameInput.OnInteractAction += GameInput_OnInteractAction;
    gameInput.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;
  }

  private void GameInput_OnInteractAlternateAction(object sender, EventArgs e)
  {
    if (selectedCounter != null)
    {
      selectedCounter.InteractAlternate(this);
    }
  }

  private void GameInput_OnInteractAction(object sender, EventArgs e)
  {
    if (selectedCounter != null)
    {
      selectedCounter.Interact(this);
    }
  }

  private void Update()
  {
    HandleMovementCharacter();
    HandleInteractions();
  }

  private void HandleInteractions()
  {
    Vector2 inputVector = gameInput.GetMovementVectorNormalized();

    Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

    if (moveDir != Vector3.zero)
    {
      lastInteractDir = moveDir;
    }

    float interactDistance = 2f;

    if (Physics.Raycast(transform.position, lastInteractDir, out RaycastHit raycastHit, interactDistance, countersLayerMask))
    {
      if (raycastHit.transform.TryGetComponent(out BaseCounter baseCounter))
      {
        if (baseCounter != selectedCounter)
        {
          SetSelectedCounter(baseCounter);
        }
      }
      else
      {
        SetSelectedCounter(null);
      }
    }
    else
    {
      SetSelectedCounter(null);
    }
  }

  private void HandleMovementCharacter()
  {
    Vector2 inputVector = gameInput.GetMovementVectorNormalized();

    Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

    float moveDistance = moveSpeed * Time.deltaTime;
    float playerRadius = .7f;
    float playerHeight = 2f;

    bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDir, moveDistance);

    if (!canMove)
    {
      Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
      canMove = moveDir.x != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDir, moveDistance);

      if (canMove)
      {
        moveDir = moveDirX;
      }
      else
      {
        Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
        canMove = moveDir.z != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDir, moveDistance);

        if (canMove)
        {
          moveDir = moveDirZ;
        }
      }
    }

    if (canMove)
    {
      transform.position += moveDir * moveDistance;
    }

    if (moveDir != Vector3.zero)
    {
      transform.forward = Vector3.Slerp(
        transform.forward,
        moveDir,
        Time.deltaTime * rotateSpeed
        );

    }
  }

  private void SetSelectedCounter(BaseCounter selectedCounter)
  {
    this.selectedCounter = selectedCounter;
    OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs
    {
      selectedCounter = selectedCounter
    });
  }

  public Transform GetKitchenObjectFollowTransform()
  {
    return KitchenObjectHoldPoint;
  }

  public void SetKitchenObject(KitchenObject kitchenObject)
  {
    this.kitchenObject = kitchenObject;
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
