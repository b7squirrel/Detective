using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject panelPause;
    [SerializeField] GameObject panelAreYouSure;
    [SerializeField] GameObject BG;
    
    public UnityEvent<bool> OnPauseButtonPressed;
    
    PauseManager pauseManager;
    bool isPaused;
    bool isLoadingScene = false; // 중복 로딩 방지
    
    void Awake()
    {
        try
        {
            pauseManager = GetComponent<PauseManager>();
            if (pauseManager == null)
            {
                Debug.LogWarning("PauseManager 컴포넌트를 찾을 수 없습니다.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"MainMenu Awake 오류: {e.Message}");
        }
    }
    
    public void PauseButtonDown()
    {
        try
        {
            if (isLoadingScene) return; // 씬 로딩 중이면 무시
            
            isPaused = true;
            OnPauseButtonPressed?.Invoke(isPaused);
            
            if (pauseManager != null)
            {
                pauseManager.PauseGame();
            }
            
            SetUIActive(true);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"PauseButtonDown 오류: {e.Message}");
        }
    }
    
    public void UnPause()
    {
        try
        {
            if (isLoadingScene) return; // 씬 로딩 중이면 무시
            
            if (pauseManager != null)
            {
                pauseManager.UnPauseGame();
            }
            
            SetUIActive(false);
            isPaused = false;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UnPause 오류: {e.Message}");
        }
    }
    
    void SetUIActive(bool active)
    {
        try
        {
            if (panelPause != null)
                panelPause.SetActive(active);
            
            if (BG != null)
                BG.SetActive(active);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SetUIActive 오류: {e.Message}");
        }
    }
    
    public void AreYouSure()
    {
        try
        {
            if (isLoadingScene) return;
            
            if (panelAreYouSure != null)
            {
                panelAreYouSure.gameObject.SetActive(true);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"AreYouSure 오류: {e.Message}");
        }
    }
    
    // 보통은 버튼의 이벤트로 호출
    public void GoToMainMenu()
    {
        if (isLoadingScene)
        {
            Debug.Log("이미 메인메뉴로 이동 중입니다.");
            return;
        }
        
        StartCoroutine(GoToMainMenuCoroutine());
    }
    
    IEnumerator GoToMainMenuCoroutine()
    {
        isLoadingScene = true;
        Debug.Log("메인메뉴로 이동 시작");
        
        // UI 즉시 비활성화
        SetUIActive(false);
        
        // 게임 일시정지 해제
        if (pauseManager != null)
        {
            pauseManager.UnPauseGame();
        }
        isPaused = false;
        
        // 데이터 저장 (안전하게)
        yield return StartCoroutine(SaveGameDataSafely());
        
        // GameManager 정리
        CleanupGameManager();
        
        // 짧은 대기 시간 (안정성을 위해)
        yield return new WaitForSecondsRealtime(0.1f);
        
        // 씬 로딩
        Debug.Log("MainMenu 씬 로딩 시작");
        LoadMainMenuScene();
    }
    
    IEnumerator SaveGameDataSafely()
    {
        Debug.Log("게임 데이터 저장 시작");
        
        // PlayerDataManager 찾기 및 저장
        bool playerDataSaved = false;
        try
        {
            PlayerDataManager playerData = FindObjectOfType<PlayerDataManager>();
            if (playerData != null)
            {
                playerData.SaveResourcesBeforeQuitting();
                Debug.Log("플레이어 데이터 저장 완료");
                playerDataSaved = true;
            }
            else
            {
                Debug.LogWarning("PlayerDataManager를 찾을 수 없습니다.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"PlayerDataManager 저장 오류: {e.Message}");
        }
        
        // 프레임 대기 (안정성)
        yield return null;
        
        // CoinManager에서 코인 정보 가져오기
        try
        {
            CoinManager coinManager = FindObjectOfType<CoinManager>();
            if (coinManager != null)
            {
                int coinNum = coinManager.GetCurrentCoins();
                Debug.Log($"현재 코인: {coinNum}");
            }
            else
            {
                Debug.LogWarning("CoinManager를 찾을 수 없습니다.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CoinManager 처리 오류: {e.Message}");
        }
        
        // 프레임 대기
        yield return null;
        
        // 힌트 리셋
        try
        {
            HintsOnLoading hintsManager = FindObjectOfType<HintsOnLoading>();
            if (hintsManager != null)
            {
                hintsManager.ResetHint();
                Debug.Log("힌트 리셋 완료");
            }
            else
            {
                Debug.LogWarning("HintsOnLoading을 찾을 수 없습니다.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"HintsOnLoading 처리 오류: {e.Message}");
        }
        
        Debug.Log("게임 데이터 저장 완료");
    }
    
    void CleanupGameManager()
    {
        try
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.DestroyStartingData();
                Debug.Log("GameManager 정리 완료");
            }
            else
            {
                Debug.LogWarning("GameManager.instance가 null입니다.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CleanupGameManager 오류: {e.Message}");
        }
    }
    
    void LoadMainMenuScene()
    {
        try
        {
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"씬 로딩 오류: {e.Message}");
            // 오류 발생 시 플래그 리셋
            isLoadingScene = false;
        }
    }
    
    // 유니티 이벤트에 붙임
    // 씬이 로드되면서 버튼 이펙트가 재생이 잘 안될 경우, 이펙트 재생이 끝난 후 씬을 로드하도록
    public void GoToMainMenuAfter(float timeToWait)
    {
        if (isLoadingScene)
        {
            Debug.Log("이미 메인메뉴로 이동 중입니다.");
            return;
        }
        
        StartCoroutine(GoToMainMenuDelayed(timeToWait));
    }
    
    IEnumerator GoToMainMenuDelayed(float timeToWait)
    {
        Debug.Log($"{timeToWait}초 후 메인메뉴로 이동");
        
        if (timeToWait > 0)
        {
            yield return new WaitForSecondsRealtime(timeToWait);
        }
        
        GoToMainMenu();
    }
    
    // 강제로 메인메뉴로 이동 (디버그용)
    public void ForceGoToMainMenu()
    {
        try
        {
            Debug.Log("강제 메인메뉴 이동");
            Time.timeScale = 1f; // 시간 스케일 복원
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ForceGoToMainMenu 오류: {e.Message}");
        }
    }
    
    // 간단한 버전 (코루틴 없이)
    public void GoToMainMenuSimple()
    {
        if (isLoadingScene) return;
        
        isLoadingScene = true;
        Debug.Log("간단한 메인메뉴 이동");
        
        // UI 비활성화
        SetUIActive(false);
        
        // 게임 일시정지 해제
        if (pauseManager != null)
        {
            pauseManager.UnPauseGame();
        }
        
        // 데이터 저장 (동기적으로)
        SaveDataSynchronously();
        
        // GameManager 정리
        CleanupGameManager();
        
        // 씬 로딩
        LoadMainMenuScene();
    }
    
    void SaveDataSynchronously()
    {
        try
        {
            // PlayerDataManager 저장
            PlayerDataManager playerData = FindObjectOfType<PlayerDataManager>();
            if (playerData != null)
            {
                playerData.SaveResourcesBeforeQuitting();
                Debug.Log("플레이어 데이터 저장 완료");
            }
            
            // 힌트 리셋
            HintsOnLoading hintsManager = FindObjectOfType<HintsOnLoading>();
            if (hintsManager != null)
            {
                hintsManager.ResetHint();
                Debug.Log("힌트 리셋 완료");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"동기 데이터 저장 오류: {e.Message}");
        }
    }
    
    void OnDestroy()
    {
        try
        {
            // 혹시 모를 상황에 대비해 시간 스케일 복원
            if (Time.timeScale != 1f)
            {
                Time.timeScale = 1f;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"MainMenu OnDestroy 오류: {e.Message}");
        }
    }
}