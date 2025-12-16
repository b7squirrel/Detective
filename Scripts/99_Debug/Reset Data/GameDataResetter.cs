using UnityEngine;
using System.IO;
using System.Collections;

/// <summary>
/// 게임의 모든 데이터를 완전히 초기화하는 유틸리티
/// 테스트 및 디버깅 용도
/// </summary>
public class GameDataResetter : MonoBehaviour
{
    [Header("초기화 옵션")]
    [SerializeField] bool resetCardData = true;
    [SerializeField] bool resetEquipmentData = true;
    [SerializeField] bool resetPlayerData = true;
    [SerializeField] bool resetStatData = true;
    [SerializeField] bool resetAllPersistentData = false; // 위험: 모든 파일 삭제
    
    [Header("UI 참조")]
    [SerializeField] CardSlotManager cardSlotManager;
    
    [Header("디버그")]
    [SerializeField] bool showLogs = true;
    
    // ⭐ 무한 루프 방지
    private static bool isResetting = false;

    void Awake()
    {
        // ⭐ 리셋 중이면 이 GameObject 즉시 파괴
        if (isResetting)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 버튼에서 호출: 선택한 데이터만 초기화
    /// </summary>
    public void ResetSelectedData()
    {
        StartCoroutine(ResetDataCoroutine(false));
    }
    
    /// <summary>
    /// 버튼에서 호출: 모든 데이터 완전 삭제 (위험)
    /// </summary>
    public void ResetAllDataDangerous()
    {
        if (Application.isEditor)
        {
            StartCoroutine(ResetDataCoroutine(true));
        }
        else
        {
            Logger.LogError("[GameDataResetter] ResetAllData는 에디터에서만 사용 가능합니다!");
        }
    }

    IEnumerator ResetDataCoroutine(bool resetAll)
    {
        Log("=== 데이터 초기화 시작 ===");
        
        // 1단계: 파일 삭제
        if (resetAll || resetCardData)
        {
            DeleteCardData();
        }
        
        if (resetAll || resetEquipmentData)
        {
            DeleteEquipmentData();
        }

        if(resetAll || resetStatData)
        {
            DeleteStatData();
        }
        
        if (resetAll || resetPlayerData)
        {
            DeletePlayerData();
        }
        
        if (resetAll && resetAllPersistentData)
        {
            DeleteAllPersistentData();
        }
        
        yield return null;
        
        // 2단계: 런타임 데이터 초기화
        ResetRuntimeData();
        
        yield return null;
        
        // 3단계: UI 초기화
        if (cardSlotManager != null)
        {
            cardSlotManager.ClearAllSlots();
            Log("✓ CardSlotManager 초기화 완료");
        }
        
        yield return null;
        
        // 4단계: 데이터 매니저 재로드
        ReloadDataManagers();
        
        Log("=== 데이터 초기화 완료 ===");
        Log("⚠️ 씬을 다시 로드하는 것을 권장합니다.");
    }

    #region 파일 삭제
    
    void DeleteCardData()
    {
        // ⭐ PlayerData 폴더 내부의 MyCards.txt
        string dataDir = Path.Combine(Application.persistentDataPath, "PlayerData");
        string filePath = Path.Combine(dataDir, "MyCards.txt");
        
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Log($"✓ 삭제됨: PlayerData/MyCards.txt");
        }
        else
        {
            Log("MyCards.txt 파일이 없습니다.");
        }
    }
    
