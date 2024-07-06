using UnityEngine;

public class ResetStageProgress : MonoBehaviour
{
    PlayerDataManager playerDataManager;
    public void CLearStageProgress()
    {
        if (playerDataManager == null)
            playerDataManager = FindObjectOfType<PlayerDataManager>();

        playerDataManager.SetCurrentStageNumber(1);
        Debug.Log("Stage number = " + playerDataManager.GetCurrentStageNumber());
    }
    public void GetStageNumber()
    {
        if (playerDataManager == null)
            playerDataManager = FindObjectOfType<PlayerDataManager>();
        Debug.Log("Stage number is " + playerDataManager.GetCurrentStageNumber());
    }
    public void NextStage()
    {
        if (playerDataManager == null)
            playerDataManager = FindObjectOfType<PlayerDataManager>();

        int index = playerDataManager.GetCurrentStageNumber() + 1;
        playerDataManager.SetCurrentStageNumber(index);
    }
    public void PreviousStage()
    {
        if (playerDataManager == null)
            playerDataManager = FindObjectOfType<PlayerDataManager>();

        int index = playerDataManager.GetCurrentStageNumber() - 1;
        playerDataManager.SetCurrentStageNumber(index);
    }
}