using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    [SerializeField] private VFXPrefabRefsSO vfxPrefabRefsSO;
    [SerializeField] private int prewarmCount = 3;
    [SerializeField] private Vector3 cutOffset = new Vector3(0f, 1.1f, 0f);
    [SerializeField] private Vector3 deliveryOffset = new Vector3(0f, 1.1f, 0f);

    private Queue<ParticleSystem> deliverySuccessPool;
    private Queue<ParticleSystem> cutPool;
    private Queue<ParticleSystem> burnedPool;

    private DeliveryManager subscribedDeliveryManager;

    private void Awake()
    {
        Instance = this;

        if (vfxPrefabRefsSO == null) return;

        deliverySuccessPool = new Queue<ParticleSystem>();
        cutPool = new Queue<ParticleSystem>();
        burnedPool = new Queue<ParticleSystem>();

        PrewarmPool(deliverySuccessPool, vfxPrefabRefsSO.deliverySuccess, prewarmCount);
        PrewarmPool(cutPool, vfxPrefabRefsSO.cut, prewarmCount);
        PrewarmPool(burnedPool, vfxPrefabRefsSO.burned, prewarmCount);
    }

    private void Start()
    {
        subscribedDeliveryManager = DeliveryManager.Instance;
        if (subscribedDeliveryManager != null)
            subscribedDeliveryManager.OnRecipeSuccess += DeliveryManager_OnRecipeSuccess;
        CuttingCounter.OnAnyCut += CuttingCounter_OnAnyCut;
    }

    private void OnDestroy()
    {
        if (subscribedDeliveryManager != null)
            subscribedDeliveryManager.OnRecipeSuccess -= DeliveryManager_OnRecipeSuccess;

        CuttingCounter.OnAnyCut -= CuttingCounter_OnAnyCut;
    }

    private void DeliveryManager_OnRecipeSuccess(object sender, EventArgs e)
    {
        if (DeliveryCounter.Instance == null) return;
        PlayDeliverySuccess(DeliveryCounter.Instance.transform.position + deliveryOffset);
    }

    private void CuttingCounter_OnAnyCut(object sender, EventArgs e)
    {
        CuttingCounter cuttingCounter = sender as CuttingCounter;
        if (cuttingCounter == null) return;
        PlayCut(cuttingCounter.transform.position + cutOffset);
    }

    public void PlayDeliverySuccess(Vector3 pos)
    {
        if (vfxPrefabRefsSO == null) return;
        Spawn(deliverySuccessPool, vfxPrefabRefsSO.deliverySuccess, pos);
    }

    public void PlayCut(Vector3 pos)
    {
        if (vfxPrefabRefsSO == null) return;
        Spawn(cutPool, vfxPrefabRefsSO.cut, pos);
    }

    public void PlayBurned(Vector3 pos)
    {
        if (vfxPrefabRefsSO == null) return;
        Spawn(burnedPool, vfxPrefabRefsSO.burned, pos);
    }

    private void PrewarmPool(Queue<ParticleSystem> pool, ParticleSystem prefab, int count)
    {
        if (prefab == null) return;

        for (int i = 0; i < count; i++)
        {
            ParticleSystem ps = Instantiate(prefab, transform);
            ps.gameObject.SetActive(false);
            pool.Enqueue(ps);
        }
    }

    private void Spawn(Queue<ParticleSystem> pool, ParticleSystem prefab, Vector3 pos)
    {
        if (prefab == null) return;

        ParticleSystem ps = GetFromPool(pool, prefab);
        ps.transform.position = pos;
        ps.gameObject.SetActive(true);
        ps.Play();

        StartCoroutine(ReturnToPool(ps, pool));
    }

    private ParticleSystem GetFromPool(Queue<ParticleSystem> pool, ParticleSystem prefab)
    {
        int count = pool.Count;
        for (int i = 0; i < count; i++)
        {
            ParticleSystem candidate = pool.Dequeue();
            if (!candidate.isPlaying)
                return candidate;
            pool.Enqueue(candidate);
        }

        return Instantiate(prefab, transform);
    }

    private IEnumerator ReturnToPool(ParticleSystem ps, Queue<ParticleSystem> pool)
    {
        yield return new WaitUntil(() => ps == null || !ps.isPlaying);
        if (ps == null) yield break;
        ps.gameObject.SetActive(false);
        pool.Enqueue(ps);
    }
}
