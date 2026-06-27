using UnityEngine;

public class PotCounterSound : MonoBehaviour
{
    [SerializeField] private PotCounter potCounter;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

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
        bool playSound = e.state == PotCounter.State.Boiling || e.state == PotCounter.State.Boiled;
        if (playSound)
        {
            audioSource.Play();
        }
        else
        {
            audioSource.Pause();
        }
    }
}