    void DeleteEquipmentData()
    {
        // ⭐ MyEquipmentsData 폴더 내부의 MyEquipments.txt
        string dataDir = Path.Combine(Application.persistentDataPath, "MyEquipmentsData");
        string filePath = Path.Combine(dataDir, "MyEquipments.txt");
        
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Log($"✓ 삭제됨: MyEquipmentsData/MyEquipments.txt");
        }
        else
        {
            Log("MyEquipments.txt 파일이 없습니다.");
        }
    }

    void DeleteStatData()
    {
        // ⭐ StatsData 폴더 내부의 AllStats.txt
        string dataDir = Path.Combine(Application.persistentDataPath, "StatsData");
        string filePath = Path.Combine(dataDir, "AllStats.txt");
        
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Log($"✓ 삭제됨: StatsData/AllStats.txt");
        }
        else
        {
            Log("AllStats.txt 파일이 없습니다.");
        }
    }
    
    void DeletePlayerData()
    {
        // ⭐ persistentDataPath 루트의 playerData.json
        string filePath = Path.Combine(Application.persistentDataPath, "playerData.json");
        
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Log($"✓ 삭제됨: playerData.json");
        }
        else
        {
            Log("playerData.json 파일이 없습니다.");
        }
    }
    
    void DeleteAllPersistentData()
    {
        string persistentPath = Application.persistentDataPath;
        
        if (Directory.Exists(persistentPath))
        {
            string[] files = Directory.GetFiles(persistentPath, "*", SearchOption.AllDirectories);
            int deletedCount = 0;
            
            foreach (string file in files)
            {
                try
                {
                    File.Delete(file);
                    deletedCount++;
                    Log($"✓ 삭제됨: {file.Replace(persistentPath, "")}");
                }
                catch (System.Exception e)
                {
                    LogError($"삭제 실패: {Path.GetFileName(file)} - {e.Message}");
                }
            }
            
            Log($"✓ 총 {deletedCount}개 파일 삭제 완료");
        }
    }
    
    #endregion

    #region 런타임 데이터 초기화
    
    void ResetRuntimeData()
    {
        Log("런타임 데이터 초기화 중...");
        
        // CardDataManager 초기화
        CardDataManager cardDataManager = FindObjectOfType<CardDataManager>();
        if (cardDataManager != null)
        {
            cardDataManager.ResetData();
            Log("✓ CardDataManager 런타임 데이터 초기화");
        }
        
        // EquipmentDataManager 초기화
        EquipmentDataManager equipmentDataManager = FindObjectOfType<EquipmentDataManager>();
        if (equipmentDataManager != null)
        {
            equipmentDataManager.ResetData();
            Log("✓ EquipmentDataManager 런타임 데이터 초기화");
        }
        
        // PlayerDataManager 초기화
        PlayerDataManager playerDataManager = PlayerDataManager.Instance;
        if (playerDataManager != null)
        {
            playerDataManager.ResetToDefault();
            Log("✓ PlayerDataManager 기본값으로 초기화");
        }
    }
    
    #endregion

    #region 데이터 매니저 재로드
    
    void ReloadDataManagers()
    {
        Log("데이터 매니저 재로드 중...");
        
        CardDataManager cardDataManager = FindObjectOfType<CardDataManager>();
        if (cardDataManager != null)
        {
            Log("⚠️ CardDataManager: 씬 재로드 필요");
        }
        
        EquipmentDataManager equipmentDataManager = FindObjectOfType<EquipmentDataManager>();
        if (equipmentDataManager != null)
        {
            Log("⚠️ EquipmentDataManager: 씬 재로드 필요");
        }
        
        PlayerDataManager playerDataManager = PlayerDataManager.Instance;
        if (playerDataManager != null)
        {
            Log("⚠️ PlayerDataManager: 씬 재로드 필요");
        }
    }
    
    #endregion

    #region 씬 재로드 (완전 초기화)
    
    /// <summary>
    /// 가장 확실한 방법: 씬 전체 재로드
    /// </summary>
    public void ResetAndReloadScene()
    {
        StartCoroutine(ResetAndReloadSceneCoroutine());
    }
    
    IEnumerator ResetAndReloadSceneCoroutine()
    {
        // ⭐ 리셋 중 플래그 설정 (무한 루프 방지)
        if (isResetting)
        {
            Log("⚠️ 이미 리셋 중입니다.");
            yield break;
        }
        
        isResetting = true;
        
        Log("=== 완전 초기화 시작 ===");
        
        // 파일 삭제
        if (resetCardData) DeleteCardData();
        if (resetEquipmentData) DeleteEquipmentData();
        if (resetStatData) DeleteStatData();
        if (resetPlayerData) DeletePlayerData();
        
        yield return new WaitForSeconds(0.5f);
        
        // 씬 재로드
        Log("씬 재로드 중...");
        
        // ⭐ 씬 로드 완료 후 플래그 리셋
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
    
    // ⭐ 씬 로드 완료 시 플래그 리셋
    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        isResetting = false;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        Logger.Log("[GameDataResetter] 씬 재로드 완료");
    }
    
    #endregion

    #region 디버그 정보
    
    /// <summary>
    /// 현재 저장된 파일 목록 출력
    /// </summary>
    [ContextMenu("Show Saved Files")]
    public void ShowSavedFiles()
    {
        string persistentPath = Application.persistentDataPath;
        Log($"=== Persistent Data Path ===");
        Log(persistentPath);
        Log("");
        
        // PlayerData 폴더 확인
        ShowFolderFiles("PlayerData");
        
        // MyEquipmentsData 폴더 확인
        ShowFolderFiles("MyEquipmentsData");
        
        // StatsData 폴더 확인
        ShowFolderFiles("StatsData");
        
        // 루트 폴더 확인
        if (Directory.Exists(persistentPath))
        {
            string[] rootFiles = Directory.GetFiles(persistentPath);
            Log($"루트 폴더: {rootFiles.Length}개 파일");
            
            foreach (string file in rootFiles)
            {
                FileInfo fileInfo = new FileInfo(file);
                Log($"  - {Path.GetFileName(file)} ({fileInfo.Length} bytes)");
            }
        }
        else
        {
            Log("폴더가 없습니다.");
        }
    }
    
    void ShowFolderFiles(string folderName)
    {
        string folderPath = Path.Combine(Application.persistentDataPath, folderName);
        
        if (Directory.Exists(folderPath))
        {
            string[] files = Directory.GetFiles(folderPath);
            Log($"{folderName} 폴더: {files.Length}개 파일");
            
            foreach (string file in files)
            {
                FileInfo fileInfo = new FileInfo(file);
                Log($"  - {Path.GetFileName(file)} ({fileInfo.Length} bytes)");
            }
            Log("");
        }
        else
        {
            Log($"{folderName} 폴더가 없습니다.");
            Log("");
        }
    }
    
    /// <summary>
    /// Persistent Data 폴더를 파일 탐색기에서 열기
    /// </summary>
    [ContextMenu("Open Persistent Data Folder")]
    public void OpenPersistentDataFolder()
    {
        string path = Application.persistentDataPath;
        
        if (Directory.Exists(path))
        {
            Application.OpenURL("file://" + path);
            Log($"폴더 열림: {path}");
        }
        else
        {
            LogError($"폴더가 없습니다: {path}");
        }
    }
    
    #endregion

    #region 로깅
    
    void Log(string message)
    {
        if (showLogs)
        {
            Logger.Log($"[GameDataResetter] {message}");
        }
    }
    
    void LogError(string message)
    {
        Logger.LogError($"[GameDataResetter] {message}");
    }
    
    #endregion
}