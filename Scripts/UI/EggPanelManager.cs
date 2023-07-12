using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EggPanelManager : MonoBehaviour
{
    [SerializeField] GameObject eggPanel;
    [SerializeField] GameObject eggImage;
    [SerializeField] GameObject kidImage;
    [SerializeField] GameObject closeButton;
    [SerializeField] PauseManager pauseManager;
    [SerializeField] GameObject oriName;
    [SerializeField] GameObject newKidText;
    [SerializeField] GameObject blackBGPanel;
    RuntimeAnimatorController kidAnim;

    private void Awake()
    {
        pauseManager = GetComponent<PauseManager>();
    }
    public void EggPanelUP(RuntimeAnimatorController anim, string name)
    {
        pauseManager.PauseGame();
        eggPanel.SetActive(true);
        EggImageUp(true);
        kidAnim = anim;
        newKidText.SetActive(true);
        oriName.GetComponent<TMPro.TextMeshProUGUI>().text = name;
        oriName.SetActive(false);

        blackBGPanel.SetActive(true);
    }

    public void EggImageUp(bool isActive)
    {
        eggImage.SetActive(isActive);
    }
    void KidImageUp(bool isActive)
    {
        kidImage.SetActive(isActive);

        newKidText.SetActive(false);
        oriName.SetActive(true);

        if (isActive) kidImage.GetComponent<Animator>().runtimeAnimatorController = kidAnim;
        closeButton.SetActive(true);
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
        closeButton.SetActive(false);
        eggPanel.SetActive(false);
        blackBGPanel.SetActive(false);
        newKidText.SetActive(false);
        oriName.SetActive(false);
    }
}
