using System.Collections.Generic;
using UnityEngine;

namespace CompleteProject
{
	public class BasePooler<T> : MonoBehaviour where T : Component
	{
		[SerializeField] protected T         prefab;
		[SerializeField] protected int       initialSize = 10;
		[SerializeField] protected Transform freezingContainer;
		[SerializeField] protected Transform runningContainer;

		private readonly Queue<T> pool = new();

		private void Awake()
		{
			InitializePool();
		}

		public bool TryGetEnemy(out T enemy)
		{
			if (pool.Count > 0 || IncreaseEnemies(10))
			{
				enemy = pool.Dequeue();
				enemy.transform.SetParent(runningContainer, false);
			}
			else
			{
				enemy = null;
				Debug.LogWarning("[EnemyPooler] Failed to expand pool.");
			}

			return enemy != null;
		}

		public T GetEnemy()
		{
			return TryGetEnemy(out var enemy) ? enemy : null;
		}

		public void ReturnEnemy(T enemy)
		{
			if (enemy == null)
				return;

			enemy.transform.SetParent(freezingContainer, false);
			pool.Enqueue(enemy);
		}

		private void InitializePool()
		{
			IncreaseEnemies(initialSize);
		}

		private bool IncreaseEnemies(int amount)
		{
			if (amount <= 0)
			{
				Debug.LogWarning("[EnemyPooler] Requested amount must be greater than 0.");
				amount = 1;
			}

			while (amount-- > 0)
			{
				var enemy = Instantiate(prefab, freezingContainer);

				enemy.transform.SetParent(freezingContainer, false);
				pool.Enqueue(enemy);
			}

			return pool.Count > 0;
		}

		public void ForceReset()
		{
			var enemies = runningContainer.GetComponentsInChildren<T>();
			foreach (var enemy in enemies)
			{
				enemy.transform.SetParent(freezingContainer, false);
				pool.Enqueue(enemy);
			}
		}
	}
}