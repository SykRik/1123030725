using System;
using HVM;
using UnityEngine;
using UniRx;
using UnityEngine.UI;

public class UIManager : MonoSingleton<UIManager>
{
	#region ===== Nested Classes =====

	[Serializable]
	public class CustomButton
	{
		public Button button        = null;
		public Text   label         = null;
		public Image  cooldownLayer = null;
		public float  cooldownTime  = 1f;
	}

	#endregion

	[SerializeField] private Slider       hpSlider           = null;
	[SerializeField] private Image        screenFader        = null;
	[SerializeField] private Joystick     movementJoystick   = null;
	[SerializeField] private Text         notifyPopup        = null;
	[SerializeField] private CustomButton switchWeaponButton = null;

	private void Start()
	{
		if (switchWeaponButton?.label != null)
		{
			switchWeaponButton.label.text = $"{(int)GameManager.Instance.PlayerController.CurrentWeapon}";
		}

		if (movementJoystick != null)
		{
			Observable.EveryUpdate()
					.Select(_ => movementJoystick.Direction)
					.Subscribe(direction => InputManager.Instance?.UpdateJoystick(direction))
					.AddTo(this);
		}
	}

	private void OnEnable()
	{
		switchWeaponButton?.button?.onClick.AddListener(HandleSwitchWeaponPressed);

		var input = InputManager.Instance;
		if (input != null)
		{
			input.OnPausePressed         += HandlePausePressed;
			input.OnPauseReleased        += HandlePauseReleased;
			input.OnSwitchWeaponPressed  += HandleSwitchWeaponPressed;
			input.OnSwitchWeaponReleased += HandleSwitchWeaponReleased;
		}
	}

	private void OnDisable()
	{
		switchWeaponButton?.button?.onClick.RemoveListener(HandleSwitchWeaponPressed);

		var input = InputManager.Instance;
		if (input != null)
		{
			input.OnPausePressed         -= HandlePausePressed;
			input.OnPauseReleased        -= HandlePauseReleased;
			input.OnSwitchWeaponPressed  -= HandleSwitchWeaponPressed;
			input.OnSwitchWeaponReleased -= HandleSwitchWeaponReleased;
		}
	}

	private void HandlePausePressed()
	{
		Debug.Log("Pause button pressed");
		// TODO: Hiển thị menu pause
	}

	private void HandlePauseReleased()
	{
		Debug.Log("Pause button released");
		// TODO: Ẩn menu pause nếu cần
	}

	private void HandleSwitchWeaponPressed()
	{
		if (switchWeaponButton == null || switchWeaponButton.button == null || switchWeaponButton.label == null)
		{
			Debug.LogWarning("SwitchWeaponButton setup is incomplete.");
			return;
		}

		switchWeaponButton.button.interactable = false;

		GameManager.Instance.PlayerController.SwitchWeapon();
		switchWeaponButton.label.text = $"{(int)GameManager.Instance.PlayerController.CurrentWeapon}";

		if (switchWeaponButton.cooldownLayer != null)
		{
			float duration = Mathf.Max(0.1f, switchWeaponButton.cooldownTime);
			switchWeaponButton.cooldownLayer.fillAmount = 1f;

			Observable.EveryUpdate()
					.Select(_ => Time.deltaTime / duration)
					.Scan(1f, (fill, delta) => Mathf.Max(0f, fill - delta))
					.TakeWhile(fill => fill > 0f)
					.Do(fill => switchWeaponButton.cooldownLayer.fillAmount = fill)
					.Finally(() =>
						{
							switchWeaponButton.cooldownLayer.fillAmount = 0f;
							switchWeaponButton.button.interactable      = true;
						})
					.Subscribe()
					.AddTo(this);
		}
		else
		{
			Observable.Timer(TimeSpan.FromSeconds(switchWeaponButton.cooldownTime))
					.Subscribe(_ => switchWeaponButton.button.interactable = true)
					.AddTo(this);
		}
	}

	private void HandleSwitchWeaponReleased()
	{
		Debug.Log("Switch weapon released");
		// Optional logic
	}

	private IDisposable fadeDisposable;

	public void ShowStatusMessage(string message, float fadeDuration = 1f)
	{
		if (notifyPopup == null || screenFader == null)
			return;

		// Hủy hiệu ứng cũ nếu còn
		fadeDisposable?.Dispose();

		notifyPopup.text = $"<b>{message}</b>";
		notifyPopup.gameObject.SetActive(true);
		screenFader.gameObject.SetActive(true);

		fadeDisposable = Observable.EveryUpdate()
								.Select(_ => Time.deltaTime / fadeDuration)
								.Scan(0f, (acc, delta) => Mathf.Min(1f, acc + delta))
								.Do(alpha =>
									{
										screenFader.color = new Color(0, 0, 0, alpha * 0.5f); // semi-transparent black
										notifyPopup.color = new Color(1, 1, 1, alpha);
									})
								.TakeWhile(alpha => alpha < 1f)
								.Subscribe()
								.AddTo(this);
	}

	public void HideStatusMessage(float fadeDuration = 1f)
	{
		if (notifyPopup == null || screenFader == null)
			return;

		// Hủy hiệu ứng cũ nếu còn
		fadeDisposable?.Dispose();

		fadeDisposable = Observable.EveryUpdate()
								.Select(_ => Time.deltaTime / fadeDuration)
								.Scan(1f, (acc, delta) => Mathf.Max(0f, acc - delta))
								.Do(alpha =>
									{
										screenFader.color = new Color(0, 0, 0, alpha * 0.5f);
										notifyPopup.color = new Color(1, 1, 1, alpha);
									})
								.TakeWhile(alpha => alpha > 0f)
								.Finally(() =>
									{
										notifyPopup.gameObject.SetActive(false);
										screenFader.gameObject.SetActive(false);
									})
								.Subscribe()
								.AddTo(this);
	}

}