using UnityEngine;
using System;
using UniRx;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

namespace HVM
{
	public class UIManager : MonoSingleton<UIManager>
	{
		[Serializable]
		public class CustomButton
		{
			public Button button;
			public Text   label;
			public Image  cooldownLayer;
			public float  cooldownTime = 1f;
		}

		[SerializeField] private Slider       hpSlider;
		[SerializeField] private Image        screenFader;
		[SerializeField] private Joystick     movementJoystick;
		[SerializeField] private Text         notifyPopup;
		[SerializeField] private Text         gameTime;
		[SerializeField] private Text         gameScore;
		[SerializeField] private CustomButton switchWeaponButton;

		private IDisposable fadeDisposable;

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

			Observable.EveryUpdate()
					.Select(_ => GameManager.Instance)
					.Where(gm => gm != null)
					.Subscribe(gm =>
						{
							float remaining = gm.RemainingTime;
							int   minutes   = Mathf.FloorToInt(remaining / 60f);
							int   seconds   = Mathf.FloorToInt(remaining % 60f);

							switch (gm.CurrentState)
							{
								case GameState.PreGame:
									gameTime.text = $"Prepare: {minutes:00}:{seconds:00}";
									break;
								case GameState.Playing:
									gameTime.text = $"Survive: {minutes:00}:{seconds:00}";
									break;
								case GameState.GameOver:
									gameTime.text = $"Result: {minutes:00}:{seconds:00}";
									break;
								default:
									gameTime.text = "--:--";
									break;
							}

							gameScore.text = $"Score: {gm.CurrentKill}/{gm.KillTarget}";
						})
					.AddTo(this);
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

		private void HandlePausePressed()         => Debug.Log("Pause button pressed");
		private void HandlePauseReleased()        => Debug.Log("Pause button released");
		private void HandleSwitchWeaponReleased() => Debug.Log("Switch weapon released");

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

		public void ShowStatusMessage(string message, float fadeDuration = 1f)
		{
			if (notifyPopup == null || screenFader == null) return;
			fadeDisposable?.Dispose();

			notifyPopup.text = $"<b>{message}</b>";
			notifyPopup.gameObject.SetActive(true);
			screenFader.gameObject.SetActive(true);

			fadeDisposable = Observable.EveryUpdate()
									.Select(_ => Time.deltaTime / fadeDuration)
									.Scan(0f, (acc, delta) => Mathf.Min(1f, acc + delta))
									.Do(alpha =>
										{
											screenFader.color = new Color(0, 0, 0, alpha * 0.5f);
											notifyPopup.color = new Color(1, 1, 1, alpha);
										})
									.TakeWhile(alpha => alpha < 1f)
									.Subscribe()
									.AddTo(this);
		}

		public void HideStatusMessage(float fadeDuration = 1f)
		{
			if (notifyPopup == null || screenFader == null) return;
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
}