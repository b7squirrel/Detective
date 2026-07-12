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
        // ⭐ incomingWarningPanel.SetActive(true)는 여기서 호출하지 않음.
        // 큐 처리 순서와 무관하게 패널이 먼저 보여버리는 문제를 막기 위해
        // 실제 활성화는 ActivateWarning()(큐 차례가 왔을 때, PauseGame과 같은 타이밍)에서 수행.

        UIEvent incomingEvent = new UIEvent(() => ActivateWarning(), "Incoming");
        GameManager.instance.popupManager.EnqueueUIEvent(incomingEvent);
    }

    public void Close()
    {
        anim.SetTrigger("Close");
        StartCoroutine(Deactivate());
    }

    public void ActivateWarning()
    {
        incomingWarningPanel.SetActive(true); // ⭐ 여기서 패널 활성화 (PauseGame 호출과 같은 타이밍)
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
        Debug.Log("CLOSE");
        Close();
    }

    // 애니메이션 이벤트
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