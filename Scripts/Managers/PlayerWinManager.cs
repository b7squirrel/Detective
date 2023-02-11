using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWinManager : MonoBehaviour
{
    [SerializeField] GameObject winMessagePanel;
    [SerializeField] DataContainer dataContainer;
    PauseManager pauseManager;
    void Start()
    {
        pauseManager = GetComponent<PauseManager>();
    }

    void Update()
    {
        
    }

    public void Win() {
        {
            winMessagePanel.SetActive(true);
            pauseManager.PauseGame();
            dataContainer.StageComplete(0);
        }
    }
}
