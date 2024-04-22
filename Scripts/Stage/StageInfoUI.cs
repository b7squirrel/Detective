using UnityEngine;

public class StageInfoUI : MonoBehaviour
{
    [SerializeField] PlayerDataManager stageManager;
    [SerializeField] TMPro.TextMeshProUGUI Title;

    internal void Init(Stages _currentStage)
    {
        stageManager = FindObjectOfType<PlayerDataManager>();
        Title.text = stageManager.GetCurrentStageNumber().ToString() + ". " + _currentStage.Title;
    }
}