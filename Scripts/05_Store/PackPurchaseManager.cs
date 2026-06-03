using UnityEngine;

/// <summary>
/// 초보자/전문가 팩의 1회 한정 구매를 관리합니다.
///
/// [초보자 팩] 계정 영구 1회 — PlayerPrefs + 클라우드 저장
/// [전문가 팩] 스테이지 조건 달성 후 영구 1회 — PlayerPrefs + 클라우드 저장
///
/// 사용법:
///   ShopManager에서 PurchaseProduct() 호출 전 CanPurchasePack()으로 먼저 체크.
///   구매 성공 후 OnPackPurchased() 호출.
/// </summary>
public class PackPurchaseManager : SingletonBehaviour<PackPurchaseManager>
{
    // PlayerPrefs 키
    const string KEY_STARTER_PURCHASED = "Pack_StarterPurchased";
    const string KEY_PRO_PURCHASED = "Pack_ProPurchased";

    // 전문가 팩 해금 조건 — 이 스테이지 번호 이상 도달해야 구매 가능
    // PlayerData.currentStageNumber 기준 (1-based)
    // 예: 10 = 스테이지 10 이상 도달 시 해금
    [SerializeField] int proPackUnlockStageNumber = 10;

    // ───────────────────────────────────────────
    //  외부에서 호출하는 주요 메서드
    // ───────────────────────────────────────────

    /// <summary>
    /// 해당 팩을 구매할 수 있는지 확인합니다.
    /// 불가 사유도 out으로 반환합니다.
    /// </summary>
    public bool CanPurchasePack(string productId, out string reason)
    {
        reason = "";

        if (productId == "pack_001") // 초보자 팩
        {
            if (IsStarterPackPurchased())
            {
                reason = "이미 구매한 상품입니다.";
                return false;
            }
            return true;
        }

        if (productId == "pack_003") // 전문가 팩
        {
            if (!IsProPackUnlocked())
            {
                reason = $"스테이지 {proPackUnlockStageNumber} 도달 후 구매 가능합니다.";
                return false;
            }
            if (IsProPackPurchased())
            {
                reason = "이미 구매한 상품입니다.";
                return false;
            }
            return true;
        }

        // 팩이 아닌 상품은 항상 통과
        return true;
    }

    /// <summary>
    /// 팩 구매 완료 후 호출합니다. (ShopManager에서 호출)
    /// </summary>
    public void OnPackPurchased(string productId)
    {
        if (productId == "pack_001")
        {
            PlayerPrefs.SetInt(KEY_STARTER_PURCHASED, 1);
            PlayerPrefs.Save();
            CloudSaveManager.Instance?.SaveToCloud();
            Logger.Log("[PackPurchaseManager] 초보자 팩 구매 완료 저장");
        }
        else if (productId == "pack_003")
        {
            PlayerPrefs.SetInt(KEY_PRO_PURCHASED, 1);
            PlayerPrefs.Save();
            CloudSaveManager.Instance?.SaveToCloud();
            Logger.Log("[PackPurchaseManager] 전문가 팩 구매 완료 저장");
        }
    }

    // ───────────────────────────────────────────
    //  상태 조회
    // ───────────────────────────────────────────

    public bool IsStarterPackPurchased()
        => PlayerPrefs.GetInt(KEY_STARTER_PURCHASED, 0) == 1;

    public bool IsProPackPurchased()
        => PlayerPrefs.GetInt(KEY_PRO_PURCHASED, 0) == 1;

    /// <summary>
    /// 전문가 팩 해금 조건 달성 여부.
    /// PlayerDataManager.currentStageNumber가 조건 이상이면 해금.
    /// </summary>
    public bool IsProPackUnlocked()
    {
        if (PlayerDataManager.Instance == null)
        {
            Logger.LogWarning("[PackPurchaseManager] PlayerDataManager가 없습니다.");
            return false;
        }

        int currentStage = PlayerDataManager.Instance.GetCurrentStageNumber();
        return currentStage >= proPackUnlockStageNumber;
    }

    // ───────────────────────────────────────────
    //  클라우드 동기화용 — CloudSaveData 연동
    //  CloudSaveData에 아래 두 필드를 추가해야 합니다:
    //    public bool starterPackPurchased;
    //    public bool proPackPurchased;
    // ───────────────────────────────────────────

    /// <summary>
    /// 클라우드에서 복원된 데이터를 로컬에 적용합니다.
    /// CloudSaveManager.ApplyAllCloudData()에서 호출하세요.
    /// </summary>
    public void ApplyFromCloud(bool starterPurchased, bool proPurchased)
    {
        // 클라우드가 true면 로컬에도 반영 (한 번 구매하면 되돌릴 수 없음)
        if (starterPurchased)
        {
            PlayerPrefs.SetInt(KEY_STARTER_PURCHASED, 1);
            Logger.Log("[PackPurchaseManager] 클라우드에서 초보자 팩 구매 기록 복원");
        }
        if (proPurchased)
        {
            PlayerPrefs.SetInt(KEY_PRO_PURCHASED, 1);
            Logger.Log("[PackPurchaseManager] 클라우드에서 전문가 팩 구매 기록 복원");
        }
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 현재 구매 상태를 클라우드 저장용으로 반환합니다.
    /// CloudSaveManager.BuildSaveData()에서 호출하세요.
    /// </summary>
    public (bool starter, bool pro) GetPurchaseStateForCloud()
        => (IsStarterPackPurchased(), IsProPackPurchased());

    // ───────────────────────────────────────────
    //  디버그용
    // ───────────────────────────────────────────

    public void DebugResetPacksForTest()
    {
        PlayerPrefs.DeleteKey("Pack_StarterPurchased");
        PlayerPrefs.DeleteKey("Pack_ProPurchased");
        PlayerPrefs.Save();
        Logger.Log("[PackPurchaseManager] 팩 구매 기록 초기화 완료 (테스트용)");
    }

#if UNITY_EDITOR
    [ContextMenu("Debug/Reset All Pack Purchases")]
    void DebugResetAll()
    {
        PlayerPrefs.DeleteKey(KEY_STARTER_PURCHASED);
        PlayerPrefs.DeleteKey(KEY_PRO_PURCHASED);
        PlayerPrefs.Save();
        Logger.Log("[PackPurchaseManager] 팩 구매 기록 초기화 완료");
    }

    [ContextMenu("Debug/Force Unlock Pro Pack")]
    void DebugUnlockPro()
    {
        Logger.Log($"[PackPurchaseManager] 현재 스테이지: {PlayerDataManager.Instance?.GetCurrentStageNumber()}, 해금 조건: {proPackUnlockStageNumber}");
        Logger.Log($"[PackPurchaseManager] 해금 여부: {IsProPackUnlocked()}");
    }
#endif
}