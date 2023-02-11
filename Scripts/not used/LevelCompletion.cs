using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelCompletion : MonoBehaviour
{
    [SerializeField] float timeToComplete;
    [SerializeField] GameWinPanel GameWinPanel;

    StageTime stageTime;
    PauseManager pauseManager;


    // void Awake()
    // {
    //     stageTime= GetComponent<StageTime>();
    //     pauseManager= FindObjectOfType<PauseManager>();
    //     GameWinPanel= FindObjectOfType<GameWinPanel>(true);
    // }

    // void Update()
    // {
    //     if (stageTime.time > timeToComplete)
    //     {
    //         pauseManager.PauseGame();
    //         GameWinPanel.gameObject.SetActive(true);
    //     }
    // }
}
