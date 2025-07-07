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
        public int CurrentHealth => currentHealth;

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

        private int currentHealth;
        private float attackTimer;
        private bool isDead;
        private bool isSinking;
        private bool playerInRange;
        private bool isKnockedBack;
        private float knockbackTimer;

        private Animator animator;
        private AudioSource audioSource;
        private CapsuleCollider capsuleCollider;
        private Rigidbody rb;
        private NavMeshAgent agent;
        private ParticleSystem hitParticles;

        private Transform player;
        private PlayerController playerController;

        private void Awake()
        {
            ID = ++Count;

            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
            capsuleCollider = GetComponent<CapsuleCollider>();
            rb = GetComponent<Rigidbody>();
            agent = GetComponent<NavMeshAgent>();
            hitParticles = GetComponentInChildren<ParticleSystem>();

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
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
                    rb.velocity = Vector3.zero;
                    agent.enabled = true;
                }
                return;
            }

            if (playerController != null && playerController.CurrentHealth > 0)
            {
                if (!agent.enabled) agent.enabled = true;
                agent.SetDestination(player.position);
            }
            else
            {
                if (agent.enabled) agent.enabled = false;
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

        private void Attack()
        {
            attackTimer = 0f;
            animator.SetTrigger("Attack");

            if (playerController.CurrentHealth > 0)
            {
                playerController.TakeDamage(attackDamage);
            }
        }

        public void TakeDamage(int amount, Vector3 sourcePosition, float knockbackForce)
        {
            if (isDead) return;

            audioSource.Play();
            currentHealth -= amount;

            if (hitParticles != null)
                hitParticles.Play();

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
            rb.velocity = Vector3.zero;

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

        public void StartSinking()
        {
            isSinking = true;
            rb.isKinematic = true;
            agent.enabled = false;

            ScoreManager.score += scoreValue;
            Destroy(gameObject, 2f);
        }

        public void ResetState()
        {
            isDead = false;
            isSinking = false;
            isKnockedBack = false;
            knockbackTimer = 0f;
            currentHealth = startingHealth;
            attackTimer = 0f;
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

#if UNITY_EDITOR
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void ResetCounter() => Count = 0;
#endif
    }
}
