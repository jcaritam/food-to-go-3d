using UnityEngine;

public class ObstacleBladePusher : MonoBehaviour
{
    [Header("Alcance")]
    [Tooltip("Distancia desde el pivote de giro dentro de la cual el jugador puede ser empujado.")]
    [SerializeField] private float bladeReach = 1.5f;
    [Tooltip("Medio angulo del sector del aspa que cuenta como golpe, en grados.")]
    [SerializeField] private float bladeHalfAngle = 35f;

    [Header("Empuje")]
    [Tooltip("Velocidad del desplazamiento aplicado al jugador, en metros por segundo.")]
    [SerializeField] private float pushSpeed = 8f;
    [Tooltip("Peso de la componente tangencial (de barrido) en la direccion de empuje.")]
    [SerializeField] private float tangentialWeight = 0.8f;
    [Tooltip("Peso de la componente radial (hacia afuera) en la direccion de empuje.")]
    [SerializeField] private float radialWeight = 0.4f;
    [Tooltip("Tiempo minimo entre dos empujes consecutivos.")]
    [SerializeField] private float pushCooldown = 0.15f;

    [Header("Referencias")]
    [Tooltip("Pivote de giro del obstaculo. Si se deja vacio, se usa el transform padre.")]
    [SerializeField] private Transform rotationPivot;
    [Tooltip("Plataforma giratoria cuyo sentido de giro determina la direccion tangencial del empuje.")]
    [SerializeField] private RotatingPlatform rotatingPlatform;

    private float lastPushTime;

    private void Awake()
    {
        if (rotationPivot == null)
        {
            rotationPivot = transform.parent;
        }
        if (rotatingPlatform == null && rotationPivot != null)
        {
            rotatingPlatform = rotationPivot.GetComponent<RotatingPlatform>();
        }
    }

    private void FixedUpdate()
    {
        if (KitchenGameManager.Instance == null || !KitchenGameManager.Instance.IsGamePlaying()) return;
        if (Player.Instance == null) return;
        if (rotationPivot == null || rotatingPlatform == null) return;
        if (Time.time - lastPushTime < pushCooldown) return;

        if (TryGetPushDirection(out Vector3 pushDirection))
        {
            Player.Instance.ApplyExternalDisplacement(pushDirection * pushSpeed * Time.fixedDeltaTime);
            lastPushTime = Time.time;
        }
    }

    private bool TryGetPushDirection(out Vector3 pushDirection)
    {
        pushDirection = Vector3.zero;

        Vector3 pivotToPlayer = Player.Instance.transform.position - rotationPivot.position;
        pivotToPlayer.y = 0f;

        if (pivotToPlayer.sqrMagnitude > bladeReach * bladeReach) return false;

        Vector3 bladeForward = transform.position - rotationPivot.position;
        bladeForward.y = 0f;
        if (bladeForward.sqrMagnitude < 0.0001f) return false;

        float angleToPlayer = Vector3.Angle(bladeForward, pivotToPlayer);
        if (angleToPlayer > bladeHalfAngle) return false;

        Vector3 radialDir = pivotToPlayer.normalized;
        float spinSign = rotatingPlatform.IsClockwise ? 1f : -1f;
        Vector3 tangentDir = Vector3.Cross(Vector3.up, radialDir) * spinSign;

        pushDirection = (tangentDir * tangentialWeight + radialDir * radialWeight).normalized;
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Transform pivot = rotationPivot != null ? rotationPivot : transform.parent;
        if (pivot == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(pivot.position, bladeReach);
    }
}
