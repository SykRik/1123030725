using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoSingleton<SoundManager>
{
    public enum SFXType
    {
        ButtonClick,
        Explosion,
        GunShot,
        ZombieHit,
        PlayerHurt,
        Victory,
        Defeat
    }

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private SoundDataSO soundData;

    private Dictionary<SFXType, AudioClip> sfxDict;

    private void Awake()
    {
        sfxDict = new Dictionary<SFXType, AudioClip>();
        foreach (var entry in soundData.sfxEntries)
        {
            if (!sfxDict.ContainsKey(entry.type))
                sfxDict[entry.type] = entry.clip;
        }
    }

    public void PlaySFX(SFXType type)
    {
        if (sfxDict.TryGetValue(type, out var clip))
            sfxSource.PlayOneShot(clip);
        else
            Debug.LogWarning($"[SoundManager] Missing SFX for type: {type}");
    }
}