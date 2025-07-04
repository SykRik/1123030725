using UnityEngine;

namespace CompleteProject
{
	public class EnemySpawner : MonoBehaviour
	{
		[SerializeField] private Transform[] spawnPoints;

		public void Spawn(Enemy enemy)
		{
			var index = Random.Range(0, spawnPoints.Length);
			var point = spawnPoints[index];

			enemy.transform.SetPositionAndRotation(point.position, point.rotation);

			if (enemy.TryGetComponent(out Enemy enemyComp))
			{
				enemyComp.ResetState();
			}
		}
	}
}