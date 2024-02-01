using UnityEngine;

public class WinStage : MonoBehaviour
{
    [SerializeField] GameObject winStage;
    [SerializeField] GameObject darkBG;

    // Victory Init 애니메이션 마지막 프레임에서 애님이벤트로 시간 정지 
    // Confirm 버튼을 클릭하면 시간을 다시 흐르게 한다
    public void OpenPanel()
    {
        GetComponent<PauseManager>().PauseGame();

        PlayerDataManager stageManager = FindObjectOfType<PlayerDataManager>();
        StageInfo stageinfo = FindObjectOfType<StageInfo>();
        int currentStage = stageManager.GetCurrentStageNumber();

        if (stageinfo.IsFinalStage(currentStage) == false)
        {
            currentStage++;
            stageManager.SetCurrentStageNumber(currentStage);
        }
        
        winStage.gameObject.SetActive(true);
        darkBG.gameObject.SetActive(true);
    }
}