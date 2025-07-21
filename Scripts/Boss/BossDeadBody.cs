using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class BossDeadBody : MonoBehaviour
{
    [Header("이펙트")]
    [SerializeField] GameObject teleportEffectPrefab;

    [Header("사운드")]
    [SerializeField] AudioClip crownDropSFX;
    [SerializeField] AudioClip squelchSFX;
    [SerializeField] AudioClip squeackSFX;
    Animator anim;
    public bool FinishBossCam {get; private set;}
    void OnEnable()
    {
        anim = GetComponent<Animator>();
    }

    public void TeleportOutEffect()
    {
        StartCoroutine(TeleportOutEffectCo());
    }
    IEnumerator TeleportOutEffectCo()
    {
        GameManager.instance.GetComponent<TeleportEffect>().GenTeleportOutEffect(transform.position);
        yield return new WaitForSeconds(.45f);
        gameObject.SetActive(false);
    }

    //animation events
    public void PlayCrownDropSFX()
    {
        SoundManager.instance.Play(crownDropSFX);
    }
    public void PlayerSquelchSFX()
    {
        SoundManager.instance.Play(squelchSFX);
        SoundManager.instance.Play(squeackSFX);
    }
    public void TriggerPlayerCamera()
    {
        BossDieManager.instance.BossCameraOff();
    }
}
