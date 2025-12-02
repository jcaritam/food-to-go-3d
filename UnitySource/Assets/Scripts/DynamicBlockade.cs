using System.Collections;
using UnityEngine;

public class DynamicBlockade : MonoBehaviour
{
  [Header("Configuración de Tiempo")]
  [SerializeField] private float initialHiddenTime = 60f;
  [SerializeField] private float transitionDuration = 1.5f;

  [Header("Rangos de Variación")]
  [SerializeField] private float minTimeVisible = 40f;
  [SerializeField] private float maxTimeVisible = 50f;
  [SerializeField] private float minTimeHidden = 15f;
  [SerializeField] private float maxTimeHidden = 30f;

  [Header("Posiciones")]
  private Vector3 endPosition;
  [SerializeField] private float heightOffset = 2f;

  private void Start()
  {
    endPosition = transform.position;
    Vector3 startPosition = endPosition - new Vector3(0f, heightOffset, 0f);

    transform.position = startPosition;

    StartCoroutine(BlockadeCycle(startPosition, endPosition));
  }

  private IEnumerator BlockadeCycle(Vector3 startPos, Vector3 endPos)
  {
    yield return new WaitForSeconds(initialHiddenTime);

    while (true)
    {
      yield return StartCoroutine(MoveBlockade(startPos, endPos, transitionDuration));

      float currentVisibleTime = Random.Range(minTimeVisible, maxTimeVisible);
      yield return new WaitForSeconds(currentVisibleTime);

      yield return StartCoroutine(MoveBlockade(endPos, startPos, transitionDuration));

      float currentHiddenTime = Random.Range(minTimeHidden, maxTimeHidden);
      yield return new WaitForSeconds(currentHiddenTime);
    }
  }

  private IEnumerator MoveBlockade(Vector3 currentPos, Vector3 targetPos, float duration)
  {
    float elapsedTime = 0f;

    while (elapsedTime < duration)
    {
      transform.position = Vector3.Lerp(currentPos, targetPos, (elapsedTime / duration));
      elapsedTime += Time.deltaTime;
      yield return null;
    }

    transform.position = targetPos;
  }
}