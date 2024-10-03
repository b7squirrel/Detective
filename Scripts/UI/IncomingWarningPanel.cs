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
        incomingText.text = "������ �����ɴϴ�!!!";
        incomingWarningPanel.SetActive(true);
        StartCoroutine(ActivateIncomingWarning());
    }
    public void Close()
    {
        anim.SetTrigger("Close");
        StartCoroutine(Deactivate());
    }
    IEnumerator Deactivate()
    {
        yield return new WaitForSecondsRealtime(.5f);
        incomingWarningPanel.SetActive(false);
        GameManager.instance.pauseManager.UnPauseGame();
    }
    IEnumerator ActivateIncomingWarning()
    {
        PauseManager pm = GameManager.instance.pauseManager;
        pm.PauseGame();
        yield return new WaitForSecondsRealtime(2f);
        Close();
    }

    // �ִϸ��̼� �̺�Ʈ
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
    public void StopIdleSound()
    {
        SoundManager.instance.StopPlaying(idleSound);
    }
}
