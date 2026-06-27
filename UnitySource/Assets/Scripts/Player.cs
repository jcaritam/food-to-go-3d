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

    [Header("Dash Configuration")]
    [SerializeField] private float dashSpeedMultiplier = 3f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("Throw Configuration")]
    [SerializeField] private float throwForwardSpeed = 6f;
    [SerializeField] private float throwUpSpeed = 4f;

    private Vector3 lastInteractDir;
    private BaseCounter selectedCounter;
    private KitchenObject selectedGroundKitchenObject;
    private KitchenObject kitchenObject;
    [SerializeField] private Collider playerCollider;

    private float dashTimer = 0f;
    private float cooldownTimer = 0f;
    private bool isDashing = false;

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
    }


    private void Start()
    {
        gameInput.OnInteractAction += GameInput_OnInteractAction;
        gameInput.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;
        gameInput.OnDashAction += GameInput_OnDashAction;
        gameInput.OnPauseAction += GameInput_OnPauseAction;
        gameInput.OnThrowAction += GameInput_OnThrowAction;
    }

    private void OnDestroy()
    {
        gameInput.OnInteractAction -= GameInput_OnInteractAction;
        gameInput.OnInteractAlternateAction -= GameInput_OnInteractAlternateAction;
        gameInput.OnDashAction -= GameInput_OnDashAction;
        gameInput.OnPauseAction -= GameInput_OnPauseAction;
        gameInput.OnThrowAction -= GameInput_OnThrowAction;
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        KitchenGameManager.Instance.TogglePause();
    }

    private void GameInput_OnDashAction(object sender, EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;

        if (!isDashing && cooldownTimer <= 0f)
        {
            Vector2 inputVector = gameInput.GetMovementVectorNormalized();
            
            if (inputVector != Vector2.zero || lastInteractDir != Vector3.zero)
            {
                StartDash();
            }
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
        HandleDashTimers();
        HandleMovementCharacter();
        HandleInteractions();
        HandleInteractHold();
    }

    private void HandleDashTimers()
    {
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;
            }
        }

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
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
        if (isDashing)
        {
            currentMoveSpeed *= dashSpeedMultiplier;
        }
        
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

    private void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        cooldownTimer = dashCooldown;
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