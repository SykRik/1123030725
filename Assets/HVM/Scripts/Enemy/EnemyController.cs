using System;
using UnityEngine;
using UnityEngine.AI;

namespace HVM
{
	[RequireComponent(typeof(Animator))]
	[RequireComponent(typeof(AudioSource))]
	[RequireComponent(typeof(CapsuleCollider))]
	[RequireComponent(typeof(ParticleSystem))]
	[RequireComponent(typeof(NavMeshAgent))]
	public class EnemyController : MonoBehaviour, IObjectID
	{
		public static int                      Count = 0;
		public        int                      ID            { get; private set; }
		public        int                      CurrentHealth => currentHealth;
		public        EnemyManager.TypeOfEnemy Type          => type;

		public event Action OnDeath;

		#region === Serialized ===
		
		[Header("General")]
		[SerializeField] private EnemyManager.TypeOfEnemy type = EnemyManager.TypeOfEnemy.Undefine;

		[Header("Health")]
		[SerializeField] private int startingHealth = 100;
		[SerializeField] private float sinkSpeed = 2.5f;
		[SerializeField] private int scoreValue = 10;
		[SerializeField] private AudioClip deathClip;

		[Header("Attack")]
		[SerializeField] private float timeBetweenAttacks = 0.5f;
		[SerializeField] private int attackDamage = 10;

		[Header("Knockback")]
		[SerializeField] private float knockbackDuration = 0.4f;
		[SerializeField] private float knockbackResistance = 1f;

		#endregion

		#region === Private Fields ===

		private int currentHealth;
		private float attackTimer;
		private float knockbackTimer;

		private bool isDead;
		private bool isSinking;
		private bool isKnockedBack;
		private bool playerInRange;

		private Animator animator;
		private AudioSource audioSource;
		private CapsuleCollider capsuleCollider;
		private Rigidbody rb;
		private NavMeshAgent agent;
		private ParticleSystem hitParticles;

		private Transform player;
		private PlayerController playerController;

		#endregion

		#region === Unity Methods ===

		private void Awake()
		{
			ID = ++Count;

			animator = GetComponent<Animator>();
			audioSource = GetComponent<AudioSource>();
			capsuleCollider = GetComponent<CapsuleCollider>();
			rb = GetComponent<Rigidbody>();
			agent = GetComponent<NavMeshAgent>();
			hitParticles = GetComponentInChildren<ParticleSystem>();

			var playerObj = GameObject.FindGameObjectWithTag("Player");
			if (playerObj)
			{
				player = playerObj.transform;
				playerController = playerObj.GetComponent<PlayerController>();
			}

			currentHealth = startingHealth;
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag("Player"))
				playerInRange = true;
		}

		private void OnTriggerExit(Collider other)
		{
			if (other.CompareTag("Player"))
				playerInRange = false;
		}

		private void Update()
		{
			if (isDead) return;

			if (isKnockedBack)
			{
				knockbackTimer -= Time.deltaTime;
				if (knockbackTimer <= 0f)
				{
					isKnockedBack = false;
					rb.linearVelocity = Vector3.zero;
					agent.enabled = true;
				}
				return;
			}

			if (playerController?.CurrentHealth > 0)
			{
				if (!agent.enabled) agent.enabled = true;
				agent.SetDestination(player.position);
			}
			else if (agent.enabled)
			{
				agent.enabled = false;
			}

			attackTimer += Time.deltaTime;

			if (attackTimer >= timeBetweenAttacks && playerInRange && playerController.CurrentHealth > 0)
			{
				Attack();
			}

			if (isSinking)
			{
				transform.Translate(-Vector3.up * sinkSpeed * Time.deltaTime);
			}
		}

		#endregion

		#region === Combat ===

		private void Attack()
		{
			attackTimer = 0f;
			animator.SetTrigger("Attack");

			if (playerController != null)
			{
				playerController.TakeDamage(attackDamage);
			}
		}

		public void TakeDamage(int amount, Vector3 sourcePosition, float knockbackForce)
		{
			if (isDead) return;

			currentHealth -= amount;
			audioSource.Play();
			hitParticles?.Play();

			if (currentHealth <= 0)
			{
				Die();
			}
			else
			{
				ApplyKnockback(sourcePosition, knockbackForce);
			}
		}

		private void ApplyKnockback(Vector3 sourcePosition, float force)
		{
			if (isKnockedBack || agent == null) return;

			isKnockedBack = true;
			knockbackTimer = knockbackDuration;

			agent.enabled = false;
			rb.linearVelocity = Vector3.zero;

			Vector3 direction = (transform.position - sourcePosition).normalized;
			direction.y = 0f;

			rb.AddForce(direction * force / knockbackResistance, ForceMode.Impulse);
		}

		private void Die()
		{
			isDead = true;
			capsuleCollider.isTrigger = true;
			agent.enabled = false;
			rb.isKinematic = true;

			animator.SetTrigger("Dead");

			if (deathClip != null)
			{
				audioSource.clip = deathClip;
				audioSource.Play();
			}

			OnDeath?.Invoke();
			EnemyManager.Instance.RegisterEnemyDeath();

			StartSinking();
		}

		private void StartSinking()
		{
			isSinking = true;
			rb.isKinematic = true;
			agent.enabled = false;

			Invoke(nameof(ReturnToPool), 2f);
		}

		private void ReturnToPool()
		{
			EnemyManager.Instance.ReturnEnemyToPool(this);
		}

		#endregion

		#region === Reset ===

		public void ResetState()
		{
			isDead = false;
			isSinking = false;
			isKnockedBack = false;
			knockbackTimer = 0f;
			attackTimer = 0f;
			currentHealth = startingHealth;
			playerInRange = false;

			gameObject.SetActive(true);
			capsuleCollider.isTrigger = false;
			rb.linearVelocity = Vector3.zero;
			rb.isKinematic = false;

			agent.velocity = Vector3.zero;
			agent.enabled = true;

			animator.ResetTrigger("Dead");
			animator.ResetTrigger("PlayerDead");
			animator.Play("Idle");
		}

		#endregion

#if UNITY_EDITOR
		[UnityEditor.InitializeOnEnterPlayMode]
		private static void ResetCounter() => Count = 0;
#endif
	}
}
