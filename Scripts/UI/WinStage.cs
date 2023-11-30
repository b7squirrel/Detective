using UnityEngine;

public class WinStage : MonoBehaviour
{
    [SerializeField] GameObject winStage;

    public void OpenPanel()
    {
        StageManager stageManager = FindObjectOfType<StageManager>();
        int currentStage = stageManager.GetCurrentStageNumber();
        currentStage++;
        stageManager.SetCurrentStageNumber(currentStage);
        Debug.Log("Current Stage = " + currentStage);
        winStage.gameObject.SetActive(true);
    }
}