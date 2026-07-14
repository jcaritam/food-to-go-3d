using System.Collections;
using UnityEngine;

// Variante de obstáculo dinámico: en vez de subir/bajar (ver DynamicBlockade),
// se desliza lateralmente en el plano horizontal (X o Z) y telegrafía su cierre
// con un parpadeo antes de bloquear el paso. Al moverse en el plano, no depende
// de "hundir" un collider bajo el suelo, evitando el bug de DynamicBlockade.
public class SlidingBarrier : MonoBehaviour
{
    private enum SlideAxis { X, Z }

    [Header("Configuración de Tiempo")]
    [Tooltip("Segundos que la barrera permanece abierta antes del primer ciclo de cierre.")]
    [SerializeField] private float initialOpenTime = 10f;
    [Tooltip("Duración del deslizamiento entre abierto y cerrado.")]
    [SerializeField] private float transitionDuration = 1f;

    [Header("Aviso previo (telegrafía)")]
    [Tooltip("Duración del parpadeo de aviso antes de que la barrera empiece a cerrar.")]
    [SerializeField] private float warningDuration = 1.5f;
    [Tooltip("Intervalo entre cada cambio de color del parpadeo de aviso.")]
    [SerializeField] private float warningBlinkInterval = 0.15f;

    [Header("Rangos de Variación")]
    [SerializeField] private float minTimeOpen = 8f;
    [SerializeField] private float maxTimeOpen = 14f;
    [SerializeField] private float minTimeClosed = 6f;
    [SerializeField] private float maxTimeClosed = 10f;

    [Header("Desplazamiento")]
    [Tooltip("Eje del plano horizontal por el que se desliza la barrera.")]
    [SerializeField] private SlideAxis slideAxis = SlideAxis.X;
    [Tooltip("Distancia recorrida entre la posición abierta y la cerrada.")]
    [SerializeField] private float slideDistance = 2f;

    [Header("Referencias")]
    [Tooltip("Renderers que parpadean durante el aviso previo al cierre.")]
    [SerializeField] private Renderer[] warningRenderers;

    private Vector3 openPosition;
    private Vector3 closedPosition;
    private Collider[] blockingColliders;
    private Color[] originalColors;

    private void Start()
    {
        openPosition = transform.position;
        Vector3 offset = slideAxis == SlideAxis.X
            ? new Vector3(slideDistance, 0f, 0f)
            : new Vector3(0f, 0f, slideDistance);
        closedPosition = openPosition + offset;

        blockingColliders = GetComponentsInChildren<Collider>(true);
        CacheOriginalColors();

        SetCollidersEnabled(false);

        StartCoroutine(BarrierCycle());
    }

    private void CacheOriginalColors()
    {
        if (warningRenderers == null || warningRenderers.Length == 0) return;

        originalColors = new Color[warningRenderers.Length];
        for (int i = 0; i < warningRenderers.Length; i++)
        {
            if (warningRenderers[i] != null && warningRenderers[i].material.HasProperty("_Color"))
            {
                originalColors[i] = warningRenderers[i].material.color;
            }
        }
    }

    private void SetCollidersEnabled(bool isEnabled)
    {
        foreach (Collider col in blockingColliders)
        {
            col.enabled = isEnabled;
        }
    }

    private IEnumerator BarrierCycle()
    {
        yield return new WaitForSeconds(initialOpenTime);

        while (true)
        {
            yield return StartCoroutine(WarningBlink());

            yield return StartCoroutine(MoveBarrier(openPosition, closedPosition, transitionDuration));
            SetCollidersEnabled(true);

            float currentClosedTime = Random.Range(minTimeClosed, maxTimeClosed);
            yield return new WaitForSeconds(currentClosedTime);

            SetCollidersEnabled(false);
            yield return StartCoroutine(MoveBarrier(closedPosition, openPosition, transitionDuration));

            float currentOpenTime = Random.Range(minTimeOpen, maxTimeOpen);
            yield return new WaitForSeconds(currentOpenTime);
        }
    }

    private IEnumerator WarningBlink()
    {
        if (warningRenderers == null || warningRenderers.Length == 0)
        {
            yield return new WaitForSeconds(warningDuration);
            yield break;
        }

        float elapsed = 0f;
        bool blinkOn = false;

        while (elapsed < warningDuration)
        {
            blinkOn = !blinkOn;
            SetWarningColor(blinkOn ? Color.red : Color.white);
            yield return new WaitForSeconds(warningBlinkInterval);
            elapsed += warningBlinkInterval;
        }

        RestoreOriginalColors();
    }

    private void SetWarningColor(Color color)
    {
        foreach (Renderer rend in warningRenderers)
        {
            if (rend != null && rend.material.HasProperty("_Color"))
            {
                rend.material.color = color;
            }
        }
    }

    private void RestoreOriginalColors()
    {
        if (originalColors == null) return;

        for (int i = 0; i < warningRenderers.Length; i++)
        {
            if (warningRenderers[i] != null && warningRenderers[i].material.HasProperty("_Color"))
            {
                warningRenderers[i].material.color = originalColors[i];
            }
        }
    }

    private IEnumerator MoveBarrier(Vector3 currentPos, Vector3 targetPos, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            transform.position = Vector3.Lerp(currentPos, targetPos, t * t * (3f - 2f * t));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
    }
}
