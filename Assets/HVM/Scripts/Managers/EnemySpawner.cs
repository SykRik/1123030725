using UnityEngine;

namespace HVM
{
	public class EnemySpawner : MonoBehaviour
	{
		[SerializeField] private Transform[] spawnPoints;

		public void Spawn(EnemyController enemyController)
		{
			var index = Random.Range(0, spawnPoints.Length);
			var point = spawnPoints[index];

			enemyController.transform.SetPositionAndRotation(point.position, point.rotation);

			if (enemyController.TryGetComponent(out EnemyController enemyComp))
			{
				enemyComp.ResetState();
			}
		}
	}
}