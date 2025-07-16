using UnityEngine;

public class TeleportEffect : MonoBehaviour
{
    [SerializeField] GameObject teleportEffectPrefab;
    [SerializeField] GameObject teleportOutEffectPrefab;
    [SerializeField] AudioClip teleportSound;
    [SerializeField] AudioClip teleportOutSound;

    public void GenTeleportEffect(Vector2 _spawnPos)
    {
        GameObject teleEffect = Instantiate(teleportEffectPrefab, _spawnPos, Quaternion.identity);
        SoundManager.instance.Play(teleportSound);
    }

    public void GenTeleportOutEffect(Vector2 _spawnPos)
    {
        GameObject teleEffect = Instantiate(teleportOutEffectPrefab, _spawnPos, Quaternion.identity);
        SoundManager.instance.Play(teleportOutSound);
    }
}