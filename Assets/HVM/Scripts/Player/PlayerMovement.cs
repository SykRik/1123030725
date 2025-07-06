using UnityEngine;

namespace HVM
{
	public class PlayerMovement : MonoBehaviour
	{
		#region ===== Serialized Fields =====

		[SerializeField] private float speed = 6f;
		[SerializeField] private float attackRange = 10f;

		#endregion

		#region ===== Private Fields =====

		private Animator animator;
		private Rigidbody playerRB;
		private Vector2 moveInput = Vector2.zero;

		#endregion

		#region ===== Unity Methods =====

		private void Awake()
		{
			animator = GetComponent<Animator>();
			playerRB = GetComponent<Rigidbody>();
		}

		private void FixedUpdate()
		{
			moveInput = InputManager.Instance.MoveInput;

			RotateLogic();
			MovePlayer();
			UpdateAnimation();
		}

		#endregion

		#region ===== Core Logic =====

		private void RotateLogic()
		{
			var target = FindTargetEnemyInRange();

			if (target != null)
			{
				var direction = target.transform.position - transform.position;
				RotateTowards(direction);
			}
			else if (moveInput.sqrMagnitude > 0.01f)
			{
				var direction = moveInput.ToVector3XZ();
				RotateTowards(direction);
			}
		}

		private void RotateTowards(Vector3 input)
		{
			var direction = input.ToVector3XZ();
			var targetRot = Quaternion.LookRotation(direction);
			
			playerRB.MoveRotation(targetRot);
		}

		private void MovePlayer()
		{
			var direction = moveInput.ToVector3XZ().normalized;
			var position = transform.position + direction * speed * Time.fixedDeltaTime;

			playerRB.MovePosition(position);
		}

		private void UpdateAnimation()
		{
			var isWalking = moveInput.sqrMagnitude > 0.01f;
			animator.SetBool("IsWalking", isWalking);
		}

		#endregion

		#region ===== Target Selection =====

		private EnemyController FindTargetEnemyInRange()
		{
			return EnemyManager.Instance.TryGetClosedEnemy(transform.position, attackRange, out var enemy) ? enemy : null;
		}

		#endregion
	}
}
