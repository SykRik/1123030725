using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PauseManager : MonoBehaviour
{
	public AudioMixerSnapshot paused;
	public AudioMixerSnapshot unpaused;

	Canvas      canvas;
	PlayerInput playerInput;
	InputAction pauseAction;

	void Awake()
	{
		canvas = GetComponent<Canvas>();

		// Khởi tạo PlayerInput từ file PlayerInput.inputactions
		playerInput = new PlayerInput();
		pauseAction = playerInput.Gameplay.Pause;

		// Gán sự kiện
		pauseAction.performed += ctx => TogglePause();

		// Bật action
		playerInput.Gameplay.Enable();
	}

	void OnEnable()
	{
		playerInput.Gameplay.Enable();
	}

	void OnDisable()
	{
		playerInput.Gameplay.Disable();
	}

	void Update()
	{
		// Không cần kiểm tra input ở đây nữa vì đã dùng event
	}

	void TogglePause()
	{
		canvas.enabled = !canvas.enabled;
		Pause();
	}

	public void Pause()
	{
		Time.timeScale = Time.timeScale == 0 ? 1 : 0;
		Lowpass();
	}

	void Lowpass()
	{
		if (Time.timeScale == 0)
		{
			paused.TransitionTo(.01f);
		}
		else
		{
			unpaused.TransitionTo(.01f);
		}
	}

	public void Quit()
	{
#if UNITY_EDITOR
		EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
	}
}