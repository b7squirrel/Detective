#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// ─────────────────────────────────────────────────────────────
// 클라우드에 저장할 데이터 구조체
// 새 데이터를 추가하려면 여기에 필드만 추가하면 됩니다.
// ─────────────────────────────────────────────────────────────
[Serializable]
public class CloudSaveData
{
    public int version = 1;                // 데이터 버전 (하위 호환성용)
    public string playerDataJson = "";     // PlayerData (스테이지, 코인 등)
    public string myCardsJson = "";        // 카드 목록
    public string myEquipmentsJson = "";   // 장비 배치 데이터
    public int tutorialStep = 0;          // 튜토리얼 단계
    public string achievementsJson = "";   // 영구 업적 진행도
    public long savedAtTicks = 0; // 저장 시각 (DateTime.Ticks)
    public bool starterPackPurchased = false;  // 초보자 팩 1회 구매 기록
    public bool proPackPurchased = false;      // 전문가 팩 1회 구매 기록

    // ─── 나중에 추가할 데이터는 아래에 필드만 추가하면 됩니다 ───
    // public List<string> collectedEquipmentIds = new List<string>();  // 장비 도감 (출시 후 추가 예정)
}

// ─────────────────────────────────────────────────────────────
// 업적 저장용 보조 클래스
// ─────────────────────────────────────────────────────────────
[Serializable]
public class AchievementSaveEntry
{
    public string id;
    public int progress;
    public bool isCompleted;
    public bool isRewarded;
}

[Serializable]
public class AchievementSaveList
{
    public List<AchievementSaveEntry> entries = new List<AchievementSaveEntry>();
}

// ─────────────────────────────────────────────────────────────
// CloudSaveManager
// ─────────────────────────────────────────────────────────────
public class CloudSaveManager : MonoBehaviour
{
    public static CloudSaveManager Instance { get; private set; }

    // 클라우드 저장 파일 이름 (변경하지 마세요)
    private const string SAVE_FILE_NAME = "QuackSurvivorsSave";

    // 초기화 및 로그인 상태
    public static bool IsInitialized { get; private set; } = false;
    public static bool PendingSceneReload { get; private set; } = false;

    public bool IsAuthenticated
    {
        get
        {
#if UNITY_ANDROID
            return PlayGamesPlatform.Instance != null &&
                   PlayGamesPlatform.Instance.IsAuthenticated();
#else
            return false;
#endif
        }
    }

    // 마지막 저장 시각 (너무 자주 저장하지 않도록)
    private DateTime lastSaveTime = DateTime.MinValue;
    private const float MIN_SAVE_INTERVAL_SECONDS = 30f;

    // ─────────────────────────────────────────────────────────
    // 초기화
    // ─────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

#if UNITY_ANDROID
        InitializePlayGames();
#else
        IsInitialized = true;
        Debug.Log("[CloudSaveManager] 비Android 플랫폼 - 클라우드 저장 비활성화");
#endif
    }

#if UNITY_ANDROID
    private void InitializePlayGames()
    {
        PlayGamesPlatform.DebugLogEnabled = false;
        PlayGamesPlatform.Activate();
        Debug.Log("[CloudSaveManager] Google Play Games 초기화 완료");
    }
