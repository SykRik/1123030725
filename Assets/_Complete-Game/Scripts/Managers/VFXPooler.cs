using System.Collections;
using UnityEngine;
using HVM;

public class VFXPooler : BasePooler<ParticleController>
{
    protected override void OnRequest(ParticleController fx)
    {
        fx.gameObject.SetActive(true);
        fx.Play();
        StartCoroutine(AutoReturn(fx, fx.Duration));
    }

    protected override void OnReturn(ParticleController fx)
    {
        fx.Stop();
        fx.gameObject.SetActive(false);
    }

    private IEnumerator AutoReturn(ParticleController fx, float delay)
    {
        yield return new WaitForSeconds(delay);
        Return(fx);
    }
}