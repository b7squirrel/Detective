using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class TeleportEffect : MonoBehaviour
{
    [SerializeField] GameObject teleportEffectPrefab;
    [SerializeField] GameObject teleportOutEffectPrefab;
    [SerializeField] GameObject teleportUpEffectPrefab;
    [SerializeField] AudioClip teleportSound;
    [SerializeField] AudioClip teleportOutSound;
    ParticleSystem particleSys;

    public void GenTeleportEffect(Vector2 _spawnPos)
    {
        GameObject teleEffect = Instantiate(teleportEffectPrefab, _spawnPos, Quaternion.identity);
        SoundManager.instance.Play(teleportSound);
    }

    public void GenTeleportOutEffect(Vector2 _spawnPos)
    {
        StartCoroutine(GenTeleportOutEffectCo(_spawnPos));
    }
    IEnumerator GenTeleportOutEffectCo(Vector2 _spawnPos)
    {
        GameObject teleUpEffect = Instantiate(teleportUpEffectPrefab, _spawnPos, quaternion.identity);
        particleSys = teleUpEffect.GetComponentInChildren<ParticleSystem>();
        particleSys.Play();

        yield return new WaitForSeconds(.3f);
        GameObject teleEffect = Instantiate(teleportOutEffectPrefab, _spawnPos, Quaternion.identity);
        SoundManager.instance.Play(teleportOutSound);
        CameraShake.instance.Shake();

        yield return new WaitForSeconds(.5f);
        particleSys.Stop();
    }
}