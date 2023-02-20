using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EggPanelManager : MonoBehaviour
{
    [SerializeField] GameObject eggPanel;
    [SerializeField] GameObject eggImage;
    [SerializeField] GameObject kidImage;
    [SerializeField] PauseManager pauseManager;
    RuntimeAnimatorController kidAnim;

    private void Awake()
    {
        pauseManager = GetComponent<PauseManager>();
    }
    public void EggPanelUP(RuntimeAnimatorController anim)
    {
        pauseManager.PauseGame();
        eggPanel.SetActive(true);
        EggImageUp(true);
        kidAnim = anim;
    }

    public void EggImageUp(bool isActive)
    {
        eggImage.SetActive(isActive);
    }
    void KidImageUp(bool isActive)
    {
        kidImage.SetActive(isActive);
        if(isActive) kidImage.GetComponent<Animator>().runtimeAnimatorController = kidAnim;
    }
    public void EggAnimFinished()
    {
        KidImageUp(true);
    }
    public void CloseButtonPressed()
    {
        EggImageUp(false);
        KidImageUp(false);
        pauseManager.UnPauseGame();
        eggPanel.SetActive(false);
    }
}
