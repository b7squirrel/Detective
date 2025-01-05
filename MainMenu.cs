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
    // public Action<bool> OnPauseButtonPressed;

    void Awake()
    {
        pauseManager= GetComponent<PauseManager>();
    }

    public void PauseButtonDown()
    {
        isPaused= true;
        OnPauseButtonPressed?.Invoke(isPaused);
        
        pauseManager.PauseGame();
        panelPause.SetActive(true);
        BG.SetActive(true);
    }

    public void UnPause()
    {
        pauseManager.UnPauseGame();
        panelPause.SetActive(false);
        BG.SetActive(false);
        isPaused = false;
    }

    public void AreYouSure()
    {
        panelAreYouSure.gameObject.SetActive(true);
    }

    // 보통은 버튼의 이벤트로 호출
    public void GoToMainMenu()
    {
        panelPause.SetActive(false);
        BG.SetActive(false);

        // 재화 저장
        PlayerDataManager playerData = FindObjectOfType<PlayerDataManager>();
        playerData.SaveResourcesBeforeQuitting(); // 스테이지와 동전 저장
        int coinNum = FindObjectOfType<CoinManager>().GetCurrentCoins();

        // 힌트 리셋
        FindObjectOfType<HintsOnLoading>().ResetHint();

        UnPause();
        GameManager.instance.DestroyStartingData();

        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }
    // 유니티 이벤트에 붙임
    // 씬이 로드되면서 버튼 이펙트가 재생이 잘 안될 경우, 이펙트 재생이 끝난 후 씬을 로드하도록
    public void GoToMainMenuAfter(float timeToWait) 
    {
        StartCoroutine(GoToMainMenu(timeToWait));
    }
    IEnumerator GoToMainMenu(float timeToWait)
    {
        yield return new WaitForSecondsRealtime(timeToWait);
        GoToMainMenu();
    }
}