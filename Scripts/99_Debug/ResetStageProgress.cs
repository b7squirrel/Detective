using UnityEngine;

public class ResetStageProgress : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI currentStageNum;
    PlayerDataManager playerDataManager;
    StageInfo stageInfo;
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

        if(stageInfo == null) stageInfo = FindObjectOfType<StageInfo>();

        
        int index = playerDataManager.GetCurrentStageNumber() + 1;
        if (index > stageInfo.GetMaxStage())
        {
            index = stageInfo.GetMaxStage();
        }
            playerDataManager.SetCurrentStageNumber(index);
    }
    public void PreviousStage()
    {
        if (playerDataManager == null)
            playerDataManager = FindObjectOfType<PlayerDataManager>();

        int index = playerDataManager.GetCurrentStageNumber() - 1;
        if (index < 1) index = 1;
        playerDataManager.SetCurrentStageNumber(index);
    }

    public void UpdateCurrentStageNumberUI()
    {
        // if (playerDataManager == null)
        //     playerDataManager = FindObjectOfType<PlayerDataManager>();
        // currentStageNum.text = playerDataManager.GetCurrentStageNumber().ToString();
    }
}