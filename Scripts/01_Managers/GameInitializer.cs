using System.Collections;
using UnityEngine;

/// <summary>
/// 게임의 모든 데이터 매니저를 순서대로 초기화하는 중앙 관리자
/// 초기화 순서:
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
    
    [Tooltip("플레이어 카드 데이터 관리")]
    [SerializeField] CardDataManager cardDataManager;
    
    [Tooltip("장비 장착 데이터 관리")]
    [SerializeField] EquipmentDataManager equipmentDataManager;
    
    [Header("초기화 상태")]
    [SerializeField] private bool showDebugLogs = true;
    
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

    void Awake()
    {
        // 씬 전환 시에도 유지 (선택사항)
        // DontDestroyOnLoad(gameObject);
        
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
        
        // 1단계: CardsDictionary 초기화 대기 (Awake에서 실행됨)
        Log("1/7: CardsDictionary 초기화 대기...");
        yield return new WaitUntil(() => CardsDictionary.IsDataLoaded);
        Log("v CardsDictionary 로드 완료");
        InitializationProgress = 0.16f;
        
        // 2단계: ProductDataTable 초기화 대기 (Awake에서 실행됨)
        Log("2/7: ProductDataTable 초기화 대기...");
        yield return new WaitUntil(() => ProductDataTable.IsDataLoaded);
        Log("v ProductDataTable 로드 완료");
        InitializationProgress = 0.33f;

        // 3단계: PlayerDataManager 초기화 대기 (Awake에서 실행됨)
        Log("3/7: PlayerDataManager 초기화 대기...");
        yield return new WaitUntil(() => PlayerDataManager.IsDataLoaded);
        Log("v PlayerDataManager 로드 완료");
        InitializationProgress = 0.5f;

        // 4단계: DailyResetManager (Awake에서 실행됨)
        Log("4/7: DailyResetManager 초기화 대기...");
        yield return new WaitUntil(() => DailyResetManager.IsInitialized);
        Log("v DailyResetManager 로드 완료");
        InitializationProgress = 0.57f;

        // 5단계: CardDataManager 초기화 대기 (Start에서 실행됨)
        Log("5/7: CardDataManager 초기화 대기...");
        yield return new WaitUntil(() => CardDataManager.IsDataLoaded);
        Log("v CardDataManager 로드 완료");
        InitializationProgress = 0.66f;
        
        // 6단계: EquipmentDataManager 초기화 대기 (Start에서 실행됨)
        Log("6/7: EquipmentDataManager 초기화 대기...");
        yield return new WaitUntil(() => EquipmentDataManager.IsDataLoaded);
        Log("v EquipmentDataManager 로드 완료");
        InitializationProgress = 0.83f;
        
        // 7단계: CardList 초기화 확인 (EquipmentDataManager.Start에서 실행됨)
        Log("7/7: CardList 초기화 확인...");
        // CardList는 EquipmentDataManager.Start()에서 InitCardList()가 호출되므로
        // 추가 대기 없이 다음 프레임만 기다림
        yield return null;
        Log("v CardList 초기화 완료");
        InitializationProgress = 1f;

        // 모든 초기화 완료
        IsInitialized = true;
        OnGameInitialized?.Invoke();

        // 초기화 완료 후 일일 보상 체크
        yield return new WaitForSeconds(0.5f); // UI 안정화 대기
        if (hasShownDailyRewardThisSession == false)
        {
            CheckAndShowDailyReward();
            hasShownDailyRewardThisSession = true;
        }

        Log("=== 게임 초기화 완료 ===");
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
                Logger.Log("[GameInitializer] 일일 출석 보상 팝업 표시");
            }
        }
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