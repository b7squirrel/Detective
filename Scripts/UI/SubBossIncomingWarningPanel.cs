using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubBossIncomingWarningPanel : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI incomingText;
    [SerializeField] GameObject incomingWarningPanel;
    [SerializeField] Animator anim;

    [SerializeField] AudioClip startingSound;
    [SerializeField] AudioClip closingSound;
    [SerializeField] AudioClip idleSound;

    public void Init()
    {
        incomingText.text = "적들이 몰려옵니다!!!";

        UIEvent incomingEvent = new UIEvent(() => ActivateWarning(), "Incoming", ForceClose); // ⭐ ForceClose 연결
        GameManager.instance.popupManager.EnqueueUIEvent(incomingEvent);
    }

    public void Close()
    {
        anim.SetTrigger("Close");
        StartCoroutine(Deactivate());
    }

    public void ActivateWarning()
    {
        incomingWarningPanel.SetActive(true);
        StartCoroutine(ActivateIncomingWarning());
    }

    IEnumerator Deactivate()
    {
        yield return new WaitForSecondsRealtime(.5f);
        incomingWarningPanel.SetActive(false);
        GameManager.instance.pauseManager.UnPauseGame();
        GameManager.instance.popupManager.IsUIDone = true;
    }

    IEnumerator ActivateIncomingWarning()
    {
        PauseManager pm = GameManager.instance.pauseManager;
        pm.PauseGame();
        yield return new WaitForSecondsRealtime(2f);
        Close();
    }

    // ⭐ 추가: 강제 종료 (UnPauseGame 호출 안 함)
    public void ForceClose()
    {
        StopAllCoroutines();
        incomingWarningPanel.SetActive(false);
    }

    public void PlayStartingSound()
    {
        SoundManager.instance.Play(startingSound);
    }
    public void PlayClosingSound()
    {
        SoundManager.instance.Play(closingSound);
    }
    public void PlayIdleSound()
    {
        SoundManager.instance.Play(idleSound);
    }
}