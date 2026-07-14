using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Orquesta el comedor: cuando el DeliveryManager genera un WaitingRecipe nuevo,
/// reserva un TableSeat libre y hace entrar a un comensal (elegido al azar entre
/// las variantes regionales configuradas) para que lo represente. Si no hay
/// asientos libres, el pedido queda "en cola" y se resuelve tan pronto se libera
/// una mesa o llega otro pedido nuevo.
/// </summary>
public class CustomerSpawner : MonoBehaviour
{
    [Header("Mesas")]
    [Tooltip("Todos los asientos del comedor disponibles en esta escena.")]
    [SerializeField] private List<TableSeat> seats = new List<TableSeat>();

    [Header("Puntos de entrada")]
    [Tooltip("Puntos desde donde aparecen los comensales (elegido al azar por cada spawn).")]
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

    [Header("Variantes regionales")]
    [Tooltip("Prefabs de CustomerNPC con distinta estetica (andina, costena, selva). Se elige uno al azar por comensal.")]
    [SerializeField] private List<CustomerNPC> customerPrefabVariants = new List<CustomerNPC>();

    private readonly List<CustomerNPC> activeCustomers = new List<CustomerNPC>();
    private readonly Queue<DeliveryManager.WaitingRecipe> pendingRecipes = new Queue<DeliveryManager.WaitingRecipe>();

    private void Start()
    {
        if (DeliveryManager.Instance != null)
        {
            DeliveryManager.Instance.OnRecipeSpawned += DeliveryManager_OnRecipeSpawned;
        }
        else
        {
            Debug.LogWarning("[CustomerSpawner] No hay DeliveryManager en la escena.", this);
        }
    }

    private void OnDestroy()
    {
        if (DeliveryManager.Instance != null)
        {
            DeliveryManager.Instance.OnRecipeSpawned -= DeliveryManager_OnRecipeSpawned;
        }
    }

    private void DeliveryManager_OnRecipeSpawned(object sender, DeliveryManager.OnRecipeEventArgs e)
    {
        pendingRecipes.Enqueue(e.waitingRecipe);
        TrySpawnPending();
    }

    /// <summary>
    /// Llamado por CustomerNPC cuando termina de irse (contento o molesto) y libera su mesa.
    /// Intenta atender el siguiente pedido en cola con el asiento recien liberado.
    /// </summary>
    public void NotifyCustomerLeft(CustomerNPC customer)
    {
        activeCustomers.Remove(customer);
        TrySpawnPending();
    }

    private void TrySpawnPending()
    {
        while (pendingRecipes.Count > 0)
        {
            TableSeat seat = GetFreeSeat();
            if (seat == null) return;

            CustomerNPC prefab = GetRandomVariant();
            if (prefab == null)
            {
                Debug.LogWarning("[CustomerSpawner] No hay variantes de CustomerNPC configuradas.", this);
                return;
            }

            Transform spawnPoint = GetRandomSpawnPoint();
            if (spawnPoint == null)
            {
                Debug.LogWarning("[CustomerSpawner] No hay puntos de entrada configurados.", this);
                return;
            }

            DeliveryManager.WaitingRecipe recipe = pendingRecipes.Dequeue();

            CustomerNPC customer = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
            customer.Initialize(seat, recipe, this);
            activeCustomers.Add(customer);
        }
    }

    private TableSeat GetFreeSeat()
    {
        foreach (TableSeat seat in seats)
        {
            if (seat != null && !seat.IsOccupied) return seat;
        }
        return null;
    }

    private CustomerNPC GetRandomVariant()
    {
        if (customerPrefabVariants == null || customerPrefabVariants.Count == 0) return null;
        return customerPrefabVariants[Random.Range(0, customerPrefabVariants.Count)];
    }

    private Transform GetRandomSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Count == 0) return null;
        return spawnPoints[Random.Range(0, spawnPoints.Count)];
    }
}
