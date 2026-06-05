using UnityEngine;

/// <summary>
/// 스테이지 도달 기반 잠금 해제 조건을 중앙 관리합니다.
/// 현재: 무한모드 해금 (6스테이지 클리어)
/// 확장 시 이 파일에만 조건을 추가하면 됩니다.
/// </summary>
public class UnlockConditionManager : SingletonBehaviour<UnlockConditionManager>
{
    [Header("해금 조건")]
    [SerializeField] int infiniteModeUnlockStage = 6;

    // 나중에 추가될 조건은 여기에
    // [SerializeField] int newContentUnlockStage = 15;

    int CurrentStage => PlayerDataManager.Instance?.GetCurrentStageNumber() ?? 1;

    // ───────────────────────────────────────────
    //  조건 판별
    // ───────────────────────────────────────────

    public bool IsInfiniteModeUnlocked()
    {
        // PlayerData에 저장된 영구 플래그 우선 확인
        // (한 번 해금되면 스테이지가 리셋돼도 유지)
        if (PlayerDataManager.Instance != null &&
            PlayerDataManager.Instance.IsInfiniteModeUnlocked())
            return true;

        return CurrentStage >= infiniteModeUnlockStage;
    }

    // ───────────────────────────────────────────
    //  디버그
    // ───────────────────────────────────────────

#if UNITY_EDITOR
    [ContextMenu("Debug/무한모드 해금 상태 확인")]
    void DebugCheckInfinite()
    {
        Logger.Log($"[UnlockCondition] 현재 스테이지: {CurrentStage}, " +
                   $"해금 조건: {infiniteModeUnlockStage}, " +
                   $"해금 여부: {IsInfiniteModeUnlocked()}");
    }
#endif
}