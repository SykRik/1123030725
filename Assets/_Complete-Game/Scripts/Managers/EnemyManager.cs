using System.Linq;
using UnityEngine;

namespace HVM
{
	[RequireComponent(typeof(EnemySpawner))]
	[RequireComponent(typeof(EnemyTracking))]
	public class EnemyManager : MonoBehaviour
	{
		public enum TypeOfEnemy
		{
			A,
			B,
			C,
			Undefine
		}

		[Header("Game References")]
		[SerializeField] private PlayerHealth playerHealth;

		[Header("Spawning Settings")]
		[SerializeField] private float spawnInterval = 3f;

		[Header("Enemy Poolers (By Type)")]
		[SerializeField] private EnemyPooler poolerA;
		[SerializeField] private EnemyPooler poolerB;
		[SerializeField] private EnemyPooler poolerC;

		[Header("Internal References")]
		[SerializeField] private EnemySpawner  spawner;
		[SerializeField] private EnemyTracking tracker;

		private float spawnTimer = 0f;
		private bool  isRunning  = false;

		#region Unity Methods

		private void OnValidate()
		{
			spawner = GetComponent<EnemySpawner>();
			tracker = GetComponent<EnemyTracking>();

#if UNITY_EDITOR
			if (spawner == null || tracker == null)
				Debug.LogError("[EnemyManager] Missing required internal references.", this);
#endif
		}

		private void Update()
		{
			if (!isRunning || playerHealth.CurrentHealth <= 0f)
				return;

			spawnTimer -= Time.deltaTime;

			if (spawnTimer <= 0f)
			{
				spawnTimer = spawnInterval;
				SpawnEnemy(TypeOfEnemy.A); // 🔁 Future: support more types
			}
		}

		#endregion

		#region Public Methods

		public void StartSpawning()
		{
			isRunning  = true;
			spawnTimer = spawnInterval;
			Debug.Log("[EnemyManager] Spawning started.");
		}

		public void StopSpawning()
		{
			isRunning = false;
			Debug.Log("[EnemyManager] Spawning stopped.");
		}

		public bool AreAllEnemiesDefeated()
		{
			return tracker.AllEnemiesDefeated;
		}

		#endregion

		#region Private Methods

		private void SpawnEnemy(TypeOfEnemy type)
		{
			var pooler = GetPooler(type);

			if (pooler == null)
			{
				Debug.LogWarning($"[EnemyManager] No pooler assigned for type: {type}");
				return;
			}

			if (!pooler.TryRequest(out var enemy))
			{
				Debug.LogWarning($"[EnemyManager] Failed to get enemy from pool for type: {type}");
				return;
			}

			spawner.Spawn(enemy);
			tracker.RegisterEnemy(enemy);

			Debug.Log($"[EnemyManager] Spawned enemy of type: {type}");
		}

		private EnemyPooler GetPooler(TypeOfEnemy type)
		{
			return type switch
			{
				TypeOfEnemy.A => poolerA,
				TypeOfEnemy.B => poolerB,
				TypeOfEnemy.C => poolerC,
				_             => null,
			};
		}

		public bool TryGetClosedEnemy(Vector3 position, float range, out EnemyController enemyController)
		{
			if (TryGetEnemyClosed(poolerA, position, range, out enemyController))
				return true;
			if (TryGetEnemyClosed(poolerB, position, range, out enemyController))
				return true;
			if (TryGetEnemyClosed(poolerC, position, range, out enemyController))
				return true;
			
			enemyController = null;
			return false;
		}

		private bool TryGetEnemyClosed(EnemyPooler pooler, Vector3 position, float range, out EnemyController enemyController)
		{
			enemyController = pooler.Enemies
				.Where(x => x != null)
				.Where(x => Vector3.Distance(x.transform.position, position) < range)
				.OrderBy(x => Vector3.Distance(x.transform.position, position))
				.FirstOrDefault();
			return enemyController != null;
		}

		#endregion
	}
}
