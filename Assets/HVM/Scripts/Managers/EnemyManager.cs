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

        [SerializeField] private PlayerController playerController = null;
        [SerializeField] private float spawnInterval = 1.2f;
        [SerializeField] private List<EnemyPooler> poolers = null;
        [SerializeField] private EnemySpawner spawner = null;
        [SerializeField] private EnemyTracking tracker = null;

        #endregion

        #region ===== Runtime Fields =====

        private float spawnTimer = 0f;
        private bool isRunning = false;
        private Queue<TypeOfEnemy> spawnQueue = new();

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

        protected override void Awake()
        {
            base.Awake();

            poolers ??= new();
            poolers = poolers.OrderByDescending(pooler => pooler.Prefab.Type).ToList();
        }

        private void Update()
        {
            if (isRunning && playerController.CurrentHealth > 0f)
            {
                spawnTimer -= Time.deltaTime;

                if (spawnTimer > 0f) 
                    return;
                
                spawnTimer = spawnInterval;

                if (spawnQueue.Count > 0)
                {
                    SpawnEnemy(spawnQueue.Dequeue());
                }
            }
        }

        #endregion

        #region ===== Public Methods =====

        public void StartSpawning(LevelConfig config)
        {
            isRunning = true;
            spawnTimer = spawnInterval;
            EnqueueEnemies(config);
            Debug.Log("[EnemyManager] Spawning started.");
        }

        public void StopSpawning()
        {
            isRunning = false;
            Debug.Log("[EnemyManager] Spawning stopped.");
        }

        public bool TryGetClosedEnemy(Vector3 position, float range, out EnemyController enemyController)
        {
            foreach (var pooler in poolers)
            {
                if (TryGetEnemyClosed(pooler, position, range, out enemyController))
                    return true;
            }

            enemyController = null;
            return false;
        }

        public void RegisterEnemyDeath()
        {
            TotalEnemiesKilled++;
        }

        public void ResetKillCount()
        {
            TotalEnemiesKilled = 0;
        }

        public void ReturnEnemyToPool(EnemyController enemy)
        {
            if (enemy == null) return;

            foreach (var pooler in poolers)
            {
                if (pooler.Prefab.Type == enemy.Type)
                {
                    pooler.Return(enemy);
                    return;
                }
            }

            Debug.LogWarning($"[EnemyManager] Unknown enemy type to return: {enemy.name}");
        }

        public void ReturnAllAliveEnemiesToPool()
        {
            foreach (var pooler in poolers)
            {
                ReturnAliveEnemiesInPool(pooler);
            }
        }

        #endregion

        #region ===== Private Methods =====

        private void SpawnEnemy(TypeOfEnemy type)
        {
            foreach (var pooler in poolers)
            {
                if (pooler.Prefab.Type == type && pooler.TryRequest(out var enemy))
                {
                    spawner.Spawn(enemy);
                    tracker.RegisterEnemy(enemy);
                    Debug.Log($"[EnemyManager] Spawned enemy of type: {type}");
                    return;
                }
            }

            Debug.LogWarning($"[EnemyManager] Failed to get enemy from pool for type: {type}");
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

        private bool TryGetEnemyClosed(EnemyPooler pooler, Vector3 position, float range, out EnemyController enemyController)
        {
            enemyController = pooler.Enemies
                .Where(x => x != null && x.CurrentHealth > 0 && Vector3.Distance(x.transform.position, position) < range)
                .OrderBy(x => Vector3.Distance(x.transform.position, position))
                .FirstOrDefault();

            return enemyController != null;
        }

        private void EnqueueEnemies(LevelConfig config)
        {
            spawnQueue.Clear();
            if(config == null)
                return;

            for (var i = 0; i < config.A; i++) spawnQueue.Enqueue(TypeOfEnemy.A);
            for (var i = 0; i < config.B; i++) spawnQueue.Enqueue(TypeOfEnemy.B);
            for (var i = 0; i < config.C; i++) spawnQueue.Enqueue(TypeOfEnemy.C);
        }

        #endregion
    }
}
