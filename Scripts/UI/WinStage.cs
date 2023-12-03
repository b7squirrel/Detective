using UnityEngine;

public class WinStage : MonoBehaviour
{
    [SerializeField] GameObject winStage;

    public void OpenPanel()
    {
        StageManager stageManager = FindObjectOfType<StageManager>();
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