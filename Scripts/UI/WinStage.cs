using UnityEngine;

public class WinStage : MonoBehaviour
{
    [SerializeField] GameObject winStage;

    // Victory Init 애니메이션 마지막 프레임에서 애님이벤트로 시간 정지 
    // Confirm 버튼을 클릭하면 시간을 다시 흐르게 한다
    public void OpenPanel()
    {
        winStage.SetActive(true);
        GameManager.instance.darkBG.SetActive(true);

        // 스테이지에서 획득한 코인만 표시.
        int killNum = GetComponent<KillManager>().GetCurrentKills();
        int coinNum = GetComponent<CoinManager>().GetCoinNumPickedup();

        // 보스가 죽는 순간 이미 스테이지가 올라갔으니까 1을 빼준 수를 넘겨준다. 현재 스테이지 넘버를 표시하기 위함
        int stageNum = FindObjectOfType<PlayerDataManager>().GetCurrentStageNumber() - 1;

        winStage.GetComponent<ResultPanel>().InitAwards(killNum, coinNum, stageNum);

        GetComponent<PauseManager>().PauseGame();
    }
}