using System.Collections;
using UnityEngine;

/// <summary>
/// 게임의 모든 데이터 매니저를 순서대로 초기화하는 중앙 관리자
/// 초기화 순서:
/// 0. CloudSaveManager (구글 플레이 로그인 및 클라우드 동기화)
/// 1. CardsDictionary (ScriptableObject 데이터)
/// 2. ProductDataTable (상품 데이터)
/// 3. PlayerDataManager (플레이어 진행 데이터)
/// 4. CardDataManager (플레이어 카드 데이터)
/// 5. EquipmentDataManager (장비 장착 데이터)
/// 6. CardList 초기화 (런타임 객체 생성)
/// </summary>
public class GameInitializer : MonoBehaviour
{
    [Header("초기화할 매니저들")]
    [Tooltip("ScriptableObject 데이터 관리")]
    [SerializeField] CardsDictionary cardsDictionary;

    [Tooltip("상품 데이터 테이블")]
    [SerializeField] ProductDataTable productDataTable;

    [Tooltip("플레이어 진행 데이터 관리")]
    [SerializeField] PlayerDataManager playerDataManager;

    [Tooltip("일일 퀘스트 진행 데이터 관리")]
    [SerializeField] DailyResetManager dailyResetManager;

    [Tooltip("주간 퀘스트 리셋 관리")]
    [SerializeField] WeeklyResetManager weeklyResetManager;

    [Tooltip("플레이어 카드 데이터 관리")]
    [SerializeField] CardDataManager cardDataManager;

    [Tooltip("장비 장착 데이터 관리")]
    [SerializeField] EquipmentDataManager equipmentDataManager;

    [Header("초기화 상태")]
    [SerializeField] private bool showDebugLogs = true;

    // ✅ 추가: 중복 실행 방지
    private static bool isRunning = false;

    /// <summary>
    /// 모든 초기화가 완료되었는지 여부
    /// 다른 스크립트에서 이 값을 체크하여 초기화 완료를 확인할 수 있음
    /// </summary>
    public static bool IsInitialized { get; private set; } = false;
    public static event System.Action OnGameInitialized; // UI등이 실행되도록

    /// <summary>
    /// 초기화 진행률 (0.0 ~ 1.0)
    /// </summary>
    public static float InitializationProgress { get; private set; } = 0f;

    // 세션 동안 한 번만 실행되도록 플래그 추가
    private static bool hasShownDailyRewardThisSession = false;

    // 각 단계별 타임아웃 (초)
    private const float STEP_TIMEOUT = 10f;

    void Awake()
    {
        // 씬 전환 시에도 유지 (선택사항)
        // DontDestroyOnLoad(gameObject);
        // ✅ 이미 실행 중이면 이 인스턴스는 즉시 파괴
        if (isRunning)
        {
            Debug.LogWarning("[GameInitializer] 중복 인스턴스 감지 - 파괴합니다.");
            Destroy(gameObject);
            return;
        }

        isRunning = true;

        StartCoroutine(InitializeGame());
    }

    void OnApplicationQuit()
    {
        IsInitialized = false;
        InitializationProgress = 0f;
        hasShownDailyRewardThisSession = false;
    }

