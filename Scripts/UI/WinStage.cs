using UnityEngine;

public class WinStage : MonoBehaviour
{
    [SerializeField] GameObject winStage;
    [SerializeField] GameObject darkBG;

    // Victory Init �ִϸ��̼� ������ �����ӿ��� �ִ��̺�Ʈ�� �ð� ���� 
    // Confirm ��ư�� Ŭ���ϸ� �ð��� �ٽ� �帣�� �Ѵ�
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
        darkBG.gameObject.SetActive(true);

        int killNum = GetComponent<KillManager>().GetCurrentKills();
        int coinNum = GetComponent<CoinManager>().GetCurrentCoins();

        winStage.GetComponent<VictoryPanel>().InitAwards(killNum, coinNum);
        GetComponent<PauseManager>().PauseGame();
    }
}