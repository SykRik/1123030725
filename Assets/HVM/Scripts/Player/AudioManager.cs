using System.Collections.Generic;
using UnityEngine;

namespace HVM
{
    public class AudioManager : MonoSingleton<AudioManager>
    {
        [SerializeField] private AudioDataSO audioDataSo;
        [SerializeField] private AudioSource soundSource;
        [SerializeField] private AudioSource musicSource;

        private Dictionary<string, AudioClip> soundMap = new();
        private Dictionary<string, AudioClip> musicMap = new();

        private void Awake()
        {
#if UNITY_EDITOR
            if (audioDataSo == null)
                audioDataSo = AudioDataSO.CreateOrLoadAsset();
            if (soundSource == null)
                Debug.LogWarning("[AudioManager] Missing reference: Sound Source");
            if (musicSource == null)
                Debug.LogWarning("[AudioManager] Missing reference: Music Source");
#endif
            ReloadAudioData();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (audioDataSo == null)
                audioDataSo = AudioDataSO.CreateOrLoadAsset();
            if (soundSource == null)
                Debug.LogWarning("[AudioManager] Missing reference: Sound Source");
            if (musicSource == null)
                Debug.LogWarning("[AudioManager] Missing reference: Music Source");
            ReloadAudioData();
        }
#endif

        [ContextMenu("Reload Audio Data")]
        public void ReloadAudioData()
        {
            soundMap.Clear();
            musicMap.Clear();

            foreach (var entry in audioDataSo.Entries)
            {
                if (entry.clip == null || string.IsNullOrEmpty(entry.name))
                    continue;

                switch (entry.type)
                {
                    case AudioDataSO.AudioType.Sound:
                        soundMap[entry.name] = entry.clip;
                        break;
                    case AudioDataSO.AudioType.Music:
                        musicMap[entry.name] = entry.clip;
                        break;
                    default:
                        Debug.LogWarning($"[AudioManager] Unknown audio type for: {entry.name}");
                        break;
                }
            }

            Debug.Log("[AudioManager] Audio dictionaries reloaded.");
        }

        public void PlaySound(string name)
        {
            if (soundMap.TryGetValue(name, out var clip))
            {
                soundSource.PlayOneShot(clip);
                Debug.Log($"[AudioManager] Playing SFX: {name}");
            }
            else
            {
                Debug.LogWarning($"[AudioManager] Missing SFX: {name}");
            }
        }

        public void StopSound()
        {
            soundSource.Stop();
            Debug.Log("[AudioManager] SFX stopped.");
        }

        public void MuteSound(bool mute)
        {
            soundSource.mute = mute;
            Debug.Log($"[AudioManager] SFX muted: {mute}");
        }

        public void SetVolumeSound(float volume)
        {
            soundSource.volume = Mathf.Clamp01(volume);
            Debug.Log($"[AudioManager] SFX volume set to: {soundSource.volume}");
        }

        public void PlayMusic(string name, bool loop = true)
        {
            if (musicMap.TryGetValue(name, out var clip))
            {
                musicSource.clip = clip;
                musicSource.loop = loop;
                musicSource.Play();
                Debug.Log($"[AudioManager] Playing music: {name}");
            }
            else
            {
                Debug.LogWarning($"[AudioManager] Missing Music: {name}");
            }
        }

        public void StopMusic()
        {
            musicSource.Stop();
            Debug.Log("[AudioManager] Music stopped.");
        }

        public void MuteMusic(bool mute)
        {
            musicSource.mute = mute;
            Debug.Log($"[AudioManager] Music muted: {mute}");
        }

        public void SetVolumeMusic(float volume)
        {
            musicSource.volume = Mathf.Clamp01(volume);
            Debug.Log($"[AudioManager] Music volume set to: {musicSource.volume}");
        }
    }
}
