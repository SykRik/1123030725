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
    [RequireComponent(typeof(Rigidbody))]
    public class EnemyController : MonoBehaviour, IObjectID
    {
        public static int Count = 0;
        public int ID { get; private set; }

        public event Action OnDeath;

        [Header("Health")]
        [SerializeField] private int startingHealth = 100;
        [SerializeField] private float sinkSpeed = 2.5f;
        [SerializeField] private int scoreValue = 10;
        [SerializeField] private AudioClip deathClip;

        [Header("Attack")]
        [SerializeField] private float timeBetweenAttacks = 0.5f;
        [SerializeField] private int attackDamage = 10;

        private int currentHealth;
        private float attackTimer;
        private bool isDead;
        private bool isSinking;
        private bool playerInRange;

        private Animator animator;
        private AudioSource audioSource;
        private ParticleSystem hitParticles;
        private CapsuleCollider capsuleCollider;
        private Rigidbody rb;
        private NavMeshAgent agent;

        private Transform player;
        private PlayerHealth playerHealth;

        private void Awake()
        {
            ID = ++Count;

            // Setup refs
            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
            hitParticles = GetComponentInChildren<ParticleSystem>();
            capsuleCollider = GetComponent<CapsuleCollider>();
            rb = GetComponent<Rigidbody>();
            agent = GetComponent<NavMeshAgent>();

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj)
            {
                player = playerObj.transform;
                playerHealth = playerObj.GetComponent<PlayerHealth>();
            }

            currentHealth = startingHealth;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
                playerInRange = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
                playerInRange = false;
        }

        private void Update()
        {
            // === Movement ===
            if (!isDead && playerHealth != null && playerHealth.CurrentHealth > 0)
            {
                if (!agent.enabled) agent.enabled = true;
                agent.SetDestination(player.position);
            }
            else
            {
                if (agent.enabled) agent.enabled = false;
            }

            // === Attack ===
            attackTimer += Time.deltaTime;

            if (attackTimer >= timeBetweenAttacks && playerInRange && currentHealth > 0)
            {
                Attack();
            }

            if (playerHealth != null && playerHealth.CurrentHealth <= 0)
            {
                animator.SetTrigger("PlayerDead");
            }

            // === Sinking ===
            if (isSinking)
            {
                transform.Translate(-Vector3.up * sinkSpeed * Time.deltaTime);
            }
        }

        private void Attack()
        {
            attackTimer = 0f;

            if (playerHealth.CurrentHealth > 0)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }

        public void TakeDamage(int amount, Vector3 hitPoint)
        {
            if (isDead) return;

            audioSource.Play();

            currentHealth -= amount;

            if (hitParticles != null)
            {
                hitParticles.transform.position = hitPoint;
                hitParticles.Play();
            }

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            isDead = true;
            capsuleCollider.isTrigger = true;

            animator.SetTrigger("Dead");

            if (deathClip != null)
            {
                audioSource.clip = deathClip;
                audioSource.Play();
            }

            OnDeath?.Invoke();
        }

        public void StartSinking()
        {
            agent.enabled = false;
            rb.isKinematic = true;
            isSinking = true;

            ScoreManager.score += scoreValue;

            Destroy(gameObject, 2f);
        }

        public void ResetState()
        {
            isDead = false;
            isSinking = false;
            currentHealth = startingHealth;
            attackTimer = 0f;
            playerInRange = false;

            gameObject.SetActive(true);
            capsuleCollider.isTrigger = false;
            rb.isKinematic = false;
            animator.ResetTrigger("Dead");
            animator.ResetTrigger("PlayerDead");
            animator.Play("Idle"); // Hoặc tên animation default
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void ResetCounter()
        {
            Count = 0;
        }
#endif
    }
}
