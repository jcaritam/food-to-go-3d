using System.Collections;
using UnityEngine;

public class KitchenObject : MonoBehaviour
{
    [SerializeField] private KitchenObjectsSO kitchenObjectsSO;

    private IKitchenObjectParent kitchenObjectParent;

    private bool isThrown = false;
    private bool isOnGround = false;
    private Rigidbody thrownRb;
    private SphereCollider thrownCollider;

    private int defaultLayer;
    private Quaternion defaultRotation;

    private static PhysicsMaterial sharedBounceMat;

    private void Awake()
    {
        defaultLayer = gameObject.layer;
        defaultRotation = transform.rotation;
    }

    private void OnDestroy()
    {
        if (thrownCollider != null && thrownCollider.material != null)
            Destroy(thrownCollider.material);
    }

    public KitchenObjectsSO GetKitChenObjectSO()
    {
        return kitchenObjectsSO;
    }

    public void SetKitchenObjectParent(IKitchenObjectParent kitchenObjectParent)
    {
        StopAllCoroutines();

        if (this.kitchenObjectParent != null)
        {
            this.kitchenObjectParent.ClearKitchenObject();
        }
        this.kitchenObjectParent = kitchenObjectParent;

        if (thrownRb != null)
        {
            Destroy(thrownRb);
            thrownRb = null;
        }
        if (thrownCollider != null)
        {
            if (thrownCollider.material != null)
                Destroy(thrownCollider.material);
            Destroy(thrownCollider);
            thrownCollider = null;
        }
        isThrown = false;
        isOnGround = false;
        SetLayerRecursive(gameObject, defaultLayer);

        if (kitchenObjectParent.HasKitchenObject())
        {
            Debug.Log("IKitchenObjectParent already has a kitchenObject");
        }

        kitchenObjectParent.SetKitchenObject(this);

        transform.parent = kitchenObjectParent.GetKitchenObjectFollowTransform();
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public IKitchenObjectParent GetKitchenObjectParent()
    {
        return kitchenObjectParent;
    }

    public bool IsOnGround()
    {
        return isOnGround;
    }

    // Suelta el objeto en el suelo sin lanzarlo como proyectil: se usa cuando
    // el IKitchenObjectParent que lo sostenía (ej. una mesa) desaparece y el
    // ingrediente debe quedar recuperable en vez de destruirse con su dueño.
    public void DropToGround()
    {
        if (kitchenObjectParent != null)
        {
            kitchenObjectParent.ClearKitchenObject();
            kitchenObjectParent = null;
        }

        transform.parent = null;
        StopAllCoroutines();
        StartCoroutine(SettleOnGround());
    }

    public void LaunchAsProjectile(Vector3 initialVelocity, Collider playerCollider)
    {
        if (kitchenObjectParent != null)
        {
            kitchenObjectParent.ClearKitchenObject();
            kitchenObjectParent = null;
        }

        transform.parent = null;

        thrownRb = gameObject.AddComponent<Rigidbody>();
        thrownRb.mass = 0.3f;
        thrownRb.useGravity = true;
        thrownRb.linearVelocity = initialVelocity;
        thrownRb.angularVelocity = new Vector3(
            Random.Range(-2f, 2f),
            Random.Range(-2f, 2f),
            Random.Range(-2f, 2f)
        );

        thrownCollider = gameObject.AddComponent<SphereCollider>();
        thrownCollider.radius = 0.25f;
        thrownCollider.isTrigger = false;

        if (sharedBounceMat == null)
        {
            sharedBounceMat = new PhysicsMaterial("ThrownBounce");
            sharedBounceMat.bounciness = 0.4f;
            sharedBounceMat.dynamicFriction = 0.4f;
            sharedBounceMat.staticFriction = 0.4f;
            sharedBounceMat.bounceCombine = PhysicsMaterialCombine.Maximum;
        }
        thrownCollider.material = sharedBounceMat;

        if (playerCollider != null)
            Physics.IgnoreCollision(thrownCollider, playerCollider, true);

        isThrown = true;
        isOnGround = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isThrown) return;

        ContactPoint contact = collision.GetContact(0);
        Vector3 normal = contact.normal;
        bool isTopSurfaceHit = normal.y > 0.5f;

        if (isTopSurfaceHit)
        {
            BaseCounter counter = collision.collider.GetComponentInParent<BaseCounter>();
            if (counter != null && CanLandOnCounter(counter))
            {
                SetKitchenObjectParent(counter);
                return;
            }
            StartCoroutine(SettleOnGround());
        }
    }

    private bool CanLandOnCounter(BaseCounter counter)
    {
        switch (counter)
        {
            case ClearCounter clear:
                return !clear.HasKitchenObject();
            case CuttingCounter cutting:
                return cutting.CanAcceptThrownObject(kitchenObjectsSO);
            case StoveCounter stove:
                return stove.CanAcceptThrownObject(kitchenObjectsSO);
            case PotCounter pot:
                return pot.CanAcceptThrownObject(kitchenObjectsSO);
            default:
                return false;
        }
    }

    private IEnumerator SettleOnGround()
    {
        isThrown = false;
        yield return new WaitForSeconds(0.3f);

        if (thrownRb != null)
        {
            thrownRb.linearVelocity = Vector3.zero;
            thrownRb.angularVelocity = Vector3.zero;
            thrownRb.isKinematic = true;
        }

        isOnGround = true;
        int groundLayer = LayerMask.NameToLayer("KitchenObjectGround");
        int targetLayer = (groundLayer == -1) ? defaultLayer : groundLayer;
        SetLayerRecursive(gameObject, targetLayer);

        float elapsed = 0f;
        float duration = 0.2f;
        Quaternion startRot = transform.rotation;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.rotation = Quaternion.Lerp(startRot, defaultRotation, elapsed / duration);
            yield return null;
        }
        transform.rotation = defaultRotation;
    }

    private static void SetLayerRecursive(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
            SetLayerRecursive(child.gameObject, layer);
    }

    public void DestroySelf()
    {
        if (kitchenObjectParent != null)
        {
            kitchenObjectParent.ClearKitchenObject();
        }
        Destroy(gameObject);
    }

    public bool TryGetPlate(out PlateKitchenObject plateKitchenObject)
    {
        if (this is PlateKitchenObject)
        {
            plateKitchenObject = this as PlateKitchenObject;
            return true;
        }
        else
        {
            plateKitchenObject = null;
            return false;
        }
    }


    public static KitchenObject SpawnKitchenObject(
        KitchenObjectsSO kitchenObjectSO,
        IKitchenObjectParent kitchenObjectParent
    )
    {
        if (kitchenObjectSO == null)
        {
            Debug.LogError("kitchenObjectSO es NULL en SpawnKitchenObject!");
            return null;
        }

        if (kitchenObjectSO.prefab == null)
        {
            Debug.LogError($"El prefab en {kitchenObjectSO.name} no esta asignado!");
            return null;
        }

        if (kitchenObjectParent == null)
        {
            Debug.LogError("kitchenObjectParent es NULL en SpawnKitchenObject!");
            return null;
        }

        Transform kitchenObjectTransform = Instantiate(
            kitchenObjectSO.prefab,
            kitchenObjectParent.GetKitchenObjectFollowTransform().position,
            Quaternion.identity
        );
        KitchenObject kitchenObject = kitchenObjectTransform.GetComponent<KitchenObject>();
        kitchenObject.SetKitchenObjectParent(kitchenObjectParent);

        return kitchenObject;
    }
}
