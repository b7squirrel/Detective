using UnityEngine;

public class ResetStageProgress : MonoBehaviour
{
    public void CLearStageProgress()
    {
        PlayerDataManager stageManager = FindObjectOfType<PlayerDataManager>();
        stageManager.SetCurrentStageNumber(1);
        Debug.Log("Stage number = " + stageManager.GetCurrentStageNumber());
    }
    public void GetStageNumber()
    {
        PlayerDataManager stageManager = FindObjectOfType<PlayerDataManager>();
        Debug.Log("Stage number is " + stageManager.GetCurrentStageNumber());
    }
}