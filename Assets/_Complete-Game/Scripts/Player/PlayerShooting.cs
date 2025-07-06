using System.Collections;
using UnityEngine;

namespace HVM
{
	public class PlayerShooting : MonoBehaviour
	{
		#region ===== Serialized Fields =====

		[SerializeField] private int damagePerShot = 20;
		[SerializeField] private float timeBetweenBullets = 0.15f;
		[SerializeField] private float range = 100f;
		[SerializeField] private float attackRange = 10f;

		#endregion

		#region ===== Private Fields =====

		private float effectsDisplayTime = 0.2f;
		private Ray shootRay = new Ray();
		private int shootableMask;

		private ParticleSystem gunParticles;
		private LineRenderer gunLine;
		private AudioSource gunAudio;
		private Light gunLight;
		private Light faceLight;

		private float lastShotTime = -999f;
		private Coroutine disableEffectRoutine;

		#endregion

		#region ===== Unity Methods =====

		private void Awake()
		{
			shootableMask = LayerMask.GetMask("Shootable");
			gunParticles = GetComponent<ParticleSystem>();
			gunLine = GetComponent<LineRenderer>();
			gunAudio = GetComponent<AudioSource>();
			gunLight = GetComponent<Light>();
			faceLight = GetComponentInChildren<Light>();
		}

		private void Update()
		{
			if (Time.timeScale <= 0f) return;
			if (Time.time < lastShotTime + timeBetweenBullets) return;

			if (HasEnemyInRange())
			{
				Shoot();
			}
		}

		#endregion

		#region ===== Auto Fire Logic =====

		private bool HasEnemyInRange()
		{
			return EnemyManager.Instance.TryGetClosedEnemy(transform.position, attackRange, out _);
		}

		#endregion

		#region ===== Core Methods =====

		private void Shoot()
		{
			lastShotTime = Time.time;

			PlayMuzzleEffects();
			PerformRaycast();
			DisableEffects(timeBetweenBullets * effectsDisplayTime);
		}

		private void PlayMuzzleEffects()
		{
			if (gunAudio != null) gunAudio.Play();
			if (gunLight != null) gunLight.enabled = true;
			if (faceLight != null) faceLight.enabled = true;

			if (gunParticles != null)
			{
				gunParticles.Stop();
				gunParticles.Play();
			}

			if (gunLine != null)
			{
				gunLine.enabled = true;
				gunLine.SetPosition(0, transform.position);
			}
		}

		private void PerformRaycast()
		{
			shootRay.origin = transform.position;
			shootRay.direction = transform.forward;

			if (Physics.Raycast(shootRay, out RaycastHit hitInfo, range, shootableMask))
			{
				TryDealDamage(hitInfo);
				SetLineEnd(hitInfo.point);
			}
			else
			{
				SetLineEnd(shootRay.origin + shootRay.direction * range);
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
		}

		private void DisableEffects(float delay)
		{
			if (delay > 0f)
			{
				RestartCoroutine(ref disableEffectRoutine, DisableEffectsAsync(delay));
			}
			else
			{
				DisableEffects();
			}
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
			if (routine != null)
				StopCoroutine(routine);

			routine = StartCoroutine(coroutine);
		}

		#endregion
	}
}
