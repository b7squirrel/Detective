using System.Collections.Generic;
using UnityEngine;
using System;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance;

    [Header("Resources 폴더 경로 (예: Resources/Achievements)")]
    [SerializeField] private string resourcePath = "Achievements";

    // ⭐ 배지 저장 관련 (업적 진행도와 완전히 별개 저장소)
    private const string BADGE_SAVE_KEY = "EARNED_BADGES";
    private HashSet<string> earnedBadgeIds = new HashSet<string>();

    // ⭐ "배지를 이미 본 적 있는지" 기록 (로비 반짝임 애니메이션 1회 재생용, 클라우드 동기화 안 함 - 로컬 전용)
    private const string BADGE_SEEN_KEY = "SEEN_BADGES";
    private HashSet<string> seenBadgeIds = new HashSet<string>();

    // 자동 로드된 업적 리스트
    public List<AchievementSO> achievementSOList = new();

    // 런타임 저장소
    public Dictionary<string, RuntimeAchievement> runtimeDict = new();

    // 글로벌 이벤트
    public event Action<RuntimeAchievement> OnAnyProgressChanged;
    public event Action<RuntimeAchievement> OnAnyCompleted;
    public event Action<RuntimeAchievement> OnAnyRewarded;

    [SerializeField] private GemCollectFX gemCollectFX;
    PlayerDataManager playerDataManager; // 크리스탈의 실제 값을 더해주기 위해

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        // ⭐ 일일 리셋 이벤트 구독
        DailyResetManager.OnDailyReset += ResetDailyQuests;
        WeeklyResetManager.OnWeeklyReset += ResetWeeklyQuests;
    }

    private void OnDisable()
    {
        // ⭐ 이벤트 구독 해제
        DailyResetManager.OnDailyReset -= ResetDailyQuests;
        WeeklyResetManager.OnWeeklyReset -= ResetWeeklyQuests;
    }

    private void Initialize()
    {
        LoadAllSO();

        runtimeDict.Clear();

        LoadEarnedBadges(); // ⭐ 추가: 영구 저장된 배지 목록 로드

        LoadSeenBadges(); // ⭐ 추가

        foreach (var so in achievementSOList)
        {
            if (runtimeDict.ContainsKey(so.id))
            {
                Debug.LogError($"[AchievementManager] 중복 업적 ID: {so.id}");
                continue;
            }

            RuntimeAchievement ra = new RuntimeAchievement(so);

            ra.OnProgressChanged += r => OnAnyProgressChanged?.Invoke(r);
            ra.OnCompleted += r => OnAnyCompleted?.Invoke(r);

            runtimeDict.Add(so.id, ra);
        }

        // ⭐ 추가: 오늘 이미 리셋했는지 확인, 안 했으면 일일 퀘스트 초기화, 주간 퀘스트 초기화
        ValidateDailyQuestState();
        ValidateWeeklyQuestState();

        if (gemCollectFX == null)
            gemCollectFX = FindObjectOfType<GemCollectFX>();

        // 디버그: 일일 퀘스트 목록 출력
        Logger.Log($"[AchievementManager] 총 업적: {runtimeDict.Count}개");
        Logger.Log($"[AchievementManager] 일일 퀘스트: {GetDailyQuests().Count}개");
        Logger.Log($"[AchievementManager] 영구 업적: {GetPermanentAchievements().Count}개");

        foreach (var daily in GetDailyQuests())
        {
            Logger.Log($"  - {daily.GetTitle()}: {daily.GetDescription()}");
        }
    }

    void ValidateDailyQuestState()
    {
        string today = DailyResetManager.GetTodayString();
        string lastResetDate = PlayerPrefs.GetString("DAILY_QUEST_LAST_RESET", "");

        if (lastResetDate != today)
        {
            // 오늘 아직 리셋 안 됐으면 강제 초기화
            foreach (var ra in runtimeDict.Values)
            {
                if (!ra.original.isDailyQuest) continue;

                ra.progress = 0;
                ra.isCompleted = false;
                ra.isRewarded = false;

                PlayerPrefs.SetInt(ra.GetProgressKey(), 0);
                PlayerPrefs.SetInt(ra.GetCompleteKey(), 0);
                PlayerPrefs.SetInt(ra.GetRewardKey(), 0);
            }

            PlayerPrefs.SetString("DAILY_QUEST_LAST_RESET", today);
            PlayerPrefs.Save();

            Logger.Log("[AchievementManager] 일일 퀘스트 상태 초기화 완료");
        }
    }

    // ★ Resources 폴더에서 AchievementSO 자동 로딩
    private void LoadAllSO()
    {
        achievementSOList.Clear();

        AchievementSO[] loaded = Resources.LoadAll<AchievementSO>(resourcePath);

        if (loaded.Length == 0)
            Debug.LogWarning($"[AchievementManager] Resources/{resourcePath} 에 업적이 없습니다.");

        achievementSOList.AddRange(loaded);
    }


    public void AddProgressByID(string id, int amount = 1)
    {
        if (runtimeDict.TryGetValue(id, out var ra))
        {
            ra.AddProgress(amount);
            SaveAchievement(ra);
        }
    }

    // 기존 메서드 - 무한모드 임무는 제외
    public void AddProgress(AchievementType type, int amount = 1)
    {
        foreach (var ra in runtimeDict.Values)
        {
            if (ra.original.type != type) continue;
            if (ra.original.isInfiniteMode) continue; // ← 추가
            if (ra.isCompleted) continue;

            ra.AddProgress(amount);
            SaveAchievement(ra);
        }
    }

    // 무한모드 전용 - 새로 추가
    public void AddProgressInfinite(AchievementType type, int amount = 1)
    {
        foreach (var ra in runtimeDict.Values)
        {
            if (ra.original.type != type) continue;
            if (!ra.original.isInfiniteMode) continue; // ← 무한모드만
            if (ra.isCompleted) continue;

            ra.AddProgress(amount);
            SaveAchievement(ra);
        }
    }

    public void Reward(string id, RectTransform pos, RewardType rewardType)
    {
        Logger.Log($"[Reward] 호출됨 - id: {id}");

        if (!runtimeDict.TryGetValue(id, out var ra))
        {
            Logger.Log($"[Reward] ID를 찾을 수 없음: {id}");
            return;
        }
        if (ra.isRewarded)
        {
            Logger.Log($"[Reward] 이미 수령함: {id}");
            return;
        }

        // ⭐ pos null 체크
        if (pos == null)
        {
            Logger.LogError("[AchievementManager] effectStartPos가 null입니다!");
            return;
        }

        ra.Reward();
        SaveAchievement(ra);

        // ⭐ 배지: 저장을 이벤트 발생보다 먼저 처리 (BadgeDisplayManager 등 구독자가
        //         이벤트를 받는 시점엔 earnedBadgeIds에 이미 반영되어 있어야 함)
        if (rewardType == RewardType.BADGE)
        {
            if (ra.original.isDailyQuest || ra.original.isWeeklyQuest)
            {
                Logger.LogWarning($"[Reward] 일일/주간 퀘스트는 배지 보상으로 설계되지 않았습니다: {id}");
            }
            if (earnedBadgeIds.Add(id))
            {
                SaveEarnedBadges();
                Logger.Log($"[Reward] 배지 획득: {id} (카테고리: {ra.original.badgeCategory})");
            }
            OnAnyRewarded?.Invoke(ra);
            return;
        }

        // ⭐ 배지가 아닌 경우(GEM/COIN/ENERGY)에도 반드시 여기서 이벤트 발생
        OnAnyRewarded?.Invoke(ra);

        if (playerDataManager == null)
            playerDataManager = FindObjectOfType<PlayerDataManager>();

        // 매번 재탐색
        if (gemCollectFX == null)
            gemCollectFX = FindObjectOfType<GemCollectFX>();

        if (gemCollectFX == null)
        {
            Logger.LogError("[AchievementManager] GemCollectFX를 찾을 수 없습니다!");
            return;
        }

        if (rewardType == RewardType.GEM)
        {
            int currentValue = playerDataManager.GetCurrentCristalNumber();
            playerDataManager.SetCristalNumberAsSilent(currentValue + ra.original.rewardNum);
            gemCollectFX.PlayGemCollectFX(pos, ra.original.rewardNum, true);
        }
        else if (rewardType == RewardType.ENERGY)
        {
            playerDataManager.AddLightningSilent(ra.original.rewardNum);
            gemCollectFX.PlayLightningCollectFX(pos, ra.original.rewardNum);
        }
        else // COIN
        {
            int currentValue = playerDataManager.GetCurrentCoinNumber();
            playerDataManager.SetCoinNumberAsSilent(currentValue + ra.original.rewardNum);
            gemCollectFX.PlayGemCollectFX(pos, ra.original.rewardNum, false);
        }
    }

    public void SaveAchievement(RuntimeAchievement ra)
    {
        // ⭐ 동적 키 사용
        PlayerPrefs.SetInt(ra.GetCompleteKey(), ra.isCompleted ? 1 : 0);
        PlayerPrefs.SetInt(ra.GetProgressKey(), ra.progress);
        PlayerPrefs.SetInt(ra.GetRewardKey(), ra.isRewarded ? 1 : 0);
    }

    public List<RuntimeAchievement> GetAll()
    {
        return new List<RuntimeAchievement>(runtimeDict.Values);
    }

    // ⭐ 일일 퀘스트만 가져오기
    public List<RuntimeAchievement> GetDailyQuests()
    {
        List<RuntimeAchievement> dailyQuests = new();
        foreach (var ra in runtimeDict.Values)
        {
            if (ra.original.isDailyQuest)
                dailyQuests.Add(ra);
        }
        return dailyQuests;
    }

    // ⭐ 영구 업적만 가져오기
    public List<RuntimeAchievement> GetPermanentAchievements()
    {
        List<RuntimeAchievement> permanentAchievements = new();
        foreach (var ra in runtimeDict.Values)
        {
            if (!ra.original.isDailyQuest)
                permanentAchievements.Add(ra);
        }
        return permanentAchievements;
    }

    // ⭐ 일일 퀘스트 리셋 (매일 자정 호출됨)
    public void ResetDailyQuests()
    {
        Logger.Log("[AchievementManager] 일일 퀘스트 리셋 시작");

        foreach (var ra in runtimeDict.Values)
        {
            // 일일 퀘스트만 리셋
            if (ra.original.isDailyQuest)
            {
                ra.progress = 0;
                ra.isCompleted = false;
                ra.isRewarded = false;

                // PlayerPrefs에 저장
                PlayerPrefs.SetInt(ra.GetCompleteKey(), 0);
                PlayerPrefs.SetInt(ra.GetProgressKey(), 0);
                PlayerPrefs.SetInt(ra.GetRewardKey(), 0);

                // UI 갱신 이벤트
                OnAnyProgressChanged?.Invoke(ra);
            }
        }

        PlayerPrefs.SetString("DAILY_QUEST_LAST_RESET", DailyResetManager.GetTodayString());
        PlayerPrefs.Save();
        Logger.Log("[AchievementManager] 일일 퀘스트 리셋 완료");
    }

    // ⭐ 주간 퀘스트만 가져오기
    public List<RuntimeAchievement> GetWeeklyQuests()
    {
        List<RuntimeAchievement> list = new();
        foreach (var ra in runtimeDict.Values)
        {
            if (ra.original.isWeeklyQuest)
                list.Add(ra);
        }
        return list;
    }

    // ⭐ 미수령 완료 주간 퀘스트만 가져오기
    public List<RuntimeAchievement> GetUnclaimedCompletedWeeklyQuests()
    {
        List<RuntimeAchievement> list = new();
        foreach (var ra in runtimeDict.Values)
        {
            if (ra.original.isWeeklyQuest && ra.isCompleted && !ra.isRewarded)
                list.Add(ra);
        }
        return list;
    }

    // ⭐ 주간 퀘스트 리셋
    public void ResetWeeklyQuests()
    {
        Logger.Log("[AchievementManager] 주간 퀘스트 리셋 시작");

        foreach (var ra in runtimeDict.Values)
        {
            if (!ra.original.isWeeklyQuest) continue;

            ra.progress = 0;
            ra.isCompleted = false;
            ra.isRewarded = false;

            PlayerPrefs.SetInt(ra.GetCompleteKey(), 0);
            PlayerPrefs.SetInt(ra.GetProgressKey(), 0);
            PlayerPrefs.SetInt(ra.GetRewardKey(), 0);

            OnAnyProgressChanged?.Invoke(ra);
        }

        PlayerPrefs.SetString("WEEKLY_QUEST_LAST_RESET", WeeklyResetManager.GetCurrentWeekString());
        PlayerPrefs.Save();

        Logger.Log("[AchievementManager] 주간 퀘스트 리셋 완료");
    }

    // ⭐ 주간 퀘스트 상태 검증
    private void ValidateWeeklyQuestState()
    {
        string currentWeek = WeeklyResetManager.GetCurrentWeekString();
        string lastResetWeek = PlayerPrefs.GetString("WEEKLY_QUEST_LAST_RESET", "");

        if (lastResetWeek != currentWeek)
        {
            foreach (var ra in runtimeDict.Values)
            {
                if (!ra.original.isWeeklyQuest) continue;

                ra.progress = 0;
                ra.isCompleted = false;
                ra.isRewarded = false;

                PlayerPrefs.SetInt(ra.GetProgressKey(), 0);
                PlayerPrefs.SetInt(ra.GetCompleteKey(), 0);
                PlayerPrefs.SetInt(ra.GetRewardKey(), 0);
            }

            PlayerPrefs.SetString("WEEKLY_QUEST_LAST_RESET", currentWeek);
            PlayerPrefs.Save();

            Logger.Log("[AchievementManager] 주간 퀘스트 상태 초기화 완료");
        }
    }

    // WAVE, SURVIVE 타입용 - 현재값보다 클 때만 업데이트
    public void SetProgressIfGreater(AchievementType type, int value)
    {
        foreach (var ra in runtimeDict.Values)
        {
            if (ra.original.type != type) continue;
            if (!ra.original.isInfiniteMode) continue;
            if (ra.isCompleted) continue;

            if (value > ra.progress)
            {
                ra.SetProgressIfGreater(value); // ← RuntimeAchievement 내부에서 이벤트 호출
                SaveAchievement(ra);
            }
        }
    }

    // 일반 모드용 - isInfiniteMode 체크 없이 최고값 업데이트
    public void SetProgressIfGreaterNormal(AchievementType type, int value)
    {
        foreach (var ra in runtimeDict.Values)
        {
            if (ra.original.type != type) continue;
            if (ra.isCompleted) continue;

            if (value > ra.progress)
            {
                ra.SetProgressIfGreater(value);
                SaveAchievement(ra);
            }
        }
    }

    // 생존 시간 누적용 - 일반/무한모드 분리
    public void AddSurviveMinutes(int minutes, bool isInfiniteMode)
    {
        foreach (var ra in runtimeDict.Values)
        {
            if (ra.original.type != AchievementType.SURVIVE) continue;
            if (ra.original.isInfiniteMode != isInfiniteMode) continue;
            if (ra.isCompleted) continue;

            ra.AddProgress(minutes);
            SaveAchievement(ra);
        }
    }

    #region 배지
    void LoadEarnedBadges()
    {
        earnedBadgeIds.Clear();
        string csv = PlayerPrefs.GetString(BADGE_SAVE_KEY, "");
        if (string.IsNullOrEmpty(csv)) return;
        foreach (var id in csv.Split(','))
        {
            if (!string.IsNullOrEmpty(id)) earnedBadgeIds.Add(id);
        }
    }

    void SaveEarnedBadges()
    {
        PlayerPrefs.SetString(BADGE_SAVE_KEY, string.Join(",", earnedBadgeIds));
        PlayerPrefs.Save();
    }

    // ===== #region 배지 안, LoadEarnedBadges()/SaveEarnedBadges() 근처에 추가 =====

