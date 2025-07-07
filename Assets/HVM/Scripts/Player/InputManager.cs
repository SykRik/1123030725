using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InputManager : MonoSingleton<InputManager>, PlayerInput.IGameplayActions
{
	public Vector2 MoveInput => VerifyMoveInput();

	private Vector2 moveJS = Vector2.zero;
	private Vector2 moveIA = Vector2.zero;

	// ===== Event Triggers =====
	public event Action OnPausePressed;
	public event Action OnPauseReleased;
	public event Action OnSwitchWeaponPressed;
	public event Action OnSwitchWeaponReleased;

	private PlayerInput input;

	[SerializeField] private Joystick joystick = null;

	private Vector2 VerifyMoveInput()
	{
		if (moveJS.x != 0 || moveJS.y != 0)
			return moveJS;
		if (moveIA.x != 0 || moveIA.y != 0)
			return moveIA;
		return Vector2.zero;
	}

	protected override void Awake()
	{
		input = new PlayerInput();
		input.Gameplay.SetCallbacks(this);
		input.Gameplay.Enable();
	}

	public void UpdateJoystick(Vector2 direction)
	{
		moveJS = direction;
	}

	private void OnDisable()
	{
		input.Gameplay.Disable();
	}

	// ===== Input Callbacks =====
	public void OnMove(InputAction.CallbackContext context)
	{
		moveIA = context.ReadValue<Vector2>();
	}

	public void OnPause(InputAction.CallbackContext context)
	{
		var action =
			context.performed ? OnPausePressed :
			context.canceled  ? OnPauseReleased :
								null;
		action?.Invoke();
	}

	public void OnSwitchWeapon(InputAction.CallbackContext context)
	{
		var action =
			context.performed ? OnSwitchWeaponPressed :
			context.canceled  ? OnSwitchWeaponReleased :
								null;
		action?.Invoke();
	}
}