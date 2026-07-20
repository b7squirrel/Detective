using UnityEngine;

public class WinStage : MonoBehaviour
{
    [SerializeField] GameObject winStage;
    [SerializeField] GameObject allClearPopup;

    StageInfo stageInfo;
    int clearedStageNum;

    public void OpenPanel()
    {
        winStage.SetActive(true);
        int killNum = GetComponent<KillManager>().GetCurrentKills();
        int coinNum = GetComponent<CoinManager>().GetCoinNumPickedup();
        clearedStageNum = FindObjectOfType<PlayerDataManager>().GetCurrentStageNumber();
        int killGold = GoldRewardManager.Instance.GetKillGold();
        int clearBonus = GoldRewardManager.Instance.GetClearBonus(clearedStageNum);
        winStage.GetComponent<ResultPanel>().InitAwards(killNum, coinNum, clearedStageNum, true, killGold, clearBonus);
        GetComponent<PauseManager>().PauseGame();
        Logger.Log("윈 스테이지");

        // ⭐ 추가: 범용 스테이지 클리어 이벤트 (스테이지 번호 파라미터 포함)
        FirebaseManager.LogEvent("stage_clear", "stage_number", clearedStageNum.ToString());

        // ⭐ 업적 추가
        if (AchievementManager.Instance != null)
        {
            // 스테이지 클리어 누적 횟수
            AchievementManager.Instance.AddProgress(AchievementType.STAGE_CLEAR);

            // 몇 번째 스테이지까지 클리어했는지 (최고값 기록)
            AchievementManager.Instance.SetProgressIfGreaterNormal(AchievementType.STAGE_REACH, clearedStageNum);
        }

        if (TutorialManager.instance != null &&
            TutorialManager.instance.CurrentStep == TutorialStep.Step0_OnlyBattle)
        {
            TutorialManager.instance.AdvanceStep();
        }

        if (UnlockConditionManager.Instance != null &&
            UnlockConditionManager.Instance.IsInfiniteModeUnlocked())
        {
            bool wasAlreadyUnlocked = PlayerDataManager.Instance.IsInfiniteModeUnlocked();
            PlayerDataManager.Instance?.UnlockInfiniteMode();
            if (!wasAlreadyUnlocked)
            {
                FindObjectOfType<InfiniteModeButton>()?.OnInfiniteModeJustUnlocked();
            }
        }

        stageInfo = FindObjectOfType<StageInfo>();
    }

    // ⭐ Tap to Continue Button의 OnClick에 연결
    public void OnTapToContinue()
    {
        if (stageInfo != null && stageInfo.IsFinalStage(clearedStageNum))
        {
            allClearPopup.SetActive(true); // All Clear Popup 표시
        }
        else
        {
            FindObjectOfType<MainMenu>().GoToMainMenuAfter(0.33f); // 일반 스테이지
        }
    }

    // ⭐ Button Close의 OnClick에 연결
    public void CloseAllClearPopup()
    {
        // allClearPopup.SetActive(false);
        FindObjectOfType<MainMenu>().GoToMainMenuAfter(0.33f);
    }
}