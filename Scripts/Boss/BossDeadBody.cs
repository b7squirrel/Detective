using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossDeadBody : MonoBehaviour
{
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
        Time.timeScale = .5f;
        anim.SetTrigger("Die");

        yield return new WaitForSecondsRealtime(waitingTime);
        FindObjectOfType<PauseManager>().UnPauseGame();

        // GameManager.instance.GetComponent<BossHealthBarManager>().DeActivateBossHealthBar();
        // GameManager.instance.GetComponent<WinStage>().OpenPanel(); 
    }
}
