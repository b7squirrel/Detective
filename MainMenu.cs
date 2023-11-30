using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

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

    public void GoToMainMenu()
    {
        panelPause.SetActive(false);
        UnPause();
        GameManager.instance.DestroyStartingData();
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }
}
