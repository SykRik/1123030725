using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Effect/EffectDataSO")]
public class EffectDataSO : ScriptableObject
{
    [System.Serializable]
    public class VFXEntry
    {
        public ParticleManager.VFXType type;
        public GameObject prefab;
    }

    public List<VFXEntry> vfxEntries = new List<VFXEntry>();
}