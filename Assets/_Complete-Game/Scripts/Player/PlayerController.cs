using UnityEngine;

namespace HVM
{
    [RequireComponent(typeof(PlayerHealth))]
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(PlayerShooting))]
    public class PlayerController : MonoBehaviour, IObjectID
    {
        public static int Count = 0;
        public int ID { get; private set; }

        [SerializeField] private PlayerHealth playerHealth = null;
        [SerializeField] private PlayerMovement playerMovement = null;
        [SerializeField] private PlayerShooting playerShooting = null;

        private void Awake()
        {
            ID = ++Count;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            bool hasError = false;

            if (playerHealth == null)
            {
                playerHealth = GetComponent<PlayerHealth>();
                if (playerHealth == null)
                {
                    Debug.LogWarning("[PlayerController] Missing PlayerHealth component!", this);
                    hasError = true;
                }
            }

            if (playerMovement == null)
            {
                playerMovement = GetComponent<PlayerMovement>();
                if (playerMovement == null)
                {
                    Debug.LogWarning("[PlayerController] Missing PlayerMovement component!", this);
                    hasError = true;
                }
            }

            if (playerShooting == null)
            {
                playerShooting = GetComponent<PlayerShooting>();
                if (playerShooting == null)
                {
                    Debug.LogWarning("[PlayerController] Missing PlayerShooting component!", this);
                    hasError = true;
                }
            }

            if (!hasError)
            {
                Debug.Log($"[PlayerController] Components validated for {gameObject.name}", this);
            }
        }

        [UnityEditor.InitializeOnEnterPlayMode]
        private static void ResetIDCounter()
        {
            Count = 0;
        }
#endif
    }
}