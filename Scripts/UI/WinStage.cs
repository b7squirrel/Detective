using UnityEngine;

public class WinStage : MonoBehaviour
{
    [SerializeField] GameObject winStage;

    public void OpenPanel()
    {
        PlayerDataManager stageManager = FindObjectOfType<PlayerDataManager>();
        StageInfo stageinfo = FindObjectOfType<StageInfo>();
        int currentStage = stageManager.GetCurrentStageNumber();

        if (stageinfo.IsFinalStage(currentStage) == false)
        {
            currentStage++;
            stageManager.SetCurrentStageNumber(currentStage);
        }
        
        winStage.gameObject.SetActive(true);
    }
}