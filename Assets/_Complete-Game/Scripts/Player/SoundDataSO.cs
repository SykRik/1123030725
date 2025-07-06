using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Audio/SoundData")]
public class SoundDataSO : ScriptableObject
{
    [System.Serializable]
    public class SFXEntry
    {
        public SoundManager.SFXType type;
        public AudioClip clip;
    }

    public List<SFXEntry> sfxEntries = new List<SFXEntry>();
}