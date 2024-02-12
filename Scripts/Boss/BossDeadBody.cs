using UnityEngine;

public class BossDeadBody : MonoBehaviour
{
    [SerializeField] AudioClip crownDropSFX;
    [SerializeField] AudioClip squelchSFX;
    [SerializeField] AudioClip squeackSFX;
    Animator anim;
    public bool FinishBossCam {get; private set;}
    void OnEnable()
    {
        anim = GetComponent<Animator>();
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
