using System;
using System.Net.NetworkInformation;
using UnityEngine;

public class Player : MonoBehaviour, IKitchenObjectParent
{
    public static Player Instance { get; private set; }

    public event EventHandler OnPickedSomething;
    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArgs : EventArgs
    {
        public BaseCounter selectedCounter;
    }

    [Header("Movement Configuration")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float rotateSpeed = 10f;
    [SerializeField] private float playerRadius = .7f;
    [SerializeField] private float playerHeight = 2f;
    [SerializeField] private LayerMask movementBlockingLayerMask;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private LayerMask countersLayerMask;
    [SerializeField] private LayerMask groundKitchenObjectLayerMask;
    [SerializeField] private Transform KitchenObjectHoldPoint;

    [Header("Jump Configuration")]
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float gravity = -20f;

    [Header("Throw Configuration")]
    [SerializeField] private float throwForwardSpeed = 6f;
    [SerializeField] private float throwUpSpeed = 4f;

    private Vector3 lastInteractDir;
    private BaseCounter selectedCounter;
    private KitchenObject selectedGroundKitchenObject;
    private KitchenObject kitchenObject;
    [SerializeField] private Collider playerCollider;

    private float verticalVelocity = 0f;
    private bool isGrounded = true;
    private float groundLevelY;

    private const float InteractHoldDuration = 3f;
    private const float InteractHoldSearchRadius = 3f;
    private float interactHoldTimer = 0f;
    private bool holdPickupTriggered = false;


    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("instance error - null");
        }
        Instance = this;
        groundLevelY = transform.position.y;
    }


    private void Start()
    {
        gameInput.OnInteractAction += GameInput_OnInteractAction;
        gameInput.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;
        gameInput.OnDashAction += GameInput_OnJumpAction;
        gameInput.OnPauseAction += GameInput_OnPauseAction;
        gameInput.OnThrowAction += GameInput_OnThrowAction;
    }

    private void OnDestroy()
    {
        gameInput.OnInteractAction -= GameInput_OnInteractAction;
        gameInput.OnInteractAlternateAction -= GameInput_OnInteractAlternateAction;
        gameInput.OnDashAction -= GameInput_OnJumpAction;
        gameInput.OnPauseAction -= GameInput_OnPauseAction;
        gameInput.OnThrowAction -= GameInput_OnThrowAction;
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        KitchenGameManager.Instance.TogglePause();
    }

    private void GameInput_OnJumpAction(object sender, EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;

        if (isGrounded)
        {
            verticalVelocity = jumpForce;
            isGrounded = false;
        }
    }

    private void GameInput_OnInteractAlternateAction(object sender, EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;

        if (selectedCounter != null)
        {
            selectedCounter.InteractAlternate(this);
        }
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;

        if (selectedGroundKitchenObject != null && !HasKitchenObject())
        {
            selectedGroundKitchenObject.SetKitchenObjectParent(this);
            return;
        }

        if (selectedCounter != null)
        {
            selectedCounter.Interact(this);
        }
    }

    private void GameInput_OnThrowAction(object sender, EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;
        if (!HasKitchenObject()) return;
        if (kitchenObject is PlateKitchenObject) return;

        Vector3 throwVelocity = transform.forward * throwForwardSpeed + Vector3.up * throwUpSpeed;
        kitchenObject.LaunchAsProjectile(throwVelocity, playerCollider);
        kitchenObject = null;
        interactHoldTimer = 0f;
        holdPickupTriggered = false;
    }

    private void Update()
    {
        HandleJump();
        HandleMovementCharacter();
        HandleInteractions();
        HandleInteractHold();
    }

    private void HandleJump()
    {
        if (KitchenGameManager.Instance.IsGamePaused()) return;
        if (isGrounded && verticalVelocity == 0f) return;

        verticalVelocity += gravity * Time.deltaTime;

        float newY = transform.position.y + verticalVelocity * Time.deltaTime;

        if (newY <= groundLevelY)
        {
            newY = groundLevelY;
            verticalVelocity = 0f;
            isGrounded = true;
        }

        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void HandleInteractions()
    {
        if (KitchenGameManager.Instance.IsGamePaused()) return;

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

        KitchenObject detectedGroundObject = null;
        if (Physics.SphereCast(transform.position, 0.4f, lastInteractDir, out RaycastHit groundHit, interactDistance, groundKitchenObjectLayerMask))
        {
            KitchenObject groundKO = groundHit.collider.GetComponentInParent<KitchenObject>();
            if (groundKO != null && groundKO.IsOnGround())
            {
                detectedGroundObject = groundKO;
            }
        }
        selectedGroundKitchenObject = detectedGroundObject;
    }

    private void HandleInteractHold()
    {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;
        if (HasKitchenObject()) { interactHoldTimer = 0f; holdPickupTriggered = false; return; }

        if (gameInput.IsInteractHeld())
        {
            interactHoldTimer += Time.deltaTime;
            if (!holdPickupTriggered && interactHoldTimer >= InteractHoldDuration)
            {
                holdPickupTriggered = true;
                TryPickupNearestGroundObject();
            }
        }
        else
        {
            interactHoldTimer = 0f;
            holdPickupTriggered = false;
        }
    }

    private void TryPickupNearestGroundObject()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, InteractHoldSearchRadius, groundKitchenObjectLayerMask);

        KitchenObject nearest = null;
        float nearestDist = float.MaxValue;

        foreach (Collider hit in hits)
        {
            KitchenObject ko = hit.GetComponentInParent<KitchenObject>();
            if (ko != null && ko.IsOnGround())
            {
                float dist = Vector3.Distance(transform.position, ko.transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = ko;
                }
            }
        }

        if (nearest != null)
            nearest.SetKitchenObjectParent(this);
    }

    private void HandleMovementCharacter()
    {
        if (KitchenGameManager.Instance.IsGamePaused()) return;

        Vector2 inputVector = gameInput.GetMovementVectorNormalized();

        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        float currentMoveSpeed = moveSpeed;

        float moveDistance = currentMoveSpeed * Time.deltaTime;

        bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDir, moveDistance, movementBlockingLayerMask);

        if (!canMove)
        {
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
            canMove = moveDir.x != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirX, moveDistance, movementBlockingLayerMask);

            if (canMove)
            {
                moveDir = moveDirX;
            }
            else
            {
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;

                canMove = moveDir.z != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirZ, moveDistance, movementBlockingLayerMask);

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
        if (kitchenObject != null)
        {
            OnPickedSomething?.Invoke(this, EventArgs.Empty);
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