using UnityEngine;

public class FoodTruckController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float turnSpeed = 90f;
    [SerializeField] private float acceleration = 10f;

    [Header("Wheels (assign Pivot_Wheel_* transforms)")]
    [SerializeField] private Transform wheelFL;
    [SerializeField] private Transform wheelFR;
    [SerializeField] private Transform wheelRL;
    [SerializeField] private Transform wheelRR;

    [Header("Wheel Settings")]
    [SerializeField] private float wheelSpinSpeed = 200f;
    [SerializeField] private float maxSteerAngle = 25f;

    private Rigidbody _rb;
    private float _currentSpeed;
    private float _currentSteer;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        float vertical   = Input.GetAxisRaw("Vertical");
        float horizontal = Input.GetAxisRaw("Horizontal");

        _currentSpeed = Mathf.MoveTowards(_currentSpeed, vertical * moveSpeed, acceleration * Time.deltaTime);
        _currentSteer = Mathf.MoveTowards(_currentSteer, horizontal * maxSteerAngle, 200f * Time.deltaTime);

        AnimateWheels(vertical, horizontal);
    }

    private void FixedUpdate()
    {
        float vertical   = Input.GetAxisRaw("Vertical");
        float horizontal = Input.GetAxisRaw("Horizontal");

        if (Mathf.Abs(vertical) > 0.01f)
        {
            Vector3 move = transform.forward * _currentSpeed * Time.fixedDeltaTime;
            _rb.MovePosition(_rb.position + move);
        }

        if (Mathf.Abs(vertical) > 0.01f && Mathf.Abs(horizontal) > 0.01f)
        {
            float turn = horizontal * turnSpeed * Time.fixedDeltaTime;
            Quaternion rot = Quaternion.Euler(0f, turn, 0f);
            _rb.MoveRotation(_rb.rotation * rot);
        }
    }

    private void AnimateWheels(float vertical, float horizontal)
    {
        float spinThisFrame = vertical * wheelSpinSpeed * Time.deltaTime;

        if (wheelRL != null) wheelRL.Rotate(spinThisFrame, 0f, 0f, Space.Self);
        if (wheelRR != null) wheelRR.Rotate(spinThisFrame, 0f, 0f, Space.Self);
        if (wheelFL != null) wheelFL.Rotate(spinThisFrame, 0f, 0f, Space.Self);
        if (wheelFR != null) wheelFR.Rotate(spinThisFrame, 0f, 0f, Space.Self);

        if (wheelFL != null)
        {
            Vector3 e = wheelFL.localEulerAngles;
            e.z = _currentSteer;
            wheelFL.localEulerAngles = e;
        }
        if (wheelFR != null)
        {
            Vector3 e = wheelFR.localEulerAngles;
            e.z = _currentSteer;
            wheelFR.localEulerAngles = e;
        }
    }
}
