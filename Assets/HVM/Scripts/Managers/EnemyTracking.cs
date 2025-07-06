using System.Collections.Generic;
using UnityEngine;

namespace HVM
{
	public class EnemyTracking : MonoBehaviour
	{
		private readonly HashSet<EnemyController> activeEnemies = new();

		public bool AllEnemiesDefeated => activeEnemies.Count == 0;

		public void RegisterEnemy(EnemyController enemyController)
		{
			if (enemyController == null)
				return;
			if (activeEnemies.Contains(enemyController))
				return;

			activeEnemies.Add(enemyController);

			if (enemyController.TryGetComponent(out EnemyController enemyComp))
			{
				enemyComp.OnDeath += () => UnregisterEnemy(enemyController);
			}
		}

		private void UnregisterEnemy(EnemyController enemyController)
		{
			activeEnemies.Remove(enemyController);
		}
	}
}