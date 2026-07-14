using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Comensal con IA de navegacion. Camina hacia una mesa asignada por el
/// CustomerSpawner, se sienta, espera su pedido (vinculado a un
/// DeliveryManager.WaitingRecipe) y reacciona segun el resultado de la entrega:
/// se va contento si su receta se completa, o se va molesto si expira o si el
/// jugador reparte otra receta equivocada mientras el suyo sigue pendiente.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class CustomerNPC : MonoBehaviour
{
    private enum State { WalkingToTable, Sitting, Leaving }

    [Header("Navigation")]
    [Tooltip("Punto de salida al que camina el comensal cuando termina (contento o molesto). Si se deja vacio, usa el punto de entrada.")]
    [SerializeField] private Transform exitPoint;

    [Header("Animation")]
    [Tooltip("Animator del visual del NPC (usa los parametros 'IsWalking' del Player.controller).")]
    [SerializeField] private Animator animator;

    [Header("Order Bubble")]
    [Tooltip("Componente que muestra el pedido y la barra de paciencia sobre la cabeza del comensal.")]
    [SerializeField] private CustomerOrderBubble orderBubble;

    private static readonly int AnimIsWalking = Animator.StringToHash("IsWalking");

    private NavMeshAgent agent;
    private State state;
    private Vector3 entryPoint;
    private Vector3 seatPoint;
    private Quaternion seatRotation;
    private TableSeat assignedSeat;
    private DeliveryManager.WaitingRecipe waitingRecipe;
    private CustomerSpawner spawner;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    /// <summary>
    /// Inicializa al comensal recien instanciado. Llamado por el CustomerSpawner.
    /// </summary>
    public void Initialize(TableSeat seat, DeliveryManager.WaitingRecipe recipe, CustomerSpawner owner)
    {
        assignedSeat = seat;
        waitingRecipe = recipe;
        spawner = owner;
        entryPoint = transform.position;

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit spawnHit, 2f, NavMesh.AllAreas))
        {
            agent.Warp(spawnHit.position);
        }

        Vector3 candidateSeat = seat.GetSeatPosition();
        if (NavMesh.SamplePosition(candidateSeat, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            seatPoint = hit.position;
        }
        else
        {
            seatPoint = candidateSeat;
            Debug.LogWarning("[CustomerNPC] No se pudo proyectar el asiento al NavMesh. Usando posicion directa.", this);
        }
        seatRotation = seat.GetSeatRotation();

        seat.Occupy();

        if (orderBubble != null) orderBubble.Hide();

        DeliveryManager.Instance.OnRecipeSuccess += DeliveryManager_OnRecipeSuccess;
        DeliveryManager.Instance.OnRecipeFailed += DeliveryManager_OnRecipeFailed;

        agent.SetDestination(seatPoint);
        SetWalkingAnimation(true);
        state = State.WalkingToTable;
    }

    private void OnDestroy()
    {
        if (DeliveryManager.Instance == null) return;
        DeliveryManager.Instance.OnRecipeSuccess -= DeliveryManager_OnRecipeSuccess;
        DeliveryManager.Instance.OnRecipeFailed -= DeliveryManager_OnRecipeFailed;
    }

    private void Update()
    {
        switch (state)
        {
            case State.WalkingToTable:
                HandleWalkingToTable();
                break;

            case State.Sitting:
                if (orderBubble != null && waitingRecipe != null)
                    orderBubble.UpdatePatience(waitingRecipe.timeRemaining / waitingRecipe.timeMax);
                break;

            case State.Leaving:
                HandleLeaving();
                break;
        }
    }

    private void HandleWalkingToTable()
    {
        if (agent.pathPending) return;

        if (agent.remainingDistance <= agent.stoppingDistance && agent.velocity.sqrMagnitude < 0.01f)
        {
            Sit();
        }
    }

    private void Sit()
    {
        agent.isStopped = true;
        transform.rotation = seatRotation;

        SetWalkingAnimation(false);
        state = State.Sitting;

        if (orderBubble != null && waitingRecipe != null)
        {
            orderBubble.Show(waitingRecipe.recipeSO);
        }
    }

    private void DeliveryManager_OnRecipeSuccess(object sender, DeliveryManager.OnRecipeEventArgs e)
    {
        if (state != State.Sitting) return;
        if (e.waitingRecipe != waitingRecipe) return;

        Leave(happy: true);
    }

    private void DeliveryManager_OnRecipeFailed(object sender, DeliveryManager.OnRecipeEventArgs e)
    {
        if (state != State.Sitting) return;
        // e.waitingRecipe es null en entregas equivocadas (no expiro ninguna receta concreta);
        // solo reaccionar cuando la receta que expiro es la propia.
        if (e.waitingRecipe == null || e.waitingRecipe != waitingRecipe) return;

        Leave(happy: false);
    }

    private void Leave(bool happy)
    {
        waitingRecipe = null;
        if (orderBubble != null) orderBubble.ShowReaction(happy);

        if (assignedSeat != null) assignedSeat.Free();

        Vector3 destination = exitPoint != null ? exitPoint.position : entryPoint;
        if (NavMesh.SamplePosition(destination, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            destination = hit.position;
        }

        agent.isStopped = false;
        agent.SetDestination(destination);
        SetWalkingAnimation(true);
        state = State.Leaving;
    }

    private void HandleLeaving()
    {
        if (agent.pathPending) return;

        if (agent.remainingDistance <= agent.stoppingDistance && agent.velocity.sqrMagnitude < 0.01f)
        {
            if (spawner != null) spawner.NotifyCustomerLeft(this);
            Destroy(gameObject);
        }
    }

    private void SetWalkingAnimation(bool isWalking)
    {
        if (animator != null)
        {
            animator.SetBool(AnimIsWalking, isWalking);
        }
    }
}
