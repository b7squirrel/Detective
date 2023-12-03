using UnityEngine;

public class WinStage : MonoBehaviour
{
    [SerializeField] GameObject winStage;

    public void OpenPanel()
    {
        StageManager stageManager = FindObjectOfType<StageManager>();
        StageInfo stageinfo = FindObjectOfType<StageInfo>();
        int currentStage = stageManager.GetCurrentStageNumber();

        if (stageinfo == null) Debug.Log("Stage info Null");
        if (stageinfo.IsFinalStage(currentStage) == false)
        {
            currentStage++;
            stageManager.SetCurrentStageNumber(currentStage);
        }
        
        Debug.Log("Current Stage = " + currentStage);
        winStage.gameObject.SetActive(true);
    }
}