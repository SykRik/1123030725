using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace HVM
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(ParticleSystem))]
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
        [SerializeField] private Transform hitPoint = null;

        [Header("Knockback")]
        [SerializeField] private float knockbackDuration = 0.5f;

        [Header("Random Force")]
        [SerializeField] private float randomForceMagnitude = 10f;
        [SerializeField] private float randomForceInterval = 1f;

        private int currentHealth;
        private float attackTimer;
        private bool isDead;
        private bool isSinking;
        private bool playerInRange;
        private bool isKnockedBack;
        private float knockbackTimer;

        private Animator animator;
        private AudioSource audioSource;
        [SerializeField]
        private ParticleSystem hitParticles;
        private CapsuleCollider capsuleCollider;
        private Rigidbody rb;

        private Transform player;
        private PlayerController playerController;

        public int CurrentHealth => currentHealth;

        private void Awake()
        {
            ID = ++Count;

            // Setup refs
            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
            hitParticles = GetComponentInChildren<ParticleSystem>();
            capsuleCollider = GetComponent<CapsuleCollider>();
            rb = GetComponent<Rigidbody>();

            // Remove NavMeshAgent component
            var navMeshAgent = GetComponent<NavMeshAgent>();
            if (navMeshAgent != null)
            {
                Destroy(navMeshAgent);
            }

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj)
            {
                player = playerObj.transform;
                playerController = playerObj.GetComponent<PlayerController>();
            }

            currentHealth = startingHealth;

            // Start random force coroutine
            StartCoroutine(ApplyRandomForceCoroutine());
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
            // === Knockback Handling ===
            if (isKnockedBack)
            {
                knockbackTimer -= Time.deltaTime;
                if (knockbackTimer <= 0)
                {
                    isKnockedBack = false;
                    rb.linearVelocity = Vector3.zero; // Stop Rigidbody movement
                    rb.angularVelocity = Vector3.zero; // Stop Rigidbody rotation
                }
            }

            // === Attack ===
            attackTimer += Time.deltaTime;

            if (attackTimer >= timeBetweenAttacks && playerInRange && currentHealth > 0 && !isKnockedBack)
            {
                Attack();
            }

            if (playerController != null && playerController.CurrentHealth <= 0)
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

            if (playerController.CurrentHealth > 0)
            {
                playerController.TakeDamage(attackDamage);
            }
        }

        public void TakeDamage(int amount, Vector3 hitPoint, float knockbackForce)
        {
            if (isDead) return;

            audioSource.Play();

            currentHealth -= amount;

            if (hitParticles != null)
            {
                hitParticles.Play();
            }

            // Use a default knockbackForce if the provided value is too low
            float effectiveKnockbackForce = knockbackForce > 0 ? knockbackForce : 20f;

            // === Apply Knockback ===
            StartCoroutine(ApplyKnockbackCoroutine(hitPoint, effectiveKnockbackForce));

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private IEnumerator ApplyKnockbackCoroutine(Vector3 hitPoint, float knockbackForce)
        {
            if (isKnockedBack) yield break;

            isKnockedBack = true;
            knockbackTimer = knockbackDuration;

            // Ensure Rigidbody is not kinematic and not constrained
            if (rb.isKinematic)
            {
                Debug.LogWarning($"Enemy {gameObject.name} has isKinematic = true, setting to false for knockback.");
                rb.isKinematic = false;
            }
            if (rb.constraints != RigidbodyConstraints.None)
            {
                Debug.LogWarning($"Enemy {gameObject.name} has Rigidbody constraints, removing for knockback.");
                rb.constraints = RigidbodyConstraints.None;
            }

            rb.linearVelocity = Vector3.zero; // Stop existing movement before knockback
            rb.angularVelocity = Vector3.zero; // Stop existing rotation

            // Wait for one frame to ensure consistent physics application
            yield return null;

            Vector3 knockbackDirection = (transform.position - hitPoint).normalized;
            knockbackDirection.y = 0; // Keep knockback horizontal

            // Debug draw line to visualize knockback direction
            Vector3 debugEndPoint = transform.position + knockbackDirection * (knockbackForce / 10f); // Scale for visibility
            Debug.DrawLine(transform.position, debugEndPoint, Color.red, 2f);

            rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
        }

        private IEnumerator ApplyRandomForceCoroutine()
        {
            while (true)
            {
                if (!isDead && !isSinking && !isKnockedBack)
                {
                    // Ensure Rigidbody is not kinematic and not constrained
                    if (rb.isKinematic)
                    {
                        Debug.LogWarning($"Enemy {gameObject.name} has isKinematic = true, setting to false for random force.");
                        rb.isKinematic = false;
                    }
                    if (rb.constraints != RigidbodyConstraints.None)
                    {
                        Debug.LogWarning($"Enemy {gameObject.name} has Rigidbody constraints, removing for random force.");
                        rb.constraints = RigidbodyConstraints.None;
                    }

                    // Generate random direction with upward component
                    Vector3 randomDirection = new Vector3(
                        Random.Range(-5f, 5f),
                        5f, // Upward component (Y positive)
                        Random.Range(-5f, 5f)
                    ).normalized;

                    // Debug draw line to visualize random force direction
                    Vector3 debugEndPoint = transform.position + randomDirection * (randomForceMagnitude / 10f); // Scale for visibility
                    Debug.DrawLine(transform.position, debugEndPoint, Color.blue, 2f);

                    // Apply random force
                    rb.linearVelocity = Vector3.zero; // Stop existing movement
                    rb.angularVelocity = Vector3.zero; // Stop existing rotation
                    rb.AddForce(randomDirection * randomForceMagnitude, ForceMode.Impulse);
                }

                // Wait for the specified interval
                yield return new WaitForSeconds(randomForceInterval);
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
            EnemyManager.Instance.RegisterEnemyDeath();
        }

        public void StartSinking()
        {
            rb.isKinematic = true;
            isSinking = true;

            ScoreManager.score += scoreValue;

            Destroy(gameObject, 2f);
        }

        public void ResetState()
        {
            isDead = false;
            isSinking = false;
            isKnockedBack = false;
            currentHealth = startingHealth;
            attackTimer = 0f;
            playerInRange = false;

            gameObject.SetActive(true);
            capsuleCollider.isTrigger = false;
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.None; // Ensure no constraints
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
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