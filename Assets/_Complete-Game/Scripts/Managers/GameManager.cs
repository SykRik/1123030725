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

	public class GameManager : MonoSingleton<GameManager>
	{
		#region ===== Serialized Fields =====

		[Header("Game Config")]
		[SerializeField] private float preGameDuration = 10f;
		[SerializeField] private float gameDuration = 180f;
		[SerializeField] private int winKillTarget = 50;

		[Header("References")]
		[SerializeField] private PlayerController playerController;

		#endregion

		#region ===== Runtime Fields =====

		private GameState currentState = GameState.Init;
		private float currentTime = 0f;
		private bool isRunning = false;
		private int lastCountdown = -1;
		private bool justEnteredState = false;

		#endregion

		#region ===== Unity Methods =====

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
				case GameState.PreGame:
					UpdatePreGame();
					break;

				case GameState.Playing:
					UpdatePlaying();
					break;
			}
		}

		#endregion

		#region ===== State Logic =====

		private void ChangeState(GameState newState, bool isWin = false, string reason = "")
		{
			if (currentState == newState) return;

			ExitState(currentState);
			currentState = newState;
			EnterState(newState, isWin, reason);

			justEnteredState = true;
		}

		private void EnterState(GameState state, bool isWin = false, string reason = "")
		{
			switch (state)
			{
				case GameState.PreGame:
					Debug.Log("[GameManager] PreGame started. Prepare yourself...");
					currentTime = preGameDuration;
					lastCountdown = -1;
					isRunning = true;
					break;

				case GameState.Playing:
					Debug.Log("[GameManager] Game Started!");
					currentTime = gameDuration;
					EnemyManager.Instance.StartSpawning();
					break;

				case GameState.GameOver:
					Debug.Log($"[GameManager] Game Over - {(isWin ? "Victory" : "Defeat")} - {reason}");
					isRunning = false;
					EnemyManager.Instance.StopSpawning();
					break;
			}
		}

		private void ExitState(GameState state)
		{
			// Reserved for fade out, reset, etc.
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
			{
				ChangeState(GameState.Playing);
			}
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

			if (EnemyManager.Instance.TotalEnemiesKilled >= winKillTarget)
			{
				ChangeState(GameState.GameOver, true, $"Kill Target Reached: {winKillTarget}");
			}
		}

		#endregion
	}
}
