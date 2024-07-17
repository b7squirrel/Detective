using UnityEngine;

public class WinStage : MonoBehaviour
{
    [SerializeField] GameObject winStage;
    [SerializeField] GameObject darkBG;

    // Victory Init 애니메이션 마지막 프레임에서 애님이벤트로 시간 정지 
    // Confirm 버튼을 클릭하면 시간을 다시 흐르게 한다
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