// ⭐ 디버그용: 배지 관련 모든 기록을 초기화 (받은 배지, 본 배지, 배지 업적 진행도까지 전부)
[ContextMenu("Debug 플레이모드 : Reset All Badges")]
public void DebugResetAllBadges()
{
    if (!Application.isPlaying)
    {
        Logger.LogWarning("[Debug] 플레이 모드에서만 실행 가능합니다.");
        return;
    }

    // 1. 받은 배지 목록 초기화
    earnedBadgeIds.Clear();
    SaveEarnedBadges();

    // 2. 본 배지(반짝임 재생 여부) 목록 초기화
    seenBadgeIds.Clear();
    SaveSeenBadges();

    // 3. 배지 업적 자체의 진행도/완료/수령 상태 초기화
    int count = 0;
    foreach (var ra in runtimeDict.Values)
    {
        if (ra.original.rewardType != RewardType.BADGE) continue;

        ra.progress = 0;
        ra.isCompleted = false;
        ra.isRewarded = false;
        PlayerPrefs.SetInt(ra.GetProgressKey(), 0);
        PlayerPrefs.SetInt(ra.GetCompleteKey(), 0);
        PlayerPrefs.SetInt(ra.GetRewardKey(), 0);
        count++;
    }
    PlayerPrefs.Save();

    // 4. 업적 탭 UI를 통째로 다시 그림 (기존 ResetAllAchievements()와 동일한 패턴)
    AchievementPanel panel = FindObjectOfType<AchievementPanel>(true);
    if (panel != null) panel.ReinitializeAll();

    // 5. 로비의 배지 나열도 즉시 갱신 (지금 로비 화면이 켜져 있다면)
    BadgeDisplayManager badgeDisplay = FindObjectOfType<BadgeDisplayManager>(true);
    if (badgeDisplay != null) badgeDisplay.Refresh();

    Logger.Log($"[Debug] 배지 {count}개 완전 초기화 완료 (받은 배지 / 본 배지 / 업적 진행도 전부 리셋).");
}

    void LoadSeenBadges()
    {
        seenBadgeIds.Clear();
        string csv = PlayerPrefs.GetString(BADGE_SEEN_KEY, "");
        if (string.IsNullOrEmpty(csv)) return;
        foreach (var id in csv.Split(','))
        {
            if (!string.IsNullOrEmpty(id)) seenBadgeIds.Add(id);
        }
    }

    void SaveSeenBadges()
    {
        PlayerPrefs.SetString(BADGE_SEEN_KEY, string.Join(",", seenBadgeIds));
        PlayerPrefs.Save();
    }

    // BadgeDisplayManager가 반짝임 애니메이션을 재생할지 판단할 때 사용
    public bool IsBadgeSeen(string badgeId)
    {
        return seenBadgeIds.Contains(badgeId);
    }

    // BadgeDisplayManager가 반짝임을 재생한 직후 호출 (다음부터는 idle로만 보이게)
    public void MarkBadgeSeen(string badgeId)
    {
        if (seenBadgeIds.Add(badgeId))
        {
            SaveSeenBadges();
        }
    }

    // Character.cs의 ApplyBadgeBonus()가 스탯 계산할 때 사용
    public int GetBadgeCount(BadgeCategory category)
    {
        int count = 0;
        foreach (var id in earnedBadgeIds)
        {
            if (runtimeDict.TryGetValue(id, out var ra) && ra.original.badgeCategory == category)
                count++;
        }
        return count;
    }

    // 로비 훈장 UI가 아이콘 나열할 때 사용
    public List<AchievementSO> GetEarnedBadges()
    {
        List<AchievementSO> list = new();
        foreach (var id in earnedBadgeIds)
        {
            if (runtimeDict.TryGetValue(id, out var ra))
                list.Add(ra.original);
        }
        return list;
    }

    // ⭐ CloudSaveManager가 업로드할 때 사용
    public List<string> GetEarnedBadgeIds()
    {
        return new List<string>(earnedBadgeIds);
    }

    // ⭐ CloudSaveManager가 다운로드 후 합집합 병합할 때 사용
    // 배지는 절대 줄어들면 안 되므로 "덮어쓰기"가 아니라 "합치기"
    public void MergeEarnedBadgesFromCloud(IEnumerable<string> cloudBadgeIds)
    {
        if (cloudBadgeIds == null) return;
        bool changed = false;
        foreach (var id in cloudBadgeIds)
        {
            if (string.IsNullOrEmpty(id)) continue;
            if (earnedBadgeIds.Add(id)) changed = true;
        }
        if (changed) SaveEarnedBadges();
    }
    #endregion

    // ⭐ 모든 업적 리셋 (디버그용)
    [ContextMenu("Debug 플레이모드 : Reset All Achievements")]
    public void ResetAllAchievements()
    {
        // ✅ 플레이 모드 아닐 때 호출 방지
        if (!Application.isPlaying)
        {
            Logger.LogWarning("[Reset] 플레이 모드에서만 실행 가능합니다.");
            return;
        }

        // ✅ 항상 실제 싱글톤 Instance를 사용
        AchievementManager target = Instance != null ? Instance : this;

        Logger.Log($"[Reset] 사용 인스턴스: {(target == this ? "this (씬)" : "Instance (DontDestroy)")}");
        Logger.Log($"[Reset] runtimeDict 총 {target.runtimeDict.Count}개");

        foreach (var ra in target.runtimeDict.Values)
        {
            ra.progress = 0;
            ra.isCompleted = false;
            ra.isRewarded = false;

            PlayerPrefs.SetInt(ra.GetProgressKey(), 0);
            PlayerPrefs.SetInt(ra.GetCompleteKey(), 0);
            PlayerPrefs.SetInt(ra.GetRewardKey(), 0);

            target.OnAnyProgressChanged?.Invoke(ra);
        }
        PlayerPrefs.Save();

        Logger.Log($"[Reset] tutorial_merge 존재: {target.runtimeDict.ContainsKey("tutorial_merge")}");

        AchievementPanel panel = FindObjectOfType<AchievementPanel>(true);
        Logger.Log($"[Reset] AchievementPanel 찾음: {panel != null}");
        if (panel != null) panel.ReinitializeAll();
    }

    // ===== AchievementManager.cs 아무 곳에나 추가 (기존 ResetAllAchievements() 근처 추천) =====

    // ⭐ 디버그용: 배지 업적 전부를 "완료" 상태로 만듦 (보상은 받지 않은 채로 남겨둠)
    // 업적 탭에서 하나씩 직접 보상 버튼을 눌러 팝업/로비 애니메이션을 테스트할 수 있게 하기 위함
    [ContextMenu("Debug 플레이모드 : Complete All Badge Achievements (미수령 상태로)")]
    public void DebugCompleteAllBadgeAchievements()
    {
        if (!Application.isPlaying)
        {
            Logger.LogWarning("[Debug] 플레이 모드에서만 실행 가능합니다.");
            return;
        }

        int count = 0;
        foreach (var ra in runtimeDict.Values)
        {
            if (ra.original.rewardType != RewardType.BADGE) continue;
            if (ra.isRewarded) continue;
            if (ra.isCompleted) continue;

            // ⭐ 변경: 필드 직접 대입 대신 AddProgress()를 호출해서
            //         ra.OnCompleted / ra.OnProgressChanged 이벤트가 정상 플레이와 동일하게 발생하도록 함
            //         (AchievementItemUI가 이 이벤트를 구독해서 체크 표시/버튼 활성화를 처리함)
            int remaining = ra.original.targetValue - ra.progress;
            if (remaining > 0)
            {
                ra.AddProgress(remaining);
            }

            SaveAchievement(ra);
            count++;
        }

        Logger.Log($"[Debug] 배지 업적 {count}개를 완료 상태로 만들었습니다 (보상은 미수령 - 업적 탭에서 직접 받아서 테스트).");
    }

    // 플레이 모드에서 AchievementManager 게임오브젝트를 선택하고 Inspector 우측 상단 ⋮ → Debug: Reset AD_DRAW Progress 로 실행
    [ContextMenu("Debug: Reset AD_DRAW Progress")]
    public void ResetAdDrawProgress()
    {
        if (!Application.isPlaying)
        {
            Logger.LogWarning("[Reset] 플레이 모드에서만 실행 가능합니다.");
            return;
        }

        foreach (var ra in runtimeDict.Values)
        {
            if (ra.original.type != AchievementType.AD_DRAW) continue;

            ra.progress = 0;
            ra.isCompleted = false;
            ra.isRewarded = false;

            PlayerPrefs.SetInt(ra.GetProgressKey(), 0);
            PlayerPrefs.SetInt(ra.GetCompleteKey(), 0);
            PlayerPrefs.SetInt(ra.GetRewardKey(), 0);

            OnAnyProgressChanged?.Invoke(ra);
        }

        PlayerPrefs.Save();
        Logger.Log("[Reset] AD_DRAW 업적 초기화 완료");
    }
}