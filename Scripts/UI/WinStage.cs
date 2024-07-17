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

        int killNum = GetComponent<KillManager>().GetCurrentKills();
        int coinNum = GetComponent<CoinManager>().GetCurrentCoins();

        winStage.GetComponent<VictoryPanel>().InitAwards(killNum, coinNum);

        GetComponent<PauseManager>().PauseGame();
    }
}