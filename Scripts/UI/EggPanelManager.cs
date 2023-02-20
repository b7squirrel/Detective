using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EggPanelManager : MonoBehaviour
{
    [SerializeField] GameObject eggImage;
    [SerializeField] GameObject kidImage;
    [SerializeField] PauseManager pauseManager;
    public RuntimeAnimatorController KidAnim { get; set; }

    private void OnEnable()
    {
        EggImageUp(true);
    }
    public void EggImageUp(bool isActive)
    {
        eggImage.SetActive(isActive);
    }
    void KidImageUp(bool isActive)
    {
        kidImage.SetActive(isActive);
        if(isActive) kidImage.GetComponent<Animator>().runtimeAnimatorController = KidAnim;
    }
    public void EggAnimFinished()
    {
        KidImageUp(true);
    }
    public void CloseButtonPressed()
    {
        EggImageUp(false);
        KidImageUp(false);
        gameObject.SetActive(false);
    }
}
