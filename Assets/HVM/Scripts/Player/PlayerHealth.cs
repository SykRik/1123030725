using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace HVM
{
	public class PlayerHealth : MonoBehaviour
	{
		#region ===== Serialized Fields =====

		[SerializeField] private int       startingHealth = 100;
		[SerializeField] private Slider    healthSlider;
		[SerializeField] private Image     damageImage;
		[SerializeField] private AudioClip deathClip;
		[SerializeField] private float     flashSpeed = 5f;
		[SerializeField] private Color     flashColor = new Color(1f, 0f, 0f, 0.1f);

		#endregion

		#region ===== Private Fields =====

		private int  currentHealth = 0;
		private bool isDead    = false;
		private bool isDamaged = false;

		private Animator       animator;
		private AudioSource    audioSource;
		private PlayerMovement movement;
		private PlayerShooting shooting;

		#endregion

		#region ===== Properties =====

		public int CurrentHealth => currentHealth;

		#endregion

		#region ===== Unity Methods =====

		private void Awake()
		{
			animator    = GetComponent<Animator>();
			audioSource = GetComponent<AudioSource>();
			movement    = GetComponent<PlayerMovement>();
			shooting    = GetComponentInChildren<PlayerShooting>();

			currentHealth = startingHealth;
		}

		private void Update()
		{
			UpdateDamageImage();
			isDamaged = false;
		}

		#endregion

		#region ===== Public Methods =====

		public void TakeDamage(int damage)
		{
			isDamaged          =  true;
			currentHealth      -= damage;
			healthSlider.value =  CurrentHealth;

			audioSource.Play();

			if (currentHealth <= 0 && !isDead)
				Die();
		}

		public void RestartLevel()
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		}

		#endregion

		#region ===== Private Methods =====

		private void UpdateDamageImage()
		{
			damageImage.color = isDamaged
									? flashColor
									: Color.Lerp(damageImage.color, Color.clear, flashSpeed * Time.deltaTime);
		}

		private void Die()
		{
			isDead = true;

			shooting.DisableEffects();
			animator.SetTrigger("Die");

			audioSource.clip = deathClip;
			audioSource.Play();

			movement.enabled = false;
			shooting.enabled = false;
		}

		#endregion
	}
}