#endif

    // ─────────────────────────────────────────────────────────
    // 로그인 및 동기화
    // GameInitializer에서 0단계로 호출합니다.
    // ─────────────────────────────────────────────────────────
    public IEnumerator SignInAndSync()
    {
#if UNITY_ANDROID
        // 이미 초기화됐으면 스킵
        if (IsInitialized)
        {
            Debug.Log("[CloudSaveManager] 이미 초기화됨. 스킵.");
            yield break;
        }
        Debug.Log("[CloudSaveManager] Google Play 로그인 시도...");

        bool loginDone = false;
        bool loginSuccess = false;

        PlayGamesPlatform.Instance.Authenticate((status) =>
        {
            loginSuccess = (status == SignInStatus.Success);
            loginDone = true;

            if (loginSuccess)
                Debug.Log("[CloudSaveManager] 로그인 성공!");
            else
                Debug.LogWarning($"[CloudSaveManager] 로그인 실패: {status}. 로컬 데이터로 진행합니다.");
        });

        // 로그인 완료 대기 (최대 10초)
        float timeout = 10f;
        while (!loginDone && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        IsInitialized = true;

        if (!loginSuccess)
        {
            yield break;
        }

        yield return StartCoroutine(DownloadAndSync());
#else
        // 에디터에서는 즉시 완료
        IsInitialized = true;
        Debug.Log("[CloudSaveManager] 에디터 환경 - 로그인 건너뜀");
        yield return null;
#endif
    }

#if UNITY_ANDROID
    // ─────────────────────────────────────────────────────────
    // 클라우드 → 로컬 동기화 (다운로드)
    // ─────────────────────────────────────────────────────────
    private IEnumerator DownloadAndSync()
    {
        Debug.Log("[CloudSaveManager] 클라우드 데이터 확인 중...");

        bool done = false;
        CloudSaveData cloudData = null;
        string cloudError = null;

        OpenSavedGame((status, game) =>
        {
            if (status != SavedGameRequestStatus.Success)
            {
                cloudError = status.ToString();
                done = true;
                return;
            }

            ReadSavedGame(game, (readStatus, data) =>
            {
                if (readStatus == SavedGameRequestStatus.Success &&
                    data != null && data.Length > 0)
                {
                    try
                    {
                        string json = Encoding.UTF8.GetString(data);
                        cloudData = JsonUtility.FromJson<CloudSaveData>(json);
                        Debug.Log($"[CloudSaveManager] 클라우드 데이터 읽기 성공 (version: {cloudData?.version})");
                    }
                    catch (Exception e)
                    {
                        cloudError = e.Message;
                        Debug.LogError($"[CloudSaveManager] 클라우드 데이터 파싱 오류: {e.Message}");
                    }
                }
                else
                {
                    Debug.Log("[CloudSaveManager] 클라우드에 저장된 데이터 없음 (신규 유저)");
                }
                done = true;
            });
        });

        yield return new WaitUntil(() => done);

        if (cloudError != null)
        {
            Debug.LogWarning($"[CloudSaveManager] 클라우드 읽기 실패: {cloudError}. 로컬 데이터 사용.");
            yield break;
        }

        if (cloudData == null)
        {
            Debug.Log("[CloudSaveManager] 신규 유저 - 로컬 데이터를 클라우드에 첫 업로드");
            yield return StartCoroutine(UploadToCloud());
            yield break;
        }

        ApplyCloudDataIfNewer(cloudData);
    }

    // ─────────────────────────────────────────────────────────
    // 클라우드 데이터 적용 여부 판단
    // 스테이지 번호가 더 높은 쪽을 최신으로 판단합니다.
    // ─────────────────────────────────────────────────────────
    private void ApplyCloudDataIfNewer(CloudSaveData cloudData)
    {
        try
        {
            // 클라우드 저장 시각이 더 최신이면 적용
            // 재설치한 경우 로컬 savedAtTicks=0이므로 항상 클라우드 적용
            long localTicks = PlayerPrefs.GetString("CloudSavedAt", "0")
                .Equals("0") ? 0 : long.Parse(PlayerPrefs.GetString("CloudSavedAt", "0"));

            if (cloudData.savedAtTicks > localTicks)
            {
                Debug.Log($"[CloudSaveManager] 클라우드가 더 최신 → 적용");
                ApplyAllCloudData(cloudData);
            }
            else
            {
                Debug.Log($"[CloudSaveManager] 로컬이 최신 또는 동일 → 클라우드 업데이트");
                StartCoroutine(UploadToCloud());
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[CloudSaveManager] 클라우드 데이터 적용 오류: {e.Message}");
        }
    }

    // ─────────────────────────────────────────────────────────
    // 클라우드 데이터를 로컬에 전부 적용
    // ─────────────────────────────────────────────────────────
    private void ApplyAllCloudData(CloudSaveData cloudData)
    {
        try
        {
            // 1. PlayerData
            if (!string.IsNullOrEmpty(cloudData.playerDataJson))
            {
                string path = System.IO.Path.Combine(
                    Application.persistentDataPath, "playerData.json");
                System.IO.File.WriteAllText(path, cloudData.playerDataJson, Encoding.UTF8);
                Debug.Log("[CloudSaveManager] PlayerData 적용 완료");

                // DontDestroyOnLoad이므로 메모리도 즉시 갱신
                if (PlayerDataManager.Instance != null)
                    PlayerDataManager.Instance.ReloadFromDisk();
            }

            // 2. 카드 데이터
            if (!string.IsNullOrEmpty(cloudData.myCardsJson))
            {
                string path = System.IO.Path.Combine(
                    Application.persistentDataPath, "PlayerData", "MyCards.txt");
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                System.IO.File.WriteAllText(path, cloudData.myCardsJson, Encoding.UTF8);
                Debug.Log("[CloudSaveManager] 카드 데이터 적용 완료");
            }

            // 3. 장비 데이터
            if (!string.IsNullOrEmpty(cloudData.myEquipmentsJson))
            {
                string path = System.IO.Path.Combine(
                    Application.persistentDataPath, "MyEquipmentsData", "MyEquipments.txt");
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                System.IO.File.WriteAllText(path, cloudData.myEquipmentsJson, Encoding.UTF8);
                Debug.Log("[CloudSaveManager] 장비 데이터 적용 완료");
            }

            // 4. 튜토리얼 단계
            int localTutorial = PlayerPrefs.GetInt("TutorialStep", 0);
            if (cloudData.tutorialStep > localTutorial)
            {
                PlayerPrefs.SetInt("TutorialStep", cloudData.tutorialStep);
                Debug.Log($"[CloudSaveManager] 튜토리얼 단계 적용: {cloudData.tutorialStep}");
            }

            // 5. 영구 업적
            if (!string.IsNullOrEmpty(cloudData.achievementsJson))
                ApplyAchievements(cloudData.achievementsJson);

            // 6. 팩 구매 기록 복원
            PackPurchaseManager.Instance?.ApplyFromCloud(
                cloudData.starterPackPurchased,
                cloudData.proPackPurchased
            );

            PlayerPrefs.Save();
            Debug.Log("[CloudSaveManager] 모든 클라우드 데이터 적용 완료 - 리로드 필요 플래그 설정");

            // ⭐ 변경: 여기서 직접 SceneManager.LoadScene() 호출하지 않음
            PendingSceneReload = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[CloudSaveManager] 로컬 적용 오류: {e.Message}");
        }
    }

    // ⭐ 추가: GameInitializer가 리로드를 소비한 후 호출
    public static void ClearPendingSceneReload()
    {
        PendingSceneReload = false;
    }

    // ─────────────────────────────────────────────────────────
    // 영구 업적 적용
    // ─────────────────────────────────────────────────────────
    private void ApplyAchievements(string achievementsJson)
    {
        try
        {
            AchievementSaveList saveList = JsonUtility.FromJson<AchievementSaveList>(achievementsJson);
            if (saveList?.entries == null) return;

            foreach (var entry in saveList.entries)
            {
                int localProgress = PlayerPrefs.GetInt("ACH_PROGRESS_" + entry.id, 0);
                if (entry.progress > localProgress)
                {
                    PlayerPrefs.SetInt("ACH_PROGRESS_" + entry.id, entry.progress);
                    PlayerPrefs.SetInt("ACH_" + entry.id, entry.isCompleted ? 1 : 0);
                    PlayerPrefs.SetInt("ACH_REWARD_" + entry.id, entry.isRewarded ? 1 : 0);
                }
            }
            Debug.Log("[CloudSaveManager] 영구 업적 적용 완료");
        }
        catch (Exception e)
        {
            Debug.LogError($"[CloudSaveManager] 업적 적용 오류: {e.Message}");
        }
    }

    // ─────────────────────────────────────────────────────────
    // 로컬 → 클라우드 업로드
    // ─────────────────────────────────────────────────────────
    private IEnumerator UploadToCloud()
    {
        Debug.Log("[CloudSaveManager] 클라우드 업로드 시작...");

        CloudSaveData saveData = BuildSaveData();
        string json = JsonUtility.ToJson(saveData);
        byte[] data = Encoding.UTF8.GetBytes(json);

        bool done = false;
        string uploadError = null;

        OpenSavedGame((status, game) =>
        {
            if (status != SavedGameRequestStatus.Success)
            {
                uploadError = status.ToString();
                done = true;
                return;
            }

            var update = new SavedGameMetadataUpdate.Builder()
                .WithUpdatedDescription($"Saved at {DateTime.Now:yyyy-MM-dd HH:mm}")
                .Build();

            PlayGamesPlatform.Instance.SavedGame.CommitUpdate(
                game, update, data, (commitStatus, _) =>
                {
                    if (commitStatus == SavedGameRequestStatus.Success)
                    {
                        lastSaveTime = DateTime.Now;
                        Debug.Log("[CloudSaveManager] 클라우드 업로드 성공!");
                    }
                    else
                    {
                        uploadError = commitStatus.ToString();
                        Debug.LogError($"[CloudSaveManager] 업로드 실패: {commitStatus}");
                    }
                    done = true;
                });
        });

        yield return new WaitUntil(() => done);

        if (uploadError != null)
            Debug.LogError($"[CloudSaveManager] 업로드 오류: {uploadError}");
    }

    // ─────────────────────────────────────────────────────────
    // 현재 로컬 데이터를 CloudSaveData로 빌드
    // ─────────────────────────────────────────────────────────
    private CloudSaveData BuildSaveData()
    {
        CloudSaveData data = new CloudSaveData();

        try
        {
            // 1. PlayerData
            string playerDataPath = System.IO.Path.Combine(
                Application.persistentDataPath, "playerData.json");
            if (System.IO.File.Exists(playerDataPath))
                data.playerDataJson = System.IO.File.ReadAllText(playerDataPath, Encoding.UTF8);

            // 2. 카드 데이터
            string cardsPath = System.IO.Path.Combine(
                Application.persistentDataPath, "PlayerData", "MyCards.txt");
            if (System.IO.File.Exists(cardsPath))
                data.myCardsJson = System.IO.File.ReadAllText(cardsPath, Encoding.UTF8);

            // 3. 장비 데이터
            string equipPath = System.IO.Path.Combine(
                Application.persistentDataPath, "MyEquipmentsData", "MyEquipments.txt");
            if (System.IO.File.Exists(equipPath))
                data.myEquipmentsJson = System.IO.File.ReadAllText(equipPath, Encoding.UTF8);

            // 4. 튜토리얼 단계
            data.tutorialStep = PlayerPrefs.GetInt("TutorialStep", 0);

            // 5. 영구 업적
            data.achievementsJson = BuildAchievementsJson();

            // 6. 팩 구매
            if (PackPurchaseManager.Instance != null)
            {
                var (starter, pro) = PackPurchaseManager.Instance.GetPurchaseStateForCloud();
                data.starterPackPurchased = starter;
                data.proPackPurchased = pro;
            }

            // 7. 저장 시각 기록 (클라우드/로컬 최신 판단용)
            data.savedAtTicks = DateTime.Now.Ticks;
            PlayerPrefs.SetString("CloudSavedAt", data.savedAtTicks.ToString());
            PlayerPrefs.Save();

            Debug.Log("[CloudSaveManager] SaveData 빌드 완료");
        }
        catch (Exception e)
        {
            Debug.LogError($"[CloudSaveManager] SaveData 빌드 오류: {e.Message}");
        }

        return data;
    }

    // ─────────────────────────────────────────────────────────
    // 영구 업적만 JSON으로 빌드
    // ─────────────────────────────────────────────────────────
    private string BuildAchievementsJson()
    {
        try
        {
            if (AchievementManager.Instance == null) return "";

            AchievementSaveList saveList = new AchievementSaveList();

            foreach (var ra in AchievementManager.Instance.GetPermanentAchievements())
            {
                saveList.entries.Add(new AchievementSaveEntry
                {
                    id = ra.original.id,
                    progress = ra.progress,
                    isCompleted = ra.isCompleted,
                    isRewarded = ra.isRewarded
                });
            }

            return JsonUtility.ToJson(saveList);
        }
        catch (Exception e)
        {
            Debug.LogError($"[CloudSaveManager] 업적 JSON 빌드 오류: {e.Message}");
            return "";
        }
    }

    // ─────────────────────────────────────────────────────────
    // 저장 파일 열기 (내부 유틸)
    // ─────────────────────────────────────────────────────────
    private void OpenSavedGame(Action<SavedGameRequestStatus, ISavedGameMetadata> callback)
    {
        PlayGamesPlatform.Instance.SavedGame.OpenWithAutomaticConflictResolution(
            SAVE_FILE_NAME,
            DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLongestPlaytime,
            callback
        );
    }

    // ─────────────────────────────────────────────────────────
    // 저장 파일 읽기 (내부 유틸)
    // ─────────────────────────────────────────────────────────
    private void ReadSavedGame(ISavedGameMetadata game,
        Action<SavedGameRequestStatus, byte[]> callback)
    {
        PlayGamesPlatform.Instance.SavedGame.ReadBinaryData(game, callback);
    }
#endif

    // ─────────────────────────────────────────────────────────
    // 외부 호출용 저장 메서드
    // ─────────────────────────────────────────────────────────
    public void SaveToCloud()
    {
#if UNITY_ANDROID
        if (!IsAuthenticated)
        {
            Debug.LogWarning("[CloudSaveManager] 로그인 상태가 아닙니다. 클라우드 저장 건너뜀.");
            return;
        }

        if ((DateTime.Now - lastSaveTime).TotalSeconds < MIN_SAVE_INTERVAL_SECONDS)
        {
            Debug.Log("[CloudSaveManager] 저장 인터벌 미충족. 건너뜀.");
            return;
        }

        StartCoroutine(UploadToCloud());
#endif
    }

    // 인터벌 무시하고 강제 저장 (게임 종료 시 등)
    public void ForceSaveToCloud()
    {
#if UNITY_ANDROID
        if (!IsAuthenticated) return;
        StartCoroutine(UploadToCloud());
#endif
    }

    // ─────────────────────────────────────────────────────────
    // 앱 일시정지/종료 시 자동 저장
    // ─────────────────────────────────────────────────────────
    private void OnApplicationPause(bool pause)
    {
        if (pause) ForceSaveToCloud();
    }

    private void OnApplicationQuit()
    {
        ForceSaveToCloud();
    }

    // ─────────────────────────────────────────────────────────
    // 디버그 용도. 클라우드, 로컬 데이터 초기화
    // ─────────────────────────────────────────────────────────
    public void DeleteCloudSave()
    {
#if UNITY_ANDROID
        if (!IsAuthenticated) return;

        OpenSavedGame((status, game) =>
        {
            if (status != SavedGameRequestStatus.Success) return;

            PlayGamesPlatform.Instance.SavedGame.Delete(game);
            Debug.Log("[CloudSaveManager] 클라우드 저장 데이터 삭제 완료");
        });
#endif
    }

    public void ResetAllLocalData()
    {
        // PlayerPrefs 초기화
        PlayerPrefs.DeleteAll();

        // 로컬 파일 삭제
        string[] paths = {
        System.IO.Path.Combine(Application.persistentDataPath, "playerData.json"),
        System.IO.Path.Combine(Application.persistentDataPath, "PlayerData", "MyCards.txt"),
        System.IO.Path.Combine(Application.persistentDataPath, "MyEquipmentsData", "MyEquipments.txt")
    };

        foreach (var path in paths)
        {
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
        }

        Debug.Log("로컬 데이터 초기화 완료 - 앱 재시작 필요");
    }

    public void ResetAll()
    {
        DeleteCloudSave();
        ResetAllLocalData();

        // 씬 리로드로 모든 매니저 재초기화
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}