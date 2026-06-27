using UnityEngine;

public class PotCounterVisual : MonoBehaviour
{
    [SerializeField] private PotCounter potCounter;
    [SerializeField] private GameObject steamGameObject;
    [SerializeField] private GameObject bubblesGameObject;

    private void Start()
    {
        potCounter.OnStateChanged += PotCounter_OnStateChanged;
    }

    private void OnDestroy()
    {
        potCounter.OnStateChanged -= PotCounter_OnStateChanged;
    }

    private void PotCounter_OnStateChanged(object sender, PotCounter.OnStateChangedEventArgs e)
    {
        bool showVisual = e.state == PotCounter.State.Boiling || e.state == PotCounter.State.Boiled;

        steamGameObject.SetActive(showVisual);
        bubblesGameObject.SetActive(showVisual);
    }
}
