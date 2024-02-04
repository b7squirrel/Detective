using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject panelPause;
    [SerializeField] GameObject panelMainMenu;
    [SerializeField] GameObject panelAreYouSure;
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
    }

    public void UnPause()
    {
        pauseManager.UnPauseGame();
        panelPause.SetActive(false);
        isPaused = false;
    }

    public void AreYouSure()
    {
        panelAreYouSure.gameObject.SetActive(true);
    }

    // ������ ��ư�� �̺�Ʈ�� ȣ��
    public void GoToMainMenu()
    {
        panelPause.SetActive(false);
        UnPause();
        GameManager.instance.DestroyStartingData();
        Debug.Log("Go to Main Menu 3");

        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }
    // ����Ƽ �̺�Ʈ�� ����
    public void GoToMainMenuAfter(float timeToWait)
    {
        Debug.Log("Go to Main Menu 1");
        StartCoroutine(GoToMainMenu(timeToWait));
    }
    IEnumerator GoToMainMenu(float timeToWait)
    {
        yield return new WaitForSecondsRealtime(timeToWait);
        Debug.Log("Go to Main Menu 2");
        GoToMainMenu();
    }
}
