using System.Collections;
using UnityEngine;

public class TeleportEffect : MonoBehaviour
{
    [SerializeField] GameObject teleportEffectPrefab;
    [SerializeField] AudioClip teleportSound;

    public void GenTeleportEffect(Vector2 _spawnPos)
    {
        GameObject teleEffect = Instantiate(teleportEffectPrefab, _spawnPos, Quaternion.identity);
        SoundManager.instance.Play(teleportSound);
    }
}