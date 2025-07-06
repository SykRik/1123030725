using HVM;
using UnityEngine;

public class ParticleController : MonoBehaviour, IObjectID
{
    public static int Count = 0;

    public int ID { get; private set; }
    [SerializeField] private float duration = 2f;

    public float Duration => duration;

    private void Awake()
    {
        ID = ++Count;
    }

    public void Play()
    {
        foreach (var ps in GetComponentsInChildren<ParticleSystem>())
        {
            ps.Play();
        }
    }

    public void Stop()
    {
        foreach (var ps in GetComponentsInChildren<ParticleSystem>())
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}