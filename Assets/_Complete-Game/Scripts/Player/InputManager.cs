using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Terresquall;

public class InputManager : MonoBehaviour, PlayerInput.IGameplayActions
{
	public static InputManager Instance { get; private set; }

	public Vector2 MoveInput => VerifyMoveInput();
	
	private Vector2 moveJS = Vector2.zero;
	private Vector2 moveIA = Vector2.zero;

	// ===== Event Triggers =====
	public event Action OnJumpPressed;
	public event Action OnJumpReleased;
	public event Action OnFirePressed;
	public event Action OnFireReleased;
	public event Action OnPausePressed;
	public event Action OnPauseReleased;

	private PlayerInput input;
	
	[SerializeField]
	private VirtualJoystick joystick = null;

	private Vector2 VerifyMoveInput()
	{
		if(moveJS.x != 0 || moveJS.y != 0)
			return moveJS;
		if(moveIA.x != 0 || moveIA.y != 0)
			return moveIA;
		return Vector2.zero;
	}

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

	private void Update()
	{
		if (joystick != null)
		{
			moveJS = joystick.GetAxis();
		}
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

	public void OnJump(InputAction.CallbackContext context)
	{
		var action = 
			context.performed ? OnJumpPressed : 
			context.canceled ? OnJumpReleased : 
			null;
		action?.Invoke();
	}

	public void OnFire(InputAction.CallbackContext context)
	{
		var action = 
			context.performed ? OnFirePressed : 
			context.canceled ? OnFireReleased : 
			null;
		action?.Invoke();
	}

	public void OnPause(InputAction.CallbackContext context)
	{
		var action = 
			context.performed ? OnPausePressed : 
			context.canceled ? OnPauseReleased : 
			null;
		action?.Invoke();
	}
}