using UnityEngine;

/// <summary>
/// Mueve el Sedan automáticamente en bucle por la pista y anima las ruedas.
/// El carro avanza en su transform.forward, y al recorrer loopDistance metros
/// reaparece en la posición inicial (efecto de tráfico decorativo de fondo).
///
/// Para el sonido del motor: asigna un AudioClip de motor al campo "Engine Audio Source"
/// en el Inspector (AudioSource con Loop habilitado). El script lo reproduce/pausa
/// automáticamente según el estado del juego.
/// </summary>
public class SedanAutoDriver : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed    = 6f;
    [SerializeField] private float loopDistance = 40f;

    [Header("Wheels (assign Pivot_Wheel_* transforms)")]
    [SerializeField] private Transform wheelFL;
    [SerializeField] private Transform wheelFR;
    [SerializeField] private Transform wheelRL;
    [SerializeField] private Transform wheelRR;

    [Header("Wheel Settings")]
    [SerializeField] private float wheelSpinSpeed = 200f;

    [Header("Engine Sound")]
    [SerializeField] private AudioSource engineAudioSource;

    private Rigidbody _rb;
    private Vector3   _startPosition;
    private Quaternion _startRotation;
    private float     _distanceTravelled;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb != null)
            _rb.isKinematic = true;   // movimiento cinemático: no cae ni choca con física

        _startPosition = transform.position;
        _startRotation = transform.rotation;
    }

    private void Start()
    {
        if (KitchenGameManager.Instance == null) return;

        KitchenGameManager.Instance.OnGamePaused   += OnGamePaused;
        KitchenGameManager.Instance.OnGameUnpaused += OnGameUnpaused;

        // Arrancar el motor si hay clip asignado y el juego ya está en marcha
        if (engineAudioSource != null && engineAudioSource.clip != null
            && KitchenGameManager.Instance.IsGamePlaying())
        {
            engineAudioSource.Play();
        }
    }

    private void OnDestroy()
    {
        if (KitchenGameManager.Instance == null) return;
        KitchenGameManager.Instance.OnGamePaused   -= OnGamePaused;
        KitchenGameManager.Instance.OnGameUnpaused -= OnGameUnpaused;
    }

    private void FixedUpdate()
    {
        // Pausar tráfico cuando el juego no está activo (cuenta regresiva, pausa, game over)
        if (KitchenGameManager.Instance != null && !KitchenGameManager.Instance.IsGamePlaying())
            return;

        float step = moveSpeed * Time.fixedDeltaTime;
        Vector3 move = transform.forward * step;
        _rb.MovePosition(_rb.position + move);
        _distanceTravelled += step;

        // Animar ruedas
        AnimateWheels();

        // Bucle: al llegar al extremo, regresar al origen
        if (_distanceTravelled >= loopDistance)
        {
            _rb.position       = _startPosition;
            _rb.rotation       = _startRotation;
            _distanceTravelled = 0f;
        }
    }

    private void AnimateWheels()
    {
        float spin = wheelSpinSpeed * Time.fixedDeltaTime;
        if (wheelFL != null) wheelFL.Rotate(spin, 0f, 0f, Space.Self);
        if (wheelFR != null) wheelFR.Rotate(spin, 0f, 0f, Space.Self);
        if (wheelRL != null) wheelRL.Rotate(spin, 0f, 0f, Space.Self);
        if (wheelRR != null) wheelRR.Rotate(spin, 0f, 0f, Space.Self);
    }

    // ── Callbacks de pausa ────────────────────────────────────────────────────

    private void OnGamePaused(object sender, System.EventArgs e)
    {
        if (engineAudioSource != null) engineAudioSource.Pause();
    }

    private void OnGameUnpaused(object sender, System.EventArgs e)
    {
        if (engineAudioSource != null && engineAudioSource.clip != null)
            engineAudioSource.UnPause();
    }
}
