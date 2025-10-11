using System;
using System.Net.NetworkInformation;
using UnityEngine;

public class Player : MonoBehaviour
{

  public  static Player Instance { get; private set; }

  public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
  public class OnSelectedCounterChangedEventArgs : EventArgs
  {
    public ClearCounter selectedCounter;
  }

  [SerializeField] private float moveSpeed = 7f;
  [SerializeField] private float rotateSpeed = 10f;
  [SerializeField] private GameInput gameInput;
  [SerializeField] private LayerMask countersLayerMask;

  private Vector3 lastInteractDir;
  private ClearCounter selectedCounter;


  private void Awake ()
  {
    if(Instance != null)
    {
      Debug.LogError("instance error - null");
    }
    Instance = this;
  }


  private void Start()
  {
    gameInput.OnInteractAction += GameInput_OnInteractAction;
  }

  private void GameInput_OnInteractAction(object sender, EventArgs e)
  {
    if (selectedCounter != null)
    {
      selectedCounter.Interact();
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
      if (raycastHit.transform.TryGetComponent(out ClearCounter clearCounter))
      {
        if (clearCounter != selectedCounter)
        {
          SetSelectedCounter(clearCounter);
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

  private void SetSelectedCounter(ClearCounter selectedCounter)
  {
    this.selectedCounter = selectedCounter;
    OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs
    {
      selectedCounter = selectedCounter
    });
  }
}
