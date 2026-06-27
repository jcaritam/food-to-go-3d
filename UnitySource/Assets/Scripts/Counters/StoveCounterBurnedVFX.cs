using System;
using UnityEngine;

public class StoveCounterBurnedVFX : MonoBehaviour
{
    [SerializeField] private StoveCounter stoveCounter;
    [SerializeField] private Vector3 vfxOffset = new Vector3(0f, 1.1f, 0f);

    private void Start()
    {
        stoveCounter.OnStateChanged += StoveCounter_OnStateChanged;
    }

    private void OnDestroy()
    {
        stoveCounter.OnStateChanged -= StoveCounter_OnStateChanged;
    }

    private void StoveCounter_OnStateChanged(object sender, StoveCounter.OnStateChangedEventArgs e)
    {
        if (e.state == StoveCounter.State.Burned && VFXManager.Instance != null)
        {
            VFXManager.Instance.PlayBurned(stoveCounter.transform.position + vfxOffset);
        }
    }
}
