using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EggPanelManager : MonoBehaviour
{
    [SerializeField] GameObject eggPanel;
    [SerializeField] GameObject eggImage;
    [SerializeField] GameObject kidImage;
    [SerializeField] GameObject closeButton;
    [SerializeField] PauseManager pauseManager;
    [SerializeField] GameObject oriName;
    [SerializeField] GameObject newKidText;
    [SerializeField] GameObject blackBGPanel;
    RuntimeAnimatorController kidAnim;
    Coroutine Close;

    private void Awake()
    {
        pauseManager = GetComponent<PauseManager>();
    }
    public void EggPanelUP(RuntimeAnimatorController anim, string name)
    {
        pauseManager.PauseGame();
        eggPanel.SetActive(true);
        EggImageUp(true);
        kidAnim = anim;
        newKidText.SetActive(true);
        oriName.GetComponent<TMPro.TextMeshProUGUI>().text = name;
        oriName.SetActive(false);

        blackBGPanel.SetActive(true);
    }

    public void EggImageUp(bool isActive)
    {
        eggImage.SetActive(isActive);
    }
    void KidImageUp(bool isActive)
    {
        kidImage.SetActive(isActive);

        newKidText.SetActive(false);
        oriName.SetActive(true);

        if (isActive) kidImage.GetComponent<Animator>().runtimeAnimatorController = kidAnim;
        closeButton.SetActive(true);
    }
    public void EggAnimFinished()
    {
        KidImageUp(true);
        Close = StartCoroutine(CloseCo());
    }

    IEnumerator CloseCo()
    {
        yield return new WaitForSecondsRealtime(2.03f);
        CloseButtonPressed();
    }
    public void CloseButtonPressed()
    {
        EggImageUp(false);
        KidImageUp(false);
        pauseManager.UnPauseGame();
        closeButton.SetActive(false);
        eggPanel.SetActive(false);
        blackBGPanel.SetActive(false);
        newKidText.SetActive(false);
        oriName.SetActive(false);
        // 돌아가고 있는 코루틴을 멈추지 않으면 
        // 버튼을 누르지 않고 자동으로 창이 종료되었을 때 코루틴이 실행되어 정지된 시간을 풀어버림
        StopCoroutine(Close); 
    }
}
