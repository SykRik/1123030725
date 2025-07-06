using System.Collections.Generic;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HVM
{
    [CreateAssetMenu(menuName = "Game/Audio/AudioDataSO")]
    public class AudioDataSO : ScriptableObject
    {
        public enum AudioType { Sound, Music }

        [System.Serializable]
        public class AudioEntry
        {
            public string name;
            public AudioClip clip;
            public AudioType type;
        }

        [SerializeField] private List<AudioEntry> entries = new();
        public List<AudioEntry> Entries => entries;

#if UNITY_EDITOR
        private const string AssetPath = "Assets/HVM/Datas/AudioDataSO.asset";
        private readonly string[] extensions = { "*.wav", "*.mp3", "*.ogg" };

        private void OnValidate()
        {
            if (!File.Exists(AssetPath))
                CreateOrLoadAsset();
        }

        [ContextMenu("Reload Audio Data")]
        public void AutoPopulateFromFolders()
        {
            entries.Clear();
            PopulateEntries("Assets/HVM/Audio/Sound", AudioType.Sound);
            PopulateEntries("Assets/HVM/Audio/Music", AudioType.Music);

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            Debug.Log("[AudioDataSO] Audio data reloaded.");
        }

        private void PopulateEntries(string folderPath, AudioType type)
        {
            foreach (var ext in extensions)
            {
                var filePaths = Directory.GetFiles(folderPath, ext, SearchOption.AllDirectories);
                foreach (var path in filePaths)
                {
                    var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                    if (clip != null)
                    {
                        entries.Add(new AudioEntry
                        {
                            name = Path.GetFileNameWithoutExtension(path),
                            clip = clip,
                            type = type
                        });
                    }
                }
            }
        }

        public static AudioDataSO CreateOrLoadAsset()
        {
            var folder = Path.GetDirectoryName(AssetPath);
            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder("Assets/HVM", "Datas");

            var asset = AssetDatabase.LoadAssetAtPath<AudioDataSO>(AssetPath);
            if (asset == null)
            {
                asset = CreateInstance<AudioDataSO>();
                AssetDatabase.CreateAsset(asset, AssetPath);
                asset.AutoPopulateFromFolders();
                AssetDatabase.SaveAssets();
                Debug.Log("[AudioDataSO] Created and populated new asset at " + AssetPath);
            }

            return asset;
        }
#endif
    }
}