    IEnumerator InitializeGame()
    {
        Log("=== 게임 초기화 시작 ===");
        IsInitialized = false;
        InitializationProgress = 0f;

        // 0단계: 클라우드 로그인 및 동기화
        Debug.Log("[GameInitializer] 0/7: 클라우드 로그인 시도...");
        float cloudWait = 0f;
        while (CloudSaveManager.Instance == null && cloudWait < 3f)
        {
            cloudWait += Time.deltaTime;
            yield return null;
        }

        if (CloudSaveManager.Instance != null)
        {
            yield return StartCoroutine(CloudSaveManager.Instance.SignInAndSync());
            Debug.Log("[GameInitializer] 클라우드 초기화 완료");
        }
        else
        {
            Debug.LogWarning("[GameInitializer] CloudSaveManager를 찾을 수 없습니다.");
        }

        // 1단계: CardsDictionary 초기화 대기 (Awake에서 실행됨)
        Log("1/7: CardsDictionary 초기화 대기...");
        float t1 = 0f;
        while (!CardsDictionary.IsDataLoaded && t1 < STEP_TIMEOUT)
        {
            t1 += Time.deltaTime;
            yield return null;
        }
        Debug.Log($"[GameInitializer] ✅ 1 CardsDictionary OK (waited {t1:F2}s, loaded={CardsDictionary.IsDataLoaded})");
        Log("v CardsDictionary 로드 완료");
        InitializationProgress = 0.16f;

        // 2단계: ProductDataTable 초기화 대기 (Awake에서 실행됨)
        Log("2/7: ProductDataTable 초기화 대기...");
        float t2 = 0f;
        while (!ProductDataTable.IsDataLoaded && t2 < STEP_TIMEOUT)
        {
            t2 += Time.deltaTime;
            yield return null;
        }
        Debug.Log($"[GameInitializer] ✅ 2 ProductDataTable OK (waited {t2:F2}s, loaded={ProductDataTable.IsDataLoaded})");
        Log("v ProductDataTable 로드 완료");
        InitializationProgress = 0.33f;

        // 3단계: PlayerDataManager 초기화 대기 (Awake에서 실행됨)
        Log("3/7: PlayerDataManager 초기화 대기...");
        float t3 = 0f;
        while (!PlayerDataManager.IsDataLoaded && t3 < STEP_TIMEOUT)
        {
            t3 += Time.deltaTime;
            yield return null;
        }
        Debug.Log($"[GameInitializer] ✅ 3 PlayerDataManager OK (waited {t3:F2}s, loaded={PlayerDataManager.IsDataLoaded})");
        Log("v PlayerDataManager 로드 완료");
        InitializationProgress = 0.5f;

        // 4단계: DailyResetManager (Awake에서 실행됨)
        Log("4/7: DailyResetManager 초기화 대기...");
        float t4 = 0f;
        while (!DailyResetManager.IsInitialized && t4 < STEP_TIMEOUT)
        {
            t4 += Time.deltaTime;
            yield return null;
        }
        Debug.Log($"[GameInitializer] ✅ 4 DailyResetManager OK (waited {t4:F2}s, loaded={DailyResetManager.IsInitialized})");
        Log("v DailyResetManager 로드 완료");
        InitializationProgress = 0.57f;

        // 5단계: WeeklyResetManager 추가
        Log("5/8: WeeklyResetManager 초기화 대기...");
        float t5 = 0f;
        while (!WeeklyResetManager.IsInitialized && t5 < STEP_TIMEOUT)
        {
            t5 += Time.deltaTime;
            yield return null;
        }
        Debug.Log($"[GameInitializer] ✅ 5 WeeklyResetManager OK (waited {t5:F2}s, loaded={WeeklyResetManager.IsInitialized})");
        Log("v WeeklyResetManager 로드 완료");
        InitializationProgress = 0.63f;

        // 6단계: CardDataManager 초기화 대기 (Start에서 실행됨)
        Log("6/7: CardDataManager 초기화 대기...");
        float t6 = 0f;
        while (!CardDataManager.IsDataLoaded && t6 < STEP_TIMEOUT)
        {
            t6 += Time.deltaTime;
            yield return null;
        }
        Debug.Log($"[GameInitializer] ✅ 6 CardDataManager OK (waited {t6:F2}s, loaded={CardDataManager.IsDataLoaded})");
        Log("v CardDataManager 로드 완료");
        InitializationProgress = 0.66f;

        // 7단계: EquipmentDataManager 초기화 대기 (Start에서 실행됨)
        Log("7/7: EquipmentDataManager 초기화 대기...");
        float t7 = 0f;
        while (!EquipmentDataManager.IsDataLoaded && t7 < STEP_TIMEOUT)
        {
            t7 += Time.deltaTime;
            yield return null;
        }
        Debug.Log($"[GameInitializer] ✅ 7 EquipmentDataManager OK (waited {t7:F2}s, loaded={EquipmentDataManager.IsDataLoaded})");
        Log("v EquipmentDataManager 로드 완료");
        InitializationProgress = 0.83f;

        // 8단계: CardList 초기화 확인 (EquipmentDataManager.Start에서 실행됨)
        Log("8/8: CardList 초기화 확인...");
        // CardList는 EquipmentDataManager.Start()에서 InitCardList()가 호출되므로
        // 추가 대기 없이 다음 프레임만 기다림
        yield return null;
        Log("v CardList 초기화 완료");
        InitializationProgress = 1f;

        // 모든 초기화 완료
        IsInitialized = true;
        Debug.Log("[GameInitializer] ✅ IsInitialized = true");
        OnGameInitialized?.Invoke();
        Debug.Log("[GameInitializer] ✅ OnGameInitialized 완료");

        // ✅ 추가: 첫 설치 감지 및 장비 초기화
        // 모든 시스템이 완전히 준비된 후 실행
        yield return null; // GachaSystem.InitializeGachaSystem() 코루틴이 진행되도록 한 프레임 대기
        yield return new WaitUntil(() => GachaSystem.IsInitialized);

        EquipmentDataManager edm = FindObjectOfType<EquipmentDataManager>();
        CardDataManager cdm = FindObjectOfType<CardDataManager>();
        GachaSystem gachaSys = FindObjectOfType<GachaSystem>();

        // 장비 데이터가 없으면 첫 설치로 판단
        if (edm != null && cdm != null && gachaSys != null
            && edm.GetMyEquipmentsList().Count == 0)
        {
            CardData leadCard = cdm.GetMyCardList()
                .Find(x => x.StartingMember == StartingMember.Zero.ToString());

            if (leadCard != null)
            {
                gachaSys.AddDefaultEquip(leadCard);
                gachaSys.ImmediateSaveEquipmentData();
                Debug.Log("[GameInitializer] ✅ 첫 설치 장비 초기화 완료");
            }
            else
            {
                Debug.LogError("[GameInitializer] ❌ 첫 설치인데 리드 카드를 찾을 수 없음!");
            }
        }

        // ⭐ 추가: 모든 초기화 완료 후 주간 리셋 체크
        // ⭐ 1. 주간 팝업 먼저
        yield return new WaitForSeconds(0.1f);
        Debug.Log("[GameInitializer] ✅ 주간 리셋 체크 시작");
        bool weeklyPopupShown = WeeklyResetManager.Instance.CheckWeeklyReset();
        Debug.Log($"[GameInitializer] ✅ 주간 팝업: {weeklyPopupShown}");

        // ⭐ 2. 주간 팝업이 떴으면 닫힐 때까지 대기
        if (weeklyPopupShown)
        {
            Debug.Log("[GameInitializer] ✅ 주간 팝업 닫힘 대기 중...");
            yield return new WaitUntil(() =>
            {
                var popup = FindObjectOfType<WeeklyRewardPopup>(true);
                return popup == null || !popup.gameObject.activeSelf;
            });
            yield return new WaitForSeconds(0.3f); // 팝업 닫힌 후 잠시 대기
        }

        // ⭐ 3. 일일 보상 팝업
        Debug.Log("[GameInitializer] ✅ 일일 보상 체크");
        if (!hasShownDailyRewardThisSession)
        {
            CheckAndShowDailyReward();
            hasShownDailyRewardThisSession = true;
        }
        Debug.Log("[GameInitializer] ✅ 초기화 완전 완료");
    }

