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
    }
    
    IEnumerator InitializeGame()
    {
        Log("=== 게임 초기화 시작 ===");
        IsInitialized = false;
        InitializationProgress = 0f;
        
        // 1단계: CardsDictionary 초기화 대기 (Awake에서 실행됨)
        Log("1/6: CardsDictionary 초기화 대기...");
        yield return new WaitUntil(() => CardsDictionary.IsDataLoaded);
        Log("✓ CardsDictionary 로드 완료");
        InitializationProgress = 0.16f;
        
        // 2단계: ProductDataTable 초기화 대기 (Awake에서 실행됨)
        Log("2/6: ProductDataTable 초기화 대기...");
        yield return new WaitUntil(() => ProductDataTable.IsDataLoaded);
        Log("✓ ProductDataTable 로드 완료");
        InitializationProgress = 0.33f;
        
        // 3단계: PlayerDataManager 초기화 대기 (Awake에서 실행됨)
        Log("3/6: PlayerDataManager 초기화 대기...");
        yield return new WaitUntil(() => PlayerDataManager.IsDataLoaded);
        Log("✓ PlayerDataManager 로드 완료");
        InitializationProgress = 0.5f;
        
        // 4단계: CardDataManager 초기화 대기 (Start에서 실행됨)
        Log("4/6: CardDataManager 초기화 대기...");
        yield return new WaitUntil(() => CardDataManager.IsDataLoaded);
        Log("✓ CardDataManager 로드 완료");
        InitializationProgress = 0.66f;
        
        // 5단계: EquipmentDataManager 초기화 대기 (Start에서 실행됨)
        Log("5/6: EquipmentDataManager 초기화 대기...");
        yield return new WaitUntil(() => EquipmentDataManager.IsDataLoaded);
        Log("✓ EquipmentDataManager 로드 완료");
        InitializationProgress = 0.83f;
        
        // 6단계: CardList 초기화 확인 (EquipmentDataManager.Start에서 실행됨)
        Log("6/6: CardList 초기화 확인...");
        // CardList는 EquipmentDataManager.Start()에서 InitCardList()가 호출되므로
        // 추가 대기 없이 다음 프레임만 기다림
        yield return null;
        Log("✓ CardList 초기화 완료");
        InitializationProgress = 1f;
        
        // 모든 초기화 완료
        IsInitialized = true;
        OnGameInitialized?.Invoke();
        Log("=== 게임 초기화 완료 ===");
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