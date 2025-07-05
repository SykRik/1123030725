using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace CompleteProject
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
				var direction = new Vector3(moveInput.x, 0f, moveInput.y);
				RotateTowards(direction);
			}
		}

		private void RotateTowards(Vector3 direction)
		{
			var targetRot = Quaternion.LookRotation(Vector3.Normalize(new Vector3(direction.x, 0f, direction.z)));
			playerRB.MoveRotation(targetRot);
		}

		private void MovePlayer()
		{
			Vector3 moveDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
			Vector3 newPos = transform.position + moveDir * speed * Time.fixedDeltaTime;

			playerRB.MovePosition(newPos);
		}

		private void UpdateAnimation()
		{
			bool isWalking = moveInput.sqrMagnitude > 0.01f;
			animator.SetBool("IsWalking", isWalking);
		}

		#endregion

		#region ===== Target Selection =====

		private Enemy FindTargetEnemyInRange()
		{
			return GameManager.Instance.EnemyManager.TryGetClosedEnemy(transform.position, attackRange, out var enemy) ? enemy : null;
		}

		#endregion
	}
}
