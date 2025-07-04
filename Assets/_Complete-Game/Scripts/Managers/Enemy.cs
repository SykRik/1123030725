using System;
using UnityEngine;

namespace CompleteProject
{
	public class Enemy : MonoBehaviour
	{
		public event Action OnDeath;

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