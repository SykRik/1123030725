using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T _instance;

	public static T Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FindObjectOfType<T>();
				if (_instance == null)
				{
					Debug.LogError($"[MonoSingleton] Instance of {typeof(T)} not found in the scene.");
				}
			}

			return _instance;
		}
		private set => _instance = value;
	}

	protected virtual void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Debug.LogWarning($"[MonoSingleton] Duplicate {typeof(T)} found. Destroying this one: {name}");
			Destroy(gameObject);
			return;
		}

		_instance = this as T;
		DontDestroyOnLoad(gameObject);
	}
}