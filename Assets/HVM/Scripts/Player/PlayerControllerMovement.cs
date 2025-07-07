using UnityEngine;

namespace HVM
{
    public partial class PlayerController
    {
        [Header("Movement")] 
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float targetRange = 10f;
        [SerializeField] private float rotateSpeed = 720f;

        private Vector2 moveInput;
        private Rigidbody playerRB;
        private Animator animator;
        private EnemyController targetEnemy = null;

        private void UpdateMovement()
        {
            FindTarget();
            HandleMovement();
            HandleRotation();
            UpdateWalkAnimation();
        }

        private void FindTarget()
        {
            targetEnemy = EnemyManager.Instance.TryGetClosedEnemy(transform.position, targetRange, out var enemy)
                ? enemy
                : null;
        }

        private void HandleMovement()
        {
            moveInput = InputManager.Instance.MoveInput;
            var direction = moveInput.ToVector3XZ().normalized;
            var position = transform.position + direction * moveSpeed * Time.fixedDeltaTime;
            playerRB.MovePosition(position);
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
    }
}