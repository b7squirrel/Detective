using UnityEngine;

/// <summary>
/// 무한모드 전용 임무 진행도 추적
/// InfiniteStageManager와 같은 GameObject에 붙이거나 씬에 배치
/// </summary>
public class InfiniteMissionTracker : MonoBehaviour
{
    private InfiniteStageManager infiniteStageManager;

    private void Start()
    {
        infiniteStageManager = FindObjectOfType<InfiniteStageManager>();

        if (infiniteStageManager == null)
        {
            Logger.LogError("[InfiniteMissionTracker] InfiniteStageManager를 찾을 수 없습니다!");
            return;
        }

        // 웨이브 완료 이벤트 구독
        infiniteStageManager.OnWaveComplete += OnWaveComplete;

        Logger.Log("[InfiniteMissionTracker] 초기화 완료");
    }

    private void OnDestroy()
    {
        if (infiniteStageManager != null)
            infiniteStageManager.OnWaveComplete -= OnWaveComplete;
    }

    // 웨이브 클리어 시 호출
    private void OnWaveComplete(int waveNumber)
    {
        if (AchievementManager.Instance == null) return;

        AchievementManager.Instance.SetProgressIfGreater(AchievementType.WAVE, waveNumber);
        Logger.Log($"[InfiniteMissionTracker] WAVE 진행도 업데이트: {waveNumber}");
    }

    // 게임 종료 시 외부에서 호출 (PlayerDataManager.SaveInfiniteModeResources() 이후)
    public void OnGameEnd()
    {
        if (AchievementManager.Instance == null) return;
        if (infiniteStageManager == null) return;

        // 생존 시간 (초 단위 정수로 변환)
        int survivalSeconds = Mathf.FloorToInt(infiniteStageManager.GetSurvivalTime());
        AchievementManager.Instance.SetProgressIfGreater(AchievementType.SURVIVE, survivalSeconds);
        Logger.Log($"[InfiniteMissionTracker] SURVIVE 진행도 업데이트: {survivalSeconds}초");
    }
}