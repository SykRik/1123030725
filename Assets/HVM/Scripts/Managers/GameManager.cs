using System;
using System.Linq;
using UnityEngine;

namespace HVM
{
	public enum GameState
	{
		Init,
		PreGame,
		Playing,
		GameOver
	}

	[Serializable]
	public class LevelConfig
	{
		public int A;
		public int B;
		public int C;
	}

	public class GameManager : MonoSingleton<GameManager>
	{
		[Header("Game Config")]
		[SerializeField] private float preGameDuration = 10f;
		[SerializeField] private float gameDuration = 150f;
		[SerializeField] private float resultDuration = 10f;
		[SerializeField] private PlayerController playerController;
		
		private LevelConfig[] levels = {
			new LevelConfig { A = 30, B = 30, C = 0 },
			new LevelConfig { A = 30, B = 29, C = 1 }
		};

		#region === Properties ===

		public PlayerController PlayerController => playerController;
		public float RemainingTime => Mathf.Max(0f, currentTime);
		public GameState CurrentState => currentState;
		public int CurrentKill => EnemyManager.Instance?.TotalEnemiesKilled ?? 0;
		public int KillTarget => TryGetLevelKillTarget(currentLevel, out var target) ? target : 0;
		public int CurrentLevel => currentLevel;
		public int MaxLevel => levels?.Length ?? 0;

		#endregion

		#region === Runtime ===

		private GameState currentState = GameState.Init;
		private float currentTime = 0f;
		private bool isRunning = false;
		private int lastCountdown = -1;
		private bool justEnteredState = false;
		private int currentLevel = 0;

		#endregion

		private void Start()
		{
			ChangeState(GameState.PreGame);
		}

		private void Update()
		{
			if (!isRunning) return;

			if (justEnteredState)
			{
				justEnteredState = false;
				return;
			}

			currentTime -= Time.deltaTime;

			switch (currentState)
			{
				case GameState.PreGame: UpdatePreGame(); break;
				case GameState.Playing: UpdatePlaying(); break;
				case GameState.GameOver: UpdateResult(); break;
			}
		}

		private void ChangeState(GameState newState, bool isWin = false, string reason = "")
		{
			if (currentState == newState) return;

			Debug.Log($"[GameManager] Changing state {currentState} → {newState}");
			ExitState(currentState);
			currentState = newState;
			justEnteredState = true;
			EnterState(newState, isWin, reason);
		}

		private void EnterState(GameState state, bool isWin = false, string reason = "")
		{
			switch (state)
			{
				case GameState.PreGame:
					Debug.Log("[GameManager] PreGame started");
					ResetPlayer();
					EnemyManager.Instance?.ResetKillCount();
					currentTime = preGameDuration;
					lastCountdown = -1;
					isRunning = true;
					break;

				case GameState.Playing:
					Debug.Log($"[GameManager] Game Started for level {currentLevel}");
					currentTime = gameDuration;
					EnemyManager.Instance?.StartSpawning(TryGetLevelConfig(currentLevel, out var config) ? config : null);
					break;

				case GameState.GameOver:
					Debug.Log($"[GameManager] Game Over - {(isWin ? "Victory" : "Defeat")} - {reason}");
					currentTime = resultDuration;
					isRunning = true;
					EnemyManager.Instance?.StopSpawning();
					EnemyManager.Instance?.ReturnAllAliveEnemiesToPool();
					UIManager.Instance.ShowStatusMessage(isWin ? "Next Level" : "Game Over");

					if (isWin)
					{
						if (currentLevel + 1 < MaxLevel)
							currentLevel++;
						else
							currentLevel = 0;
					}
					break;
			}
		}

		private void ExitState(GameState state)
		{
			if (state == GameState.GameOver)
				UIManager.Instance.HideStatusMessage();
		}

		private void UpdatePreGame()
		{
			if (currentTime <= 3f)
			{
				int countdown = Mathf.CeilToInt(currentTime);
				if (countdown != lastCountdown && countdown > 0)
				{
					lastCountdown = countdown;
					Debug.Log($"[GameManager] Starting in: {countdown}");
				}
			}

			if (currentTime <= 0f)
				ChangeState(GameState.Playing);
		}

		private void UpdatePlaying()
		{
			if (playerController.CurrentHealth <= 0)
			{
				ChangeState(GameState.GameOver, false, "Player Died");
				return;
			}

			if (currentTime <= 0)
			{
				ChangeState(GameState.GameOver, false, "Time Expired");
				return;
			}

			if (CurrentKill >= KillTarget)
			{
				ChangeState(GameState.GameOver, true, $"Kill Target Reached: {KillTarget}");
			}
		}

		private void UpdateResult()
		{
			if (currentTime <= 0f)
			{
				ChangeState(GameState.PreGame);
			}
		}

		private void ResetPlayer()
		{
			if (playerController == null) return;
			playerController.ResetState();
			playerController.transform.position = Vector3.zero;
			playerController.gameObject.SetActive(true);
		}

		private bool TryGetLevelConfig(int index, out LevelConfig config)
		{
			config = levels == null || levels.Length == 0 ? null : levels.Length > index ? levels[index] : levels.Last();

			return config != null;
		}

		private bool TryGetLevelKillTarget(int index, out int amount)
		{
			amount = TryGetLevelConfig(index, out var config) ? config.A + config.B  + config.C: 0;
			return amount > 0;
		}
	}
}
