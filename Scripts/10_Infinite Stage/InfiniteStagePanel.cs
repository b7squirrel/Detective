using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class InfiniteStagePanel : MonoBehaviour
{
    [Header("최고 기록")]
    [SerializeField] TextMeshProUGUI bestWave;
    [SerializeField] TextMeshProUGUI bestTime;

    [Header("무한 모드 패널")]
    [SerializeField] Animator infiniteModePanelAnim;
    [SerializeField] Animator infinitePanelOpenerAnim;

    [Header("시작 버튼")]
    [SerializeField] GameObject startButton;
    Coroutine co;
    PlayerDataManager playerDataManager;

    public void InitInfinitePanel()
    {
        if (playerDataManager == null) playerDataManager = FindObjectOfType<PlayerDataManager>();
        bestWave.text = playerDataManager.GetBestWave().ToString();
        float recordTime = playerDataManager.GetBestSurvivalTime();
        bestTime.text = new GeneralFuctions().FormatTime(recordTime);
    }

    void ActivateStartButton()
    {
        co = StartCoroutine(ActivateStartButtonCo());
    }
    IEnumerator ActivateStartButtonCo()
    {
        startButton.GetComponent<Button>().interactable = false;
        yield return new WaitForSeconds(0.25f);
        startButton.SetActive(true);
        startButton.GetComponent<Button>().interactable = true;
    }
    void DeactivateStartButton()
    {
        startButton.SetActive(false);
    }

    public void ActivateInfinitePanel()
    {
        infiniteModePanelAnim.gameObject.SetActive(true);
        infiniteModePanelAnim.SetTrigger("Up");
        infinitePanelOpenerAnim.SetTrigger("Off");
        if(co != null) StopCoroutine(co);
        DeactivateStartButton();
    }
    public void DeactivateInfinitePanel()
    {
        infiniteModePanelAnim.gameObject.SetActive(false);
        infiniteModePanelAnim.SetTrigger("Down");
        infinitePanelOpenerAnim.SetTrigger("On");
        ActivateStartButton();
    }
}
