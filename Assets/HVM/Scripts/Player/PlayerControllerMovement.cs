using UnityEngine;
using UnityEngine.AI;

namespace HVM
{
    public partial class PlayerController
    {
        [Header("Movement")] 
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float targetRange  = 10f;
        [SerializeField] private float rotateSpeed  = 720f;
        [SerializeField] private float moveDistance = 2f;

        private Vector2         moveInput;
        private NavMeshAgent    agent;
        private Animator        animator;
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

            if (moveInput.sqrMagnitude < Mathf.Epsilon)
            {
                if (agent.hasPath || agent.velocity.sqrMagnitude > Mathf.Epsilon)
                {
                    agent.ResetPath();
                    agent.velocity = Vector3.zero; // <- dừng ngay
                }
                return;
            }

            Vector3 moveDir    = moveInput.ToVector3XZ().normalized;
            Vector3 desiredPos = transform.position + moveDir * moveDistance;

            if (NavMesh.SamplePosition(desiredPos, out var hit, 1f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }


        private void HandleRotation()
        {
            Vector3? lookDirection = null;

            if (targetEnemy != null)
            {
                Vector3 dirToEnemy = (targetEnemy.transform.position - transform.position).ToVector3XZ();
                if (dirToEnemy.sqrMagnitude > Mathf.Epsilon)
                    lookDirection = dirToEnemy;
            }
            else if (agent.velocity.sqrMagnitude > Mathf.Epsilon)
            {
                lookDirection = agent.velocity.normalized;
            }

            if (lookDirection.HasValue)
            {
                Quaternion currentRot = transform.rotation;
                Quaternion targetRot  = Quaternion.LookRotation(lookDirection.Value);
                Quaternion smoothRot  = Quaternion.RotateTowards(currentRot, targetRot, rotateSpeed * Time.deltaTime);
                transform.rotation = smoothRot;
            }
        }

        private void UpdateWalkAnimation()
        {
            animator.SetBool("IsWalking", moveInput.sqrMagnitude > 0.01f);
        }
    }
}