using UnityEngine;

public class WinStage : MonoBehaviour
{
    [SerializeField] GameObject winStage;

    // Victory Init 애니메이션 마지막 프레임에서 애님이벤트로 시간 정지 
    // Confirm 버튼을 클릭하면 시간을 다시 흐르게 한다
    public void OpenPanel()
    {
        winStage.SetActive(true);
        int killNum = GetComponent<KillManager>().GetCurrentKills();
        int coinNum = GetComponent<CoinManager>().GetCoinNumPickedup();
        int stageNum = FindObjectOfType<PlayerDataManager>().GetCurrentStageNumber() - 1;
        int killGold = GoldRewardManager.Instance.GetKillGold();
        int clearBonus = GoldRewardManager.Instance.GetClearBonus(stageNum);
        winStage.GetComponent<ResultPanel>().InitAwards(killNum, coinNum, stageNum, true, killGold, clearBonus);
        GetComponent<PauseManager>().PauseGame();
        Logger.Log("윈 스테이지");

        // 튜토리얼 Step 진행
        if (TutorialManager.instance != null &&
            TutorialManager.instance.CurrentStep == TutorialStep.Step0_OnlyBattle)
        {
            TutorialManager.instance.AdvanceStep();
        }

        // ✅ 추가: 무한모드 해금 체크
        if (UnlockConditionManager.Instance != null &&
            UnlockConditionManager.Instance.IsInfiniteModeUnlocked())
        {
            bool wasAlreadyUnlocked = PlayerDataManager.Instance.IsInfiniteModeUnlocked();
            PlayerDataManager.Instance?.UnlockInfiniteMode();

            // 이번 클리어로 처음 해금된 경우에만 배지 표시
            if (!wasAlreadyUnlocked)
            {
                FindObjectOfType<InfiniteModeButton>()?.OnInfiniteModeJustUnlocked();
            }
        }
    }
}