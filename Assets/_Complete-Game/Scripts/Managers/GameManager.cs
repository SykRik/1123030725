using UnityEngine;

namespace CompleteProject
{
	public enum GameState
	{
		Init,
		PreGame,
		Playing,
		GameOver
	}

	public class GameManager : MonoBehaviour
	{
		[Header("Game Config")]
		[SerializeField] private float preGameDuration = 10f;
		[SerializeField] private float gameDuration    = 180f;

		[Header("References")]
		[SerializeField] private PlayerHealth playerHealth;
		[SerializeField] private EnemyManager enemyManager;

		private GameState currentState  = GameState.Init;
		private float     currentTime   = 0f;
		private bool      isRunning     = false;
		private int       lastCountdown = -1;
		private bool      justEnteredState = false;

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
			if (playerHealth.CurrentHealth <= 0)
			{
				ChangeState(GameState.GameOver, false, "Player Died");
				return;
			}

			if (currentTime <= 0)
			{
				ChangeState(GameState.GameOver, false, "Time Expired");
				return;
			}

			// if (enemyManager.AreAllEnemiesDefeated())
			// {
			// 	ChangeState(GameState.GameOver, true, "All Enemies Defeated");
			// }
		}

		private void ChangeState(GameState newState, bool isWin = false, string reason = "")
		{
			if (currentState == newState) return;

			ExitState(currentState);
			currentState = newState;
			EnterState(newState, isWin, reason);

			justEnteredState = true; // ✅ skip logic 1 frame
		}

		private void EnterState(GameState state, bool isWin = false, string reason = "")
		{
			switch (state)
			{
				case GameState.PreGame:
					Debug.Log("[GameManager] PreGame started. Prepare yourself...");
					currentTime   = preGameDuration;
					lastCountdown = -1;
					isRunning     = true;
					break;

				case GameState.Playing:
					Debug.Log("[GameManager] Game Started!");
					currentTime = gameDuration;
					enemyManager.StartSpawning();
					break;

				case GameState.GameOver:
					Debug.Log($"[GameManager] Game Over - {(isWin ? "Victory" : "Defeat")} - {reason}");
					isRunning = false;
					enemyManager.StopSpawning();
					break;
			}
		}

		private void ExitState(GameState state)
		{
			// Reserved for fade out, reset, etc.
		}
	}
}
