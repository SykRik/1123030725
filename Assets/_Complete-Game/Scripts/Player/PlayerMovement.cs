using UnityEngine;
using UnityEngine.InputSystem;

namespace CompleteProject
{
	public class PlayerMovement : MonoBehaviour
	{
		#region ===== Fields =====

		[SerializeField] 
		private float     speed     = 6f;
		private Animator  animator  = null;
		private Rigidbody playerRB  = null;
		private int       floorMask = 0;
		private float     rayLength = 100f;
		private Vector2   moveInput = Vector2.zero;

		#endregion

		#region ===== Methods =====

		private void Awake()
		{
			floorMask = LayerMask.GetMask("Floor");
			animator  = GetComponent<Animator>();
			playerRB  = GetComponent<Rigidbody>();
		}

		private void FixedUpdate()
		{
			moveInput = PlayerInputHandler.Instance.MoveInput;
			Rotation(Time.fixedDeltaTime);
			Movement(Time.fixedDeltaTime);
			Animating(Time.fixedDeltaTime);
		}

		private void Rotation(float dt)
		{
			if (Mouse.current == null)
			{
				Debug.LogWarning("Missing Mouse.current.");
				return;
			}

			var pos = Mouse.current.position.ReadValue();
			var ray = Camera.main.ScreenPointToRay(pos);

			Debug.DrawRay(ray.origin, ray.direction * rayLength, Color.red);

			if (!Physics.Raycast(ray, out var floorHit, rayLength, floorMask))
			{
				Debug.LogWarning("Raycast missed the Floor.");
				return;
			}

			var direction = floorHit.point - transform.position;
			var rotation  = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));

			playerRB.MoveRotation(rotation);
		}

		private void Movement(float dt)
		{
			var horizontal = moveInput.x;
			var vertical   = moveInput.y;
			var direction  = new Vector3(horizontal, 0f, vertical);
			var position   = direction.normalized * speed * dt;

			playerRB.MovePosition(transform.position + position);
		}

		private void Animating(float dt)
		{
			var horizontal = moveInput.x;
			var vertical   = moveInput.y;
			var isWalking  = horizontal != 0f || vertical != 0f;

			animator.SetBool("IsWalking", isWalking);
		}

		#endregion
	}
}