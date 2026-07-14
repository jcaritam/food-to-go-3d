using UnityEngine;

/// <summary>
/// Marca un punto de asiento en una mesa del comedor. El CustomerSpawner reserva
/// asientos libres para los comensales que entran y los libera cuando se van.
/// </summary>
public class TableSeat : MonoBehaviour
{
    [Tooltip("Punto exacto donde el comensal se sienta y mira. Si se deja vacio, se usa este transform.")]
    [SerializeField] private Transform seatPoint;

    public bool IsOccupied { get; private set; }

    public Vector3 GetSeatPosition() => seatPoint != null ? seatPoint.position : transform.position;
    public Quaternion GetSeatRotation() => seatPoint != null ? seatPoint.rotation : transform.rotation;

    public void Occupy() => IsOccupied = true;
    public void Free() => IsOccupied = false;

    private void OnDrawGizmos()
    {
        Gizmos.color = IsOccupied ? Color.red : Color.green;
        Gizmos.DrawWireSphere(GetSeatPosition(), 0.3f);
    }
}
