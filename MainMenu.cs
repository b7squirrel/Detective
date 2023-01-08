using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject panelPause;
    [SerializeField] GameObject panelMainMenu;
    PauseManager pauseManager;
    bool isPaused;


    void Awake()
    {
        pauseManager= GetComponent<PauseManager>();
    }

    public void PauseButtonDown()
    {
        if (isPaused)
        { 
            UnPause();
            return;
        }
        pauseManager.PauseGame();
        panelPause.SetActive(true);
        isPaused= true;
    }

    public void UnPause()
    {
        pauseManager.UnPauseGame();
        panelPause.SetActive(false);
        isPaused = false;
    }

    public void GoToMainMenu()
    {
        panelPause.SetActive(false);
        UnPause();
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }
}
