using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossWarningPanel : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI bossName;
    [SerializeField] GameObject bossWarningPanel;
    [SerializeField] Animator anim;

    public void Init(string _name)
    {
        bossName.text = "The " + _name + " is coming!";
        bossWarningPanel.SetActive(true);
        StartCoroutine(ActivateBossWarning());
    }
    public void Close()
    {
        anim.SetTrigger("Close");
        StartCoroutine(Deactivate());
    }
    IEnumerator Deactivate()
    {
        yield return new WaitForSecondsRealtime(.5f);
        bossWarningPanel.SetActive(false);
        GameManager.instance.pauseManager.UnPauseGame();
    }
    IEnumerator ActivateBossWarning()
    {
        PauseManager pm = GameManager.instance.pauseManager;
        pm.PauseGame();
        yield return new WaitForSecondsRealtime(2f);
        Close();
    }
}