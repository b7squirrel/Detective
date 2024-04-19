using UnityEngine;

public class StageInfoUI : MonoBehaviour
{
    [SerializeField] PlayerDataManager stageManager;
    [SerializeField] TMPro.TextMeshProUGUI Title;
    [SerializeField] TMPro.TextMeshProUGUI stageNumber;

    internal void Init(Stages _currentStage)
    {
        stageManager = FindObjectOfType<PlayerDataManager>();
        Title.text = _currentStage.Title;
        stageNumber.text = "Stage " + stageManager.GetCurrentStageNumber().ToString();
    }
}