using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossWarningPanel : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI bossName;
    [SerializeField] GameObject bossWarningPanel;
    [SerializeField] Animator anim;
    [SerializeField] Image tagColorImage;
    [SerializeField] Color tagColor;

    [SerializeField] AudioClip startingSound;
    [SerializeField] AudioClip closingSound;
    [SerializeField] AudioClip idleSound;

    public void Init(string _name)
    {
        bossName.text = _name + " !";
        tagColorImage.color = tagColor;
        UIEvent bossEvent = new UIEvent(() => ActivateWarning(), "Boss");
        GameManager.instance.popupManager.EnqueueUIEvent(bossEvent);
    }
    public void Close()
    {
        anim.SetTrigger("Close");
        StartCoroutine(Deactivate());
    }

    public void ActivateWarning()
    {
        bossWarningPanel.SetActive(true);
        StartCoroutine(ActivateBossWarning());
    }
    IEnumerator Deactivate()
    {
        yield return new WaitForSecondsRealtime(.5f);
        bossWarningPanel.SetActive(false);
        GameManager.instance.pauseManager.UnPauseGame();
        GameManager.instance.popupManager.IsUIDone = true;
    }
    IEnumerator ActivateBossWarning()
    {
        PauseManager pm = GameManager.instance.pauseManager;
        pm.PauseGame();
        yield return new WaitForSecondsRealtime(2f);
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