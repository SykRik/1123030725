using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoSingleton<ParticleManager>
{
    public enum VFXType
    {
        Explosion,
        BloodHit,
        HealEffect,
        LevelUp,
        MuzzleFlash
    }

    [System.Serializable]
    public class VFXMapping
    {
        public VFXType type;
        public VFXPooler pooler;
    }

    [SerializeField] private VFXMapping[] vfxMappings;
    private readonly Dictionary<VFXType, VFXPooler> _vfxDict = new();

    protected override void Awake()
    {
        base.Awake();
        foreach (var map in vfxMappings)
        {
            _vfxDict[map.type] = map.pooler;
        }
    }

    public void PlayVFX(VFXType type, Vector3 position, Quaternion rotation)
    {
        if (_vfxDict.TryGetValue(type, out var pooler))
        {
            var fx = pooler.Request();
            if (fx != null)
            {
                fx.transform.SetPositionAndRotation(position, rotation);
            }
        }
        else
        {
            Debug.LogWarning($"[ParticleManager] No pooler found for {type}");
        }
    }
}