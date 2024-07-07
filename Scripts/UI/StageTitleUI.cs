using System.Collections;
using UnityEngine;

public class StageTitleUI : MonoBehaviour
{
    [SerializeField] GameObject stageTitlePanel;
    [SerializeField] TMPro.TextMeshProUGUI title;
    [SerializeField] TMPro.TextMeshProUGUI bossName;

    [SerializeField] float titleDuration;

    private void Start()
    {
        //Init();
    }
    public void Init()
    {
        PlayerDataManager playerData = FindObjectOfType<PlayerDataManager>();
        StageInfo stageInfo = FindObjectOfType<StageInfo>();
        int index = playerData.GetCurrentStageNumber();
        title.text = "Stage " + index.ToString();
        bossName.text = stageInfo.GetStageInfo(index).Title;

        StartCoroutine(StageTitleUpCo());
    }

    IEnumerator StageTitleUpCo()
    {
        stageTitlePanel.SetActive(true);
        yield return new WaitForSeconds(titleDuration);

        stageTitlePanel.SetActive(false);
    }
}