using UnityEngine;

/// <summary>
/// Plataforma giratoria estilo carrusel. Los counters y objetos colocados como
/// hijos de este transform rotan con el via la jerarquia de Unity, sin mover cada
/// objeto por codigo. Velocidad y sentido de giro son configurables desde el Inspector.
/// </summary>
public class RotatingPlatform : MonoBehaviour
{
    private enum SpinDirection { Clockwise, CounterClockwise }

    [Header("Giro")]
    [Tooltip("Velocidad de giro en grados por segundo.")]
    [SerializeField] private float rotationSpeed = 20f;
    [Tooltip("Sentido de giro visto desde arriba.")]
    [SerializeField] private SpinDirection direction = SpinDirection.Clockwise;

    [Header("Condiciones")]
    [Tooltip("Si esta activo, la plataforma solo gira mientras la partida esta en juego.")]
    [SerializeField] private bool onlySpinWhilePlaying = true;

    [Header("Gizmos")]
    [Tooltip("Radio del disco dibujado en el editor para visualizar la plataforma.")]
    [SerializeField] private float gizmoRadius = 2.5f;

    public bool IsClockwise => direction == SpinDirection.Clockwise;

    private float SignedSpeed => rotationSpeed * (direction == SpinDirection.Clockwise ? 1f : -1f);

    private void Update()
    {
        if (onlySpinWhilePlaying
            && KitchenGameManager.Instance != null
            && !KitchenGameManager.Instance.IsGamePlaying())
        {
            return;
        }

        transform.Rotate(0f, SignedSpeed * Time.deltaTime, 0f, Space.Self);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, gizmoRadius);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * gizmoRadius);
    }
}
