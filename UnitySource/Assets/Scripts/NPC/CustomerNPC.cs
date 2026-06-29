using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// NPC cliente demostrativo con IA de navegacion.
/// Espera a que el juego comience (GAME_PLAYING), camina hacia una mesa
/// usando NavMeshAgent y se "sienta" (queda en Idle mirando la mesa).
///
/// TODO (siguiente iteracion):
/// - Ligar al DeliveryManager (OnRecipeSpawned / OnRecipeFailed / OnRecipeSuccess)
/// - Animacion real de "sentarse" cuando el rig sea Humanoid
/// - Spawner multi-NPC con asientos asignados por mesa
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class CustomerNPC : MonoBehaviour
{
    private enum State { WaitingForGame, WalkingToTable, Sitting }

    [Header("Navigation")]
    [Tooltip("La mesa a la que se dirige este cliente.")]
    [SerializeField] private Transform targetTable;

    [Tooltip("Distancia lateral desde el centro de la mesa donde se posiciona el asiento.")]
    [SerializeField] private float seatOffset = 1.2f;

    [Header("Animation")]
    [Tooltip("Animator del visual del NPC (usa los parametros 'IsWalking' del Player.controller).")]
    [SerializeField] private Animator animator;

    // Parametro del Player.controller existente en el proyecto
    private static readonly int AnimIsWalking = Animator.StringToHash("IsWalking");

    private NavMeshAgent agent;
    private State state = State.WaitingForGame;
    private Vector3 seatPoint;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.isStopped = true; // espera hasta GAME_PLAYING
    }

    private void Start()
    {
        if (targetTable == null)
        {
            Debug.LogWarning("[CustomerNPC] No hay mesa asignada. Asigna 'targetTable' en el Inspector.", this);
            return;
        }

        // Snapear el NPC al NavMesh desde el inicio (corrige diferencias de Y en el spawn)
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit spawnHit, 2f, NavMesh.AllAreas))
        {
            agent.Warp(spawnHit.position);
        }

        // Calcular el punto de asiento: desplazar desde la mesa hacia donde esta el NPC
        Vector3 dirToNpc = (transform.position - targetTable.position).normalized;
        if (dirToNpc == Vector3.zero) dirToNpc = Vector3.forward;
        Vector3 candidateSeat = targetTable.position + dirToNpc * seatOffset;

        // Proyectar al NavMesh para garantizar que el punto es alcanzable
        if (NavMesh.SamplePosition(candidateSeat, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            seatPoint = hit.position;
        }
        else
        {
            // Fallback: usar la posicion de la mesa directamente
            seatPoint = targetTable.position;
            Debug.LogWarning("[CustomerNPC] No se pudo proyectar el punto de asiento al NavMesh. Usando posicion de mesa.", this);
        }
    }

    private void Update()
    {
        switch (state)
        {
            case State.WaitingForGame:
                HandleWaitingForGame();
                break;

            case State.WalkingToTable:
                HandleWalkingToTable();
                break;

            case State.Sitting:
                // Estado final: no hay nada que actualizar en este ejemplo.
                // En la siguiente iteracion aqui se esperaria el resultado del pedido.
                break;
        }
    }

    private void HandleWaitingForGame()
    {
        // Asegurarse de que KitchenGameManager exista antes de consultarlo
        if (KitchenGameManager.Instance == null) return;
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;

        // El juego comenzo: comenzar a caminar
        agent.isStopped = false;
        agent.SetDestination(seatPoint);
        SetWalkingAnimation(true);
        state = State.WalkingToTable;
    }

    private void HandleWalkingToTable()
    {
        // Esperar a que la ruta este calculada
        if (agent.pathPending) return;

        // Verificar si llegamos al destino
        if (agent.remainingDistance <= agent.stoppingDistance && agent.velocity.sqrMagnitude < 0.01f)
        {
            Sit();
        }
    }

    private void Sit()
    {
        agent.isStopped = true;

        // Rotar para mirar a la mesa
        Vector3 lookDir = (targetTable.position - transform.position);
        lookDir.y = 0f;
        if (lookDir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDir);
        }

        SetWalkingAnimation(false);
        state = State.Sitting;

        Debug.Log("[CustomerNPC] Cliente sentado en mesa: " + (targetTable != null ? targetTable.name : "?"), this);
    }

    private void SetWalkingAnimation(bool isWalking)
    {
        if (animator != null)
        {
            animator.SetBool(AnimIsWalking, isWalking);
        }
    }
}
