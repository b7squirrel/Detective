using UnityEngine;
using System.IO;

/// <summary>
/// 플레이어의 게임 진행 데이터를 저장하고 불러오는 클래스
/// </summary>
[System.Serializable]
public class PlayerData
{
    public int currentStageNumber;          // 현재 진행 중인 스테이지 번호
    public bool isNewStage;                 // 새로운 스테이지 진입 여부
    
    public int currentCandyNumber;          // 보유한 캔디(코인) 수
    public int currentHighCoinNumber;       // 보유한 하이 코인 수 (프리미엄 화폐)
    public int currentLightningNumber;      // 보유한 번개(라이트닝) 수
    public int currentKillNumber;           // 총 처치 수 (통계용)
}

/// <summary>
/// 플레이어 데이터의 저장/로드 및 관리를 담당하는 매니저 클래스
/// JSON 파일로 로컬에 저장
/// </summary>
public class PlayerDataManager : MonoBehaviour
{
    [SerializeField] PlayerData playerData; // 디버그를 위해 직렬화 (인스펙터에서 확인 가능)
    string filePath;                        // 저장 파일 경로
    bool isStageCleared;                    // 현재 스테이지 클리어 여부 (세션 내 임시 변수)
    
    void Awake()
    {
        // 저장 파일 경로 설정 (persistentDataPath: 플랫폼별 영구 저장 경로)
        filePath = Path.Combine(Application.persistentDataPath, "playerData.json");
        
        // 게임 시작 시 저장된 데이터 불러오기
        LoadStageNumberData();
        LoadCandyNumberData();
    }
    
    #region Stage
    // 현재 스테이지 번호 반환
    public int GetCurrentStageNumber()
    {
        return playerData.currentStageNumber;
    }
    
    // 스테이지 번호 설정 및 저장
    public void SetCurrentStageNumber(int stageNumber)
    {
        playerData.currentStageNumber = stageNumber;
        SavePlayerData();
    }
    
    // 플레이어 데이터를 JSON 파일로 저장
    void SavePlayerData()
    {
        string jsonData = JsonUtility.ToJson(playerData);
        File.WriteAllText(filePath, jsonData);
    }
    
    // 스테이지 데이터 로드. 파일이 없으면 초기값으로 생성
    void LoadStageNumberData()
    {
        if (File.Exists(filePath))
        {
            // 저장된 파일이 있으면 불러오기
            string jsonData = File.ReadAllText(filePath);
            playerData = JsonUtility.FromJson<PlayerData>(jsonData);
            // Debug.LogError($"플레이어 데이터 매니져.크리스탈 개수 = {playerData.currentHighCoinNumber}");
        }
        else
        {
            // 저장 파일이 없으면 초기값으로 새 데이터 생성
            playerData = new PlayerData
            {
                currentStageNumber = 1,         // 초기 스테이지는 1
                currentLightningNumber = 60,    // 초기 번개 60개
                currentCandyNumber = 100,       // 초기 캔디 100개
                currentHighCoinNumber = 50      // 초기 하이 코인 50개
            };
            SavePlayerData();
        }
    }
    
    // 새로운 스테이지 진입 여부 반환
    public bool IsNewStage() { return playerData.isNewStage; }
    
    // 새로운 스테이지 진입 여부 설정 및 저장
    public void SetIsNewStage(bool isNew)
    {
        playerData.isNewStage = isNew;
        SavePlayerData();
    }
    
    // 현재 스테이지 클리어 표시 (세션 내에서만 유효)
    public void SetCurrentStageCleared()
    {
        isStageCleared = true;
    }
    #endregion
    
    // 현재 보유한 하이 코인 수 반환
    public int GetCurrentHighCoinNumber()
    {
        return playerData.currentHighCoinNumber;
    }

    // 하이 코인 추가 및 저장
    public void AddHighCoin(int highCoinNumToAdd)
    {
        playerData.currentHighCoinNumber += highCoinNumToAdd;
        SavePlayerData();
    }
    
    // 하이 코인을 특정 값으로 설정 및 저장
    public void SetCristalNumberAs(int cristalNumberToSet)
    {
        playerData.currentHighCoinNumber = cristalNumberToSet;
        SavePlayerData();
    }
    
    // 현재 보유한 번개 수 반환
    public int GetCurrentLightningNumber()
    {
        return playerData.currentLightningNumber;
    }
    
    // 번개 추가 및 저장
    public void AddLightning(int lightningToAdd)
    {
        playerData.currentLightningNumber += lightningToAdd;
        SavePlayerData();
    }
    
    #region Candy
    // 현재 보유한 캔디 수 반환
    public int GetCurrentCandyNumber()
    {
        return playerData.currentCandyNumber;
    }
    
    // 캔디 추가 및 저장
    public void AddCandyNumber(int candyNumberToAdd)
    {
        playerData.currentCandyNumber += candyNumberToAdd;
        Debug.Log("Add Candy Number " + candyNumberToAdd);
        SavePlayerData();
    }
    
    // 캔디 수를 특정 값으로 설정 및 저장
    public void SetCandyNumberAs(int candyNumberToSet)
    {
        playerData.currentCandyNumber = candyNumberToSet;
        SavePlayerData();
    }
    
    // 캔디 데이터 로드. 파일이 없으면 초기값으로 생성
    void LoadCandyNumberData()
    {
        if (File.Exists(filePath))
        {
            // 저장된 파일이 있으면 불러오기
            string jsonData = File.ReadAllText(filePath);
            playerData = JsonUtility.FromJson<PlayerData>(jsonData);
        }
        else
        {
            // 저장 파일이 없으면 초기값으로 새 데이터 생성
            playerData = new PlayerData
            {
                currentCandyNumber = 1 // 초기 캔디 수
            };
            SavePlayerData();
        }
    }
    #endregion
    
    #region 나가기 전 재화 저장
    // 게임 종료 전 현재 재화(코인)와 스테이지 진행 상황 저장
    // 스테이지 클리어 시 다음 스테이지로 진행
    public void SaveResourcesBeforeQuitting()
    {
        StageInfo stageinfo = GetComponent<StageInfo>();
        int currentStage = GetCurrentStageNumber();
        
        // 스테이지 저장 로직
        // 최종 스테이지가 아니고 클리어했다면 다음 스테이지로 진행
        if (stageinfo.IsFinalStage(currentStage) == false)
        {
            if(isStageCleared)
            {
                Debug.Log("현재 스테이지 = " + currentStage);
                currentStage++;
                SetCurrentStageNumber(currentStage);
                Debug.Log("다음 스테이지 = " + currentStage);
                isStageCleared = false;
            }
        }
        
        // 현재 보유한 코인 수를 가져와서 저장
        int coinNum = FindObjectOfType<CoinManager>().GetCurrentCoins();
        SetCandyNumberAs(coinNum);

        int hiCoinNum = FindObjectOfType<CristalManager>().GetCurrentCristals();
        // Debug.LogError($"크리스탈 개수 = {hiCoinNum}");
        SetCristalNumberAs(hiCoinNum);
        
        // 게임 일시정지
        FindObjectOfType<PauseManager>().PauseGame();
    }
    #endregion
}