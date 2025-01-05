using System.Collections;
using UnityEngine;

public class IncomingWarningPanel : MonoBehaviour
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
        incomingWarningPanel.SetActive(true);

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