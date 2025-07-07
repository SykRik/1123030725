using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HVM
{
    [RequireComponent(typeof(EnemySpawner))]
    [RequireComponent(typeof(EnemyTracking))]
    public class EnemyManager : MonoSingleton<EnemyManager>
    {
        #region ===== Enum =====

        public enum TypeOfEnemy
        {
            A,
            B,
            C,
            Undefine
        }

        #endregion

        #region ===== Serialized Fields =====

        [Header("Game References")]
        [SerializeField] private PlayerController playerController;

        [Header("Spawning Settings")]
        [SerializeField] private float spawnInterval = 3f;

        [Header("Enemy Poolers (By Type)")]
        [SerializeField] private EnemyPooler poolerA;
        [SerializeField] private EnemyPooler poolerB;
        [SerializeField] private EnemyPooler poolerC;

        [Header("Internal References")]
        [SerializeField] private EnemySpawner spawner;
        [SerializeField] private EnemyTracking tracker;

        #endregion

        #region ===== Runtime Fields =====

        private float spawnTimer = 0f;
        private bool isRunning = false;

        #endregion

        #region ===== Properties =====

        public int TotalEnemiesKilled { get; private set; }

        #endregion

        #region ===== Unity Methods =====

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
            if (!isRunning || playerController.CurrentHealth <= 0f)
                return;

            spawnTimer -= Time.deltaTime;

            if (spawnTimer <= 0f)
            {
                spawnTimer = spawnInterval;
                SpawnEnemy(TypeOfEnemy.A); // 🔁 Future: support more types
            }
        }

        #endregion

        #region ===== Public Methods =====

        public void StartSpawning()
        {
            isRunning = true;
            spawnTimer = spawnInterval;
            Debug.Log("[EnemyManager] Spawning started.");
        }

        public void StopSpawning()
        {
            isRunning = false;
            Debug.Log("[EnemyManager] Spawning stopped.");
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

        public void RegisterEnemyDeath()
        {
            TotalEnemiesKilled++;
        }

        #endregion

        #region ===== Private Methods =====

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

            enemy.Type = type;
            spawner.Spawn(enemy);
            tracker.RegisterEnemy(enemy);

            Debug.Log($"[EnemyManager] Spawned enemy of type: {type}");
        }
        
        public void ReturnEnemyToPool(EnemyController enemy)
        {
            if (enemy == null) return;

            switch (enemy.Type)
            {
                case TypeOfEnemy.A:
                    poolerA.Return(enemy);
                    break;
                case TypeOfEnemy.B:
                    poolerB.Return(enemy);
                    break;
                case TypeOfEnemy.C:
                    poolerC.Return(enemy);
                    break;
                default:
                    Debug.LogWarning($"[EnemyManager] Unknown enemy type to return: {enemy.name}");
                    break;
            }
        }


        private EnemyPooler GetPooler(TypeOfEnemy type)
        {
            return type switch
            {
                TypeOfEnemy.A => poolerA,
                TypeOfEnemy.B => poolerB,
                TypeOfEnemy.C => poolerC,
                _ => null,
            };
        }

        private bool TryGetEnemyClosed(EnemyPooler pooler, Vector3 position, float range, out EnemyController enemyController)
        {
            enemyController = pooler.Enemies
                .Where(x => x != null && x.CurrentHealth > 0 && Vector3.Distance(x.transform.position, position) < range)
                .OrderBy(x => Vector3.Distance(x.transform.position, position))
                .FirstOrDefault();

            return enemyController != null;
        }
        
        public void ReturnAllAliveEnemiesToPool()
        {
            ReturnAliveEnemiesInPool(poolerA);
            ReturnAliveEnemiesInPool(poolerB);
            ReturnAliveEnemiesInPool(poolerC);
        }

        private void ReturnAliveEnemiesInPool(EnemyPooler pooler)
        {
            var queueEnemy = new Queue<EnemyController>(pooler.Enemies);
            while (queueEnemy.Count > 0)
            {
                var enemy = queueEnemy.Dequeue();
                if (enemy != null && enemy.CurrentHealth > 0)
                {
                    ReturnEnemyToPool(enemy);
                }
            }
        }

        #endregion
    }
}
