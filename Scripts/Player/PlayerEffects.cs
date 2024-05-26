using System.Collections;
using UnityEngine;

public class PlayerEffects : MonoBehaviour
{
    [SerializeField] ParticleSystem wallDust;
    [SerializeField] float wallDustDuration;

    private void Start()
    {
        wallDust.Stop();
    }
    public void PlayWallDust()
    {
        if (wallDust.isPlaying) return;
        StartCoroutine(PlayParticle(wallDust, wallDustDuration));
    }
    IEnumerator PlayParticle(ParticleSystem _particle, float _duration)
    {
        _particle.Play();
        yield return new WaitForSeconds(_duration);
        _particle.Stop();
    }
}