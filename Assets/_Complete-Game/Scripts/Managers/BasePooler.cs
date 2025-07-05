using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using List = NUnit.Framework.List;

namespace CompleteProject
{
    public class BasePooler<T> : MonoBehaviour where T : Component, IObjectID
    {
        [SerializeField] protected T prefab = null;
        [SerializeField] protected int initialSize = 10;
        [SerializeField] protected Transform idleContainer = null;
        [SerializeField] protected Transform liveContainer = null;

        protected readonly Queue<T> idleItems = new();
        protected readonly List<T> liveItems = new();

        private void Awake()
        {
            InitializePool();
        }

        public bool TryGetEnemy(out T enemy)
        {
            if (idleItems.Count > 0 || IncreaseEnemies(10))
            {
                enemy = idleItems.Dequeue();
                enemy.transform.SetParent(liveContainer, false);
                liveItems.Add(enemy);
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
            if (liveItems.Contains(enemy))
                return;

            enemy.transform.SetParent(idleContainer, false);
            idleItems.Enqueue(enemy);
            liveItems.Remove(enemy);
        }

        private void InitializePool()
        {
            IncreaseEnemies(initialSize);
        }

        private bool IncreaseEnemies(int amount)
        {
            for (var i = 0; i < amount; i++)
            {
                var enemy = Instantiate(prefab, idleContainer);
                enemy.transform.SetParent(idleContainer, false);
                idleItems.Enqueue(enemy);
            }

            return idleItems.Count > 0;
        }

        public void ForceReset()
        {
            foreach (var item in liveItems)
            {
                item.transform.SetParent(idleContainer, false);
                idleItems.Enqueue(item);
            }

            liveItems.Clear();
        }
    }
}