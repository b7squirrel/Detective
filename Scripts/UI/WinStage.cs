using UnityEngine;

public class WinStage : MonoBehaviour
{
    [SerializeField] GameObject winStage;
    [SerializeField] GameObject darkBG;

    // Victory Init �ִϸ��̼� ������ �����ӿ��� �ִ��̺�Ʈ�� �ð� ���� 
    // Confirm ��ư�� Ŭ���ϸ� �ð��� �ٽ� �帣�� �Ѵ�
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