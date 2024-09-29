using System.Collections;
using UnityEngine;

public class IncomingWarningPanel : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI incomingText;
    [SerializeField] GameObject incomingWarningPanel;
    [SerializeField] Animator anim;

    public void Init()
    {
        incomingText.text = "Enemies Incoming!!!";
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
}
