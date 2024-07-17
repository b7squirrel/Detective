using UnityEngine;

public class WinStage : MonoBehaviour
{
    [SerializeField] GameObject winStage;
    [SerializeField] GameObject darkBG;

    // Victory Init �ִϸ��̼� ������ �����ӿ��� �ִ��̺�Ʈ�� �ð� ���� 
    // Confirm ��ư�� Ŭ���ϸ� �ð��� �ٽ� �帣�� �Ѵ�
    public void OpenPanel()
    {
        winStage.gameObject.SetActive(true);
        darkBG.gameObject.SetActive(true);

        // ������������ ȹ���� ���θ� ǥ��.
        int killNum = GetComponent<KillManager>().GetCurrentKills();
        int coinNum = GetComponent<CoinManager>().GetCoinNumPickedup();

        // ������ �״� ���� �̹� ���������� �ö����ϱ� 1�� ���� ���� �Ѱ��ش�.
        int stageNum = FindObjectOfType<PlayerDataManager>().GetCurrentStageNumber() - 1;

        winStage.GetComponent<VictoryPanel>().InitAwards(killNum, coinNum, stageNum);

        GetComponent<PauseManager>().PauseGame();
    }
}