using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Events;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject panelPause;
    [SerializeField] GameObject panelMainMenu;
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

    public void GoToMainMenu()
    {
        panelPause.SetActive(false);
        UnPause();
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }
}
