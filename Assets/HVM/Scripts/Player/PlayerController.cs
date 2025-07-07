using UnityEngine;
using UnityEngine.AI;

namespace HVM
{
    public partial class PlayerController : MonoBehaviour, IObjectID
    {
        public static int Count = 0;

#if UNITY_EDITOR
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void ResetIDCounter() => Count = 0;
#endif

        public int ID { get; private set; }
        public int CurrentHealth => currentHealth;

        public enum TypeOfWeapon
        {
            Rifle = 1,
            Shotgun = 2
        }

        [Header("Health")]
        [SerializeField] private int startingHealth = 100;
        [SerializeField] private UnityEngine.UI.Slider healthSlider;
        [SerializeField] private UnityEngine.UI.Image damageImage;
        [SerializeField] private AudioClip deathClip;
        [SerializeField] private float flashSpeed = 5f;
        [SerializeField] private Color flashColor = new Color(1f, 0f, 0f, 0.1f);

        private int currentHealth;
        private bool isDead;
        private bool isDamaged;

        private void Awake()
        {
            ID = ++Count;
            currentHealth = startingHealth;
            ApplyWeaponData(currentWeapon);
        }

        private void Start()
        {
            shootableMask = LayerMask.GetMask("Shootable");
            GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            agent    = GetComponent<NavMeshAgent>();
            
            if (shootingEffectObject != null)
            {
                gunParticles = shootingEffectObject.GetComponent<ParticleSystem>();
                gunLine     = shootingEffectObject.GetComponent<LineRenderer>();
                gunAudio    = shootingEffectObject.GetComponent<AudioSource>();
                gunLight    = shootingEffectObject.GetComponent<Light>();
                faceLight   = shootingEffectObject.GetComponentInChildren<Light>();
            }
        }

        private void Update()
        {
            UpdateAttack();
        }

        private void FixedUpdate()
        {
            UpdateMovement();
        }

        private void Die()
        {
            isDead = true;
            DisableEffects();
            GetComponent<Animator>()?.SetTrigger("Die");

            var audio = GetComponent<AudioSource>();
            if (deathClip != null && audio != null)
            {
                audio.clip = deathClip;
                audio.Play();
            }

            enabled = false;
        }

        public void ResetState()
        {
            enabled = true;
            isDead        = false;
            currentHealth = startingHealth;
            GetComponent<Animator>()?.SetTrigger("Revive");
            GetComponent<NavMeshAgent>().enabled = true;
        }

    }
}