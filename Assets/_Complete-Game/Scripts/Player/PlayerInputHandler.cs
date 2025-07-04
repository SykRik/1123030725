using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerInputHandler : MonoBehaviour, PlayerInput.IGameplayActions
{
	public static PlayerInputHandler Instance { get; private set; }

	public Vector2 MoveInput { get; private set; }
	public bool    FireHeld  { get; private set; }

	// ===== Event Triggers =====
	public event Action OnJumpPressed;
	public event Action OnJumpReleased;
	public event Action OnFirePressed;
	public event Action OnFireReleased;
	public event Action OnPausePressed;

	private PlayerInput input;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;

		input = new PlayerInput();
		input.Gameplay.SetCallbacks(this);
		input.Gameplay.Enable();
	}

	private void OnDisable()
	{
		input.Gameplay.Disable();
	}

	// ===== Input Callbacks =====
	public void OnMove(InputAction.CallbackContext context)
	{
		MoveInput = context.ReadValue<Vector2>();
	}

	public void OnJump(InputAction.CallbackContext context)
	{
		if (context.performed)
			OnJumpPressed?.Invoke();
		else if (context.canceled)
			OnJumpReleased?.Invoke();
	}

	public void OnFire(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			FireHeld = true;
			OnFirePressed?.Invoke();
		}
		else if (context.canceled)
		{
			FireHeld = false;
			OnFireReleased?.Invoke();
		}
	}

	public void OnPause(InputAction.CallbackContext context)
	{
		if (context.performed)
			OnPausePressed?.Invoke();
	}
}