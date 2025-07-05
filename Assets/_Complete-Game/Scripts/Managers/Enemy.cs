using System;
using UnityEngine;

namespace CompleteProject
{
	public interface IObjectID
	{
		int ID { get; }
	}

	public class Enemy : MonoBehaviour, IObjectID
	{
		public static int Count = 0;
		
		public int ID { get; private set; }

		public event Action OnDeath;

		private void Awake()
		{
			ID = ++Count;
		}

		public void ResetState()
		{
			// Reset HP, logic v.v.
		}

		public void Die()
		{
			OnDeath?.Invoke();
			gameObject.SetActive(false);
		}
	}
}