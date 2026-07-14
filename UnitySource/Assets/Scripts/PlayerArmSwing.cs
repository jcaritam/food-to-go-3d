using UnityEngine;

/// <summary>
/// Procedurally swings the player's little arms back and forth while the character
/// is moving, and eases them to a gentle idle sway when standing still.
/// Movement is detected from the planar world-position delta of this transform,
/// so it works regardless of how the player is driven (input, physics, etc.).
/// </summary>
public class PlayerArmSwing : MonoBehaviour
{
    [Header("Arm Pivots (rotate around local X)")]
    [SerializeField] private Transform armLeft;
    [SerializeField] private Transform armRight;

    [Header("Walk Swing")]
    [Tooltip("How fast the arms swing while walking (radians per second).")]
    [SerializeField] private float swingSpeed = 9f;
    [Tooltip("Maximum swing angle in degrees while walking.")]
    [SerializeField] private float swingAmplitude = 38f;

    [Header("Idle Sway")]
    [Tooltip("Gentle sway angle in degrees while standing still.")]
    [SerializeField] private float idleSwayAmplitude = 4f;
    [Tooltip("How fast the idle sway oscillates.")]
    [SerializeField] private float idleSwaySpeed = 2f;

    [Header("Detection & Smoothing")]
    [Tooltip("Planar speed (units/sec) above which the character is considered moving.")]
    [SerializeField] private float moveThreshold = 0.15f;
    [Tooltip("How quickly the swing blends in/out when starting/stopping.")]
    [SerializeField] private float weightLerpSpeed = 8f;

    private Quaternion armLeftBaseRotation;
    private Quaternion armRightBaseRotation;

    private Vector3 lastPosition;
    private float phase;
    private float walkWeight;

    private void Awake()
    {
        if (armLeft != null) armLeftBaseRotation = armLeft.localRotation;
        if (armRight != null) armRightBaseRotation = armRight.localRotation;
        lastPosition = transform.position;
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        if (deltaTime <= 0f) return;

        // Measure planar movement speed from world-position delta.
        Vector3 delta = transform.position - lastPosition;
        lastPosition = transform.position;
        delta.y = 0f;
        float speed = delta.magnitude / deltaTime;

        // Ease the walk weight in/out for smooth start/stop.
        float targetWeight = speed > moveThreshold ? 1f : 0f;
        walkWeight = Mathf.MoveTowards(walkWeight, targetWeight, weightLerpSpeed * deltaTime);

        // Advance the swing phase while moving.
        phase += swingSpeed * deltaTime * Mathf.Max(walkWeight, 0.0001f);

        float walkAngle = Mathf.Sin(phase) * swingAmplitude * walkWeight;
        float idleAngle = Mathf.Sin(Time.time * idleSwaySpeed) * idleSwayAmplitude * (1f - walkWeight);
        float angle = walkAngle + idleAngle;

        // Left and right arms swing in opposite phase.
        if (armLeft != null)
            armLeft.localRotation = armLeftBaseRotation * Quaternion.Euler(angle, 0f, 0f);
        if (armRight != null)
            armRight.localRotation = armRightBaseRotation * Quaternion.Euler(-angle, 0f, 0f);
    }
}
