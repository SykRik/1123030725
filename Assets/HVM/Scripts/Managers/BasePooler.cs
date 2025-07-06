using System.Collections.Generic;
using UnityEngine;

namespace HVM
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
            Initialize();
        }

        public bool TryRequest(out T item)
        {
            if (idleItems.Count > 0 || Expand(10))
            {
                item = idleItems.Dequeue();
                item.transform.SetParent(liveContainer, false);
                OnRequest(item);
                liveItems.Add(item);
            }
            else
            {
                item = null;
                Debug.LogWarning("[Pooler] Failed to expand pool.");
            }

            return item != null;
        }

        public T Request()
        {
            return TryRequest(out var item) ? item : null;
        }

        public void Return(T item)
        {
            if (item == null || !liveItems.Contains(item))
                return;

            OnReturn(item);
            item.transform.SetParent(idleContainer, false);
            idleItems.Enqueue(item);
            liveItems.Remove(item);
        }

        public void ForceReset()
        {
            foreach (var item in liveItems)
            {
                OnReturn(item);
                item.transform.SetParent(idleContainer, false);
                idleItems.Enqueue(item);
            }

            liveItems.Clear();
        }

        protected virtual void OnRequest(T item) { }
        protected virtual void OnReturn(T item) { }

        private void Initialize()
        {
            Expand(initialSize);
        }

        private bool Expand(int amount)
        {
            for (var i = 0; i < amount; i++)
            {
                var item = Instantiate(prefab, idleContainer);
                item.transform.SetParent(idleContainer, false);
                item.gameObject.SetActive(false);
                idleItems.Enqueue(item);
            }

            return idleItems.Count > 0;
        }
    }
}