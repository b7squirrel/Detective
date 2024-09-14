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

    // ������ ��ư�� �̺�Ʈ�� ȣ��
    public void GoToMainMenu()
    {
        Debug.Log("���� �޴����� ȣ��");
        panelPause.SetActive(false);
        BG.SetActive(false);

        // ��ȭ ����
        PlayerDataManager playerData = FindObjectOfType<PlayerDataManager>();
        playerData.SaveResourcesBeforeQuitting(); // ���������� ���� ����
        int coinNum = FindObjectOfType<CoinManager>().GetCurrentCoins();

        UnPause();
        GameManager.instance.DestroyStartingData();

        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }
    // ����Ƽ �̺�Ʈ�� ����
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
