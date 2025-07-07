using UnityEngine;
using UnityEngine.UI;
using System;
using UniRx;

namespace HVM
{
    public class UIManager : MonoSingleton<UIManager>
    {
        [Serializable]
        public class CustomButton
        {
            public Button button;
            public Text label;
            public Image cooldownLayer;
            public float cooldownTime = 1f;
        }

        [SerializeField] private Slider hpSlider;
        [SerializeField] private Image screenDamage;
        [SerializeField] private Image screenFader;
        [SerializeField] private Joystick movementJoystick;
        [SerializeField] private Text notifyPopup;
        [SerializeField] private Text gameTime;
        [SerializeField] private Text gameScore;
        [SerializeField] private Text stageText;
        [SerializeField] private CustomButton switchWeaponButton;

        private IDisposable fadeDisposable;

        private void Start()
        {
            if (switchWeaponButton?.label != null)
                switchWeaponButton.label.text = $"{(int)GameManager.Instance.PlayerController.CurrentWeapon}";

            if (movementJoystick != null)
            {
                Observable.EveryUpdate()
                        .Select(_ => movementJoystick.Direction)
                        .Subscribe(dir => InputManager.Instance?.UpdateJoystick(dir))
                        .AddTo(this);
            }

            Observable.EveryUpdate()
                    .Where(_ => GameManager.Instance != null)
                    .Subscribe(_ =>
                        {
                            var gm = GameManager.Instance;
                            float remain = gm.RemainingTime;
                            int m = Mathf.FloorToInt(remain / 60f);
                            int s = Mathf.FloorToInt(remain % 60f);

                            gameTime.text = gm.CurrentState switch
                            {
                                GameState.PreGame => $"Prepare: {m:00}:{s:00}",
                                GameState.Playing => $"Survive: {m:00}:{s:00}",
                                GameState.GameOver => $"Result: {m:00}:{s:00}",
                                _ => "--:--"
                            };

                            gameScore.text = $"SCORE {gm.CurrentKill}/{gm.KillTarget}";
                            stageText.text = $"STAGE {gm.CurrentLevel}";
                        })
                    .AddTo(this);

        }

        private void OnEnable()
        {
            switchWeaponButton?.button?.onClick.AddListener(HandleSwitchWeaponPressed);

            InputManager.Instance.OnSwitchWeaponPressed += HandleSwitchWeaponPressed;
            InputManager.Instance.OnSwitchWeaponReleased += HandleSwitchWeaponReleased;
        }

        private void OnDisable()
        {
            switchWeaponButton?.button?.onClick.RemoveListener(HandleSwitchWeaponPressed);

            InputManager.Instance.OnSwitchWeaponPressed -= HandleSwitchWeaponPressed;
            InputManager.Instance.OnSwitchWeaponReleased -= HandleSwitchWeaponReleased;
        }

        private void HandleSwitchWeaponReleased()
        {
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
                                switchWeaponButton.button.interactable = true;
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
                                    .Scan(0f, (acc, d) => Mathf.Min(1f, acc + d))
                                    .Do(a =>
                                        {
                                            screenFader.color = new Color(0, 0, 0, a * 0.5f);
                                            notifyPopup.color = new Color(1, 1, 1, a);
                                        })
                                    .TakeWhile(a => a < 1f)
                                    .Subscribe()
                                    .AddTo(this);
        }

        public void HideStatusMessage(float fadeDuration = 1f)
        {
            if (notifyPopup == null || screenFader == null) return;
            fadeDisposable?.Dispose();

            fadeDisposable = Observable.EveryUpdate()
                                    .Select(_ => Time.deltaTime / fadeDuration)
                                    .Scan(1f, (acc, d) => Mathf.Max(0f, acc - d))
                                    .Do(a =>
                                        {
                                            screenFader.color = new Color(0, 0, 0, a * 0.5f);
                                            notifyPopup.color = new Color(1, 1, 1, a);
                                        })
                                    .TakeWhile(a => a > 0f)
                                    .Finally(() =>
                                        {
                                            notifyPopup.gameObject.SetActive(false);
                                            screenFader.gameObject.SetActive(false);
                                        })
                                    .Subscribe()
                                    .AddTo(this);
        }
        private IDisposable fadeDamageDisposable;


        public void FlashScreenDamage(float fadeDuration = 0.2f)
        {
            if (screenDamage == null) return;

            fadeDamageDisposable?.Dispose();

            // Reset alpha về 1 (full red)
            screenDamage.color = new Color(1f, 0f, 0f, 0.2f);
            screenDamage.gameObject.SetActive(true);

            fadeDamageDisposable = Observable.EveryUpdate()
                .Select(_ => Time.deltaTime / fadeDuration)
                .Scan(1f, (acc, d) => Mathf.Max(0f, acc - d))
                .Do(a =>
                {
                    screenDamage.color = new Color(1f, 0f, 0f, a);
                })
                .TakeWhile(a => a > 0f)
                .Finally(() =>
                {
                    screenDamage.color = new Color(1f, 0f, 0f, 0f);
                })
                .Subscribe()
                .AddTo(this);
        }

    }
}