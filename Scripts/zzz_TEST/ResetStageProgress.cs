using UnityEngine;

public class ResetStageProgress : MonoBehaviour
{
    public void CLearStageProgress()
    {
        StageManager stageManager = FindObjectOfType<StageManager>();
        stageManager.SetCurrentStageNumber(1);
        Debug.Log("Stage number = " + stageManager.GetCurrentStageNumber());
    }
    public void GetStageNumber()
    {
        StageManager stageManager = FindObjectOfType<StageManager>();
        Debug.Log("Stage number is " + stageManager.GetCurrentStageNumber());
    }
}