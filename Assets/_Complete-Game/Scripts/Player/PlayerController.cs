using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace HVM
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour, IObjectID
    {
        public static int Count = 0;
        public int ID { get; private set; }

        [Header("Health")]
        [SerializeField] private int startingHealth = 100;
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Image damageImage;
        [SerializeField] private AudioClip deathClip;
        [SerializeField] private float flashSpeed = 5f;
        [SerializeField] private Color flashColor = new Color(1f, 0f, 0f, 0.1f);

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float targetRange = 10f;

        [Header("Shooting")]
        [SerializeField] private int damagePerShot = 20;
        [SerializeField] private float timeBetweenBullets = 0.15f;
        [SerializeField] private float shotRange = 100f;

        [Header("Shooting FX")]
        [SerializeField] private GameObject shootingEffectObject;
        [SerializeField] private ParticleSystem gunParticles;
        [SerializeField] private LineRenderer gunLine;
        [SerializeField] private AudioSource gunAudio;
        [SerializeField] private Light gunLight;
        [SerializeField] private Light faceLight;

        private int currentHealth;
        private bool isDead;
        private bool isDamaged;
        private float lastShotTime;

        private Vector2 moveInput;
        private Ray shootRay;
        private int shootableMask;
        private Coroutine disableEffectRoutine;

        private Animator animator;
        private Rigidbody playerRB;

        public int CurrentHealth => currentHealth;

        private void Awake()
        {
            ID = ++Count;

            animator = GetComponent<Animator>();
            playerRB = GetComponent<Rigidbody>();
            shootableMask = LayerMask.GetMask("Shootable");

            if (shootingEffectObject == null)
            {
                Debug.LogError("[PlayerController] Missing reference: shootingEffectObject", this);
            }
            else
            {
                gunParticles = shootingEffectObject.GetComponent<ParticleSystem>();
                gunLine     = shootingEffectObject.GetComponent<LineRenderer>();
                gunAudio    = shootingEffectObject.GetComponent<AudioSource>();
                gunLight    = shootingEffectObject.GetComponent<Light>();
                faceLight   = shootingEffectObject.GetComponentInChildren<Light>();

                if (gunParticles == null) Debug.LogError("[PlayerController] Missing ParticleSystem on shootingEffectObject", shootingEffectObject);
                if (gunLine == null) Debug.LogError("[PlayerController] Missing LineRenderer on shootingEffectObject", shootingEffectObject);
                if (gunAudio == null) Debug.LogError("[PlayerController] Missing AudioSource on shootingEffectObject", shootingEffectObject);
                if (gunLight == null) Debug.LogError("[PlayerController] Missing Light on shootingEffectObject", shootingEffectObject);
                if (faceLight == null) Debug.LogError("[PlayerController] Missing child Light in shootingEffectObject", shootingEffectObject);
            }

            currentHealth = startingHealth;
        }

        private void Update()
        {
            HandleDamageFlash();
            HandleShooting();
        }

        private void FixedUpdate()
        {
            FindTarget();
            HandleMovement();
            HandleRotation();
            UpdateWalkAnimation();
        }

        private EnemyController targetEnemy = null;

        private void FindTarget()
        {
            targetEnemy = EnemyManager.Instance.TryGetClosedEnemy(transform.position, targetRange, out var enemy) ? enemy : null;
        }


        public void TakeDamage(int damage)
        {
            isDamaged = true;
            currentHealth -= damage;
            if (healthSlider != null) healthSlider.value = currentHealth;

            gunAudio?.Play();

            if (currentHealth <= 0 && !isDead)
                Die();
        }

        private void Die()
        {
            isDead = true;
            DisableEffects();
            animator.SetTrigger("Die");

            if (deathClip != null)
            {
                gunAudio.clip = deathClip;
                gunAudio.Play();
            }

            enabled = false;
        }

        private void HandleDamageFlash()
        {
            if (damageImage != null)
            {
                damageImage.color = isDamaged
                    ? flashColor
                    : Color.Lerp(damageImage.color, Color.clear, flashSpeed * Time.deltaTime);
            }

            isDamaged = false;
        }

        public void RestartLevel()
        {
            int index = SceneManager.GetActiveScene().buildIndex;
            if (index < 0)
            {
                Debug.LogError("[PlayerController] Invalid scene build index.");
                return;
            }

            SceneManager.LoadScene(index);
        }

        private void HandleMovement()
        {
            if (InputManager.Instance == null)
            {
                Debug.LogError("[PlayerController] Missing InputManager.Instance");
                return;
            }

            moveInput = InputManager.Instance.MoveInput;
            var dir = moveInput.ToVector3XZ().normalized;
            var newPos = transform.position + dir * moveSpeed * Time.fixedDeltaTime;
            playerRB.MovePosition(newPos);
        }

        [SerializeField] private float rotateSpeed = 720f; // degrees per second

        private void HandleRotation()
        {
            var direction = targetEnemy == null
                ? moveInput.ToVector3XZ()
                : (targetEnemy.transform.position - transform.position).ToVector3XZ();

            if (direction.sqrMagnitude > Mathf.Epsilon)
            {
                var originRot = playerRB.rotation;
                var targetRot = Quaternion.LookRotation(direction);
                var smoothRot = Quaternion.RotateTowards(originRot, targetRot, rotateSpeed * Time.fixedDeltaTime);

                playerRB.MoveRotation(smoothRot);
            }
        }

        private void UpdateWalkAnimation()
        {
            animator.SetBool("IsWalking", moveInput.sqrMagnitude > 0.01f);
        }

        private void HandleShooting()
        {
            if (Time.timeScale <= 0f)
                return;
            if (Time.time < lastShotTime + timeBetweenBullets)
                return;
            lastShotTime = Time.time;
            PerformRaycast();
            PlayMuzzleEffects();
            DisableEffects(timeBetweenBullets * 0.2f);
        }

        private void PlayMuzzleEffects()
        {
            gunAudio?.Play();
            if (gunLight != null) gunLight.enabled = true;
            if (faceLight != null) faceLight.enabled = true;
            gunParticles?.Stop();
            gunParticles?.Play();

            if (gunLine != null)
            {
                gunLine.enabled = true;
                gunLine.SetPosition(0, shootingEffectObject.transform.position);
            }
        }

        private void PerformRaycast()
        {
            // var hitPoint = targetEnemy == null
            //     ? transform.position + transform.forward * shotRange
            //     : targetEnemy.transform.position;
            //
            // if (gunLine != null)
            //     gunLine.SetPosition(1, hitPoint + Vector3.up * 0.2f);
            
            
            shootRay.origin = shootingEffectObject.transform.position;
            shootRay.direction = transform.forward;

            if (Physics.Raycast(shootRay, out var hitInfo, shotRange, shootableMask))
            {
                TryDealDamage(hitInfo);
                SetLineEnd(hitInfo.point);
            }
            else
            {
                SetLineEnd(shootRay.origin + shootRay.direction * shotRange);
            }
        }

        private void SetLineEnd(Vector3 endPoint)
        {
            if (gunLine != null)
                gunLine.SetPosition(1, endPoint);
        }

        private void TryDealDamage(RaycastHit hit)
        {
            if (hit.collider.TryGetComponent<EnemyController>(out var enemy))
            {
                enemy.TakeDamage(damagePerShot, hit.point);
            }
            else
            {
                Debug.LogWarning("");
            }
        }

        private void DisableEffects(float delay)
        {
            RestartCoroutine(ref disableEffectRoutine, DisableEffectsAsync(delay));
        }

        public void DisableEffects()
        {
            if (gunLine != null) gunLine.enabled = false;
            if (gunLight != null) gunLight.enabled = false;
            if (faceLight != null) faceLight.enabled = false;
        }

        private IEnumerator DisableEffectsAsync(float delay)
        {
            yield return new WaitForSeconds(delay);
            DisableEffects();
        }

        private void RestartCoroutine(ref Coroutine routine, IEnumerator coroutine)
        {
            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(coroutine);
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void ResetIDCounter() => Count = 0;
#endif
    }
}
