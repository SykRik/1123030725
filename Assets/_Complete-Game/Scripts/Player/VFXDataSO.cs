using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/VFX/VFXData")]
public class VFXDataSO : ScriptableObject
{
    [System.Serializable]
    public class VFXEntry
    {
        public ParticleManager.VFXType type;
        public GameObject prefab;
    }

    public List<VFXEntry> vfxEntries = new List<VFXEntry>();
}