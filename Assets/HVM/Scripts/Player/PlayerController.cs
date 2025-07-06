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
        #region ===== Static =====

        public static int Count = 0;

#if UNITY_EDITOR
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void ResetIDCounter() => Count = 0;
#endif

        #endregion

        #region ===== Public Properties =====

        public int ID { get; private set; }
        public int CurrentHealth => currentHealth;

        #endregion

        #region ===== Enums =====

        public enum ShootingMode { Single, Shotgun }

        #endregion

        #region ===== Serialized Fields =====

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
        [SerializeField] private float rotateSpeed = 720f;

        [Header("Shooting")]
        [SerializeField] private int damagePerShot = 20;
        [SerializeField] private float timeBetweenBullets = 0.3f;
        [SerializeField] private float shotRange = 100f;
        [SerializeField] private ShootingMode shootingMode = ShootingMode.Single;
        [SerializeField] private float shotgunAngle = 60f;
        [SerializeField] private float shotgunRadius = 6f;
        [SerializeField] private float knockbackForce = 10f;
        [SerializeField] private int shotgunRayCount = 5;

        [Header("Shooting FX")]
        [SerializeField] private GameObject shootingEffectObject;
        [SerializeField] private ParticleSystem gunParticles;
        [SerializeField] private LineRenderer gunLine;
        [SerializeField] private AudioSource gunAudio;
        [SerializeField] private Light gunLight;
        [SerializeField] private Light faceLight;

        #endregion

        #region ===== Private Fields =====

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
        private EnemyController targetEnemy = null;

        #endregion

        #region ===== Unity Methods =====

        private void Awake()
        {
            ID = ++Count;
            animator = GetComponent<Animator>();
            playerRB = GetComponent<Rigidbody>();
            shootableMask = LayerMask.GetMask("Shootable");

            if (shootingEffectObject != null)
            {
                gunParticles = shootingEffectObject.GetComponent<ParticleSystem>();
                gunLine     = shootingEffectObject.GetComponent<LineRenderer>();
                gunAudio    = shootingEffectObject.GetComponent<AudioSource>();
                gunLight    = shootingEffectObject.GetComponent<Light>();
                faceLight   = shootingEffectObject.GetComponentInChildren<Light>();
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

        #endregion

        #region ===== Health Methods =====

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
            if (index >= 0)
                SceneManager.LoadScene(index);
        }

        #endregion

        #region ===== Targeting =====

        private void FindTarget()
        {
            targetEnemy = EnemyManager.Instance.TryGetClosedEnemy(transform.position, targetRange, out var enemy) ? enemy : null;
        }

        #endregion

        #region ===== Movement & Rotation =====

        private void HandleMovement()
        {
            if (InputManager.Instance == null) return;

            moveInput = InputManager.Instance.MoveInput;
            var dir = moveInput.ToVector3XZ().normalized;
            var newPos = transform.position + dir * moveSpeed * Time.fixedDeltaTime;
            playerRB.MovePosition(newPos);
        }

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

        #endregion

        #region ===== Shooting =====

        private void HandleShooting()
        {
            if (Time.timeScale <= 0f || Time.time < lastShotTime + timeBetweenBullets) return;

            lastShotTime = Time.time;

            switch (shootingMode)
            {
                case ShootingMode.Single:
                    PerformRaycast();
                    break;
                case ShootingMode.Shotgun:
                    PerformShotgunBlast();
                    break;
            }

            PlayMuzzleEffects();
            DisableEffects(timeBetweenBullets * 0.2f);
        }

        private void PerformRaycast()
        {
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

        private void PerformShotgunBlast()
        {
            Vector3 origin = shootingEffectObject.transform.position;
            Vector3 forward = transform.forward;

            Collider[] hits = Physics.OverlapSphere(origin, shotgunRadius, shootableMask);
            foreach (var col in hits)
            {
                if (col.TryGetComponent<EnemyController>(out var enemy))
                {
                    Vector3 dirToEnemy = (enemy.transform.position - origin).normalized;
                    float angle = Vector3.Angle(forward, dirToEnemy);

                    if (angle <= shotgunAngle / 2f)
                    {
                        enemy.TakeDamage(damagePerShot, enemy.transform.position, knockbackForce);
                    }
                }
            }

            if (gunLine != null)
            {
                gunLine.positionCount = shotgunRayCount * 2;
                for (int i = 0; i < shotgunRayCount; i++)
                {
                    float angleOffset = Mathf.Lerp(-shotgunAngle / 2f, shotgunAngle / 2f, i / (float)(shotgunRayCount - 1));
                    Vector3 dir = Quaternion.Euler(0f, angleOffset, 0f) * forward;
                    gunLine.SetPosition(i * 2, origin);
                    gunLine.SetPosition(i * 2 + 1, origin + dir * shotgunRadius);
                }
                gunLine.enabled = true;
            }
        }

        private void TryDealDamage(RaycastHit hit)
        {
            if (hit.collider.TryGetComponent<EnemyController>(out var enemy))
                enemy.TakeDamage(damagePerShot, hit.point, knockbackForce);
        }

        private void SetLineEnd(Vector3 endPoint)
        {
            if (gunLine != null)
            {
                gunLine.positionCount = 2;
                gunLine.SetPosition(1, endPoint);
            }
        }

        private void PlayMuzzleEffects()
        {
            gunAudio?.Play();
            if (gunLine != null) gunLine.enabled = true;
            if (gunLight != null) gunLight.enabled = true;
            if (faceLight != null) faceLight.enabled = true;
            gunParticles?.Stop();
            gunParticles?.Play();
            if (gunLine != null)
                gunLine.SetPosition(0, shootingEffectObject.transform.position);
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

        #endregion
    }
}