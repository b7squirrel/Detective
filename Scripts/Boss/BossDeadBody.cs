using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossDeadBody : MonoBehaviour
{
    [SerializeField] AudioClip crownDropSFX;
    [SerializeField] AudioClip squelchSFX;
    [SerializeField] AudioClip squeackSFX;
    Animator anim;
    void OnEnable()
    {
        anim = GetComponent<Animator>();
        StartCoroutine(DieEvent(.1f, 2f));
    }

    IEnumerator DieEvent(float desiredTimeScale, float waitingTime)
    {
        Time.timeScale = desiredTimeScale;
        yield return new WaitForSecondsRealtime(waitingTime);
        FindObjectOfType<PauseManager>().UnPauseGame();
        anim.SetTrigger("Die");
        BossDieManager bossDieManager = new BossDieManager();
        bossDieManager.RemoveAllEnemies();

        StartCoroutine(WinMessage());
    }
    IEnumerator WinMessage()
    {
        yield return new WaitForSeconds(5f);
        GameManager.instance.GetComponent<WinStage>().OpenPanel(); 
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
}
