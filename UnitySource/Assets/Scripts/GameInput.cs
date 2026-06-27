using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{

    public event EventHandler OnDashAction;
    public event EventHandler OnInteractAction;
    public event EventHandler OnInteractAlternateAction;
    public event EventHandler OnPauseAction;
    public event EventHandler OnThrowAction;

    private PlayerInputActions playerInputActions;

    private void Awake()
    {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Interact.performed += Interact_performed;
        playerInputActions.Player.InteractAlternate.performed += InteractAlternate_performed;
        playerInputActions.Player.Dash.performed += Dash_performed;
        playerInputActions.Player.Pause.performed += Pause_performed;
        playerInputActions.Player.Throw.performed += Throw_performed;
    }

    private void OnDestroy()
    {
        playerInputActions.Player.Interact.performed -= Interact_performed;
        playerInputActions.Player.InteractAlternate.performed -= InteractAlternate_performed;
        playerInputActions.Player.Dash.performed -= Dash_performed;
        playerInputActions.Player.Pause.performed -= Pause_performed;
        playerInputActions.Player.Throw.performed -= Throw_performed;
        playerInputActions.Player.Disable();
        playerInputActions.Dispose();
    }

  private void Dash_performed(InputAction.CallbackContext context)
  {
    OnDashAction?.Invoke(this, EventArgs.Empty);
  }

  private void Throw_performed(InputAction.CallbackContext context)
  {
    OnThrowAction?.Invoke(this, EventArgs.Empty);
  }

  private void Pause_performed(InputAction.CallbackContext context)
  {
    OnPauseAction?.Invoke(this, EventArgs.Empty);
  }

  private void InteractAlternate_performed(InputAction.CallbackContext context)
  {
        OnInteractAlternateAction?.Invoke(this, EventArgs.Empty);
  }

  private void Interact_performed(InputAction.CallbackContext obj)
    {
        OnInteractAction?.Invoke(this, EventArgs.Empty);
    }

    public bool IsInteractHeld()
    {
        return playerInputActions.Player.Interact.IsPressed();
    }

    public Vector2 GetMovementVectorNormalized()
    {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();

        inputVector = inputVector.normalized;

        return inputVector;
    }
}
