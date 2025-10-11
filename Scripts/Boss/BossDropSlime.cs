using UnityEngine;

public class BossDropSlime : MonoBehaviour
{
    [Header("사운드")]
    [SerializeField] AudioClip dropSound;
    void OnEnable()
    {
        GameManager.instance.loopSoundManager.RegisterAudio(dropSound);
    }
    void OnDisable()
    {
        GameManager.instance.loopSoundManager.UnregisterAudio(dropSound);
    }
}
