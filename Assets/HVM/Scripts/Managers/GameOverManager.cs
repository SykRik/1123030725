using UnityEngine;

namespace HVM
{
	public class GameOverManager : MonoBehaviour
	{
		public PlayerController playerController = null; // Reference to the player's health.


		private Animator anim; // Reference to the animator component.


		void Awake()
		{
			// Set up the reference.
			anim = GetComponent<Animator>();
		}


		void Update()
		{
			// If the player has run out of health...
			if (playerController.CurrentHealth <= 0)
			{
				// ... tell the animator the game is over.
				anim.SetTrigger("GameOver");
			}
		}
	}
}