    void CheckAndShowDailyReward()
    {
        PlayerDataManager pdm = PlayerDataManager.Instance;

        if (pdm == null) return;

        // 오늘 출석 보상을 아직 안 받았으면
        if (!pdm.HasTakenDailyReward())
        {
            // 일일 보상 팝업 표시
            DailyRewardPanel panel = FindObjectOfType<DailyRewardPanel>(true);

            if (panel != null)
            {
                panel.gameObject.SetActive(true);
                panel.GetComponent<Animator>().SetTrigger("Up");
                Logger.Log("[GameInitializer] 일일 출석 보상 팝업 표시");
            }
        }
    }

    void OnDestroy()
    {
        // 이 인스턴스가 실제로 실행 중이었을 때만 플래그 해제
        isRunning = false;
        IsInitialized = false;
        InitializationProgress = 0f;
        hasShownDailyRewardThisSession = false;
    }

    void Log(string message)
    {
        if (showDebugLogs)
        {
            Logger.Log($"[GameInitializer] {message} (Frame: {Time.frameCount})");
        }
    }

    /// <summary>
    /// 에디터에서 초기화 상태 확인용
    /// </summary>
    void OnGUI()
    {
        if (!showDebugLogs) return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = IsInitialized ? Color.green : Color.yellow;

        GUI.Label(new Rect(10, 10, 400, 30),
            $"Game Initialization: {InitializationProgress * 100:F0}%", style);

        if (IsInitialized)
        {
            GUI.Label(new Rect(10, 40, 400, 30), "✓ Ready!", style);
        }
    }
}