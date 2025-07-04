using System.Collections.Generic;
using UnityEngine;

namespace CompleteProject
{
	public class EnemyTracking : MonoBehaviour
	{
		private readonly HashSet<Enemy> activeEnemies = new();

		public bool AllEnemiesDefeated => activeEnemies.Count == 0;

		public void RegisterEnemy(Enemy enemy)
		{
			if (enemy == null)
				return;
			if (activeEnemies.Contains(enemy))
				return;

			activeEnemies.Add(enemy);

			if (enemy.TryGetComponent(out Enemy enemyComp))
			{
				enemyComp.OnDeath += () => UnregisterEnemy(enemy);
			}
		}

		private void UnregisterEnemy(Enemy enemy)
		{
			activeEnemies.Remove(enemy);
		}
	}
}