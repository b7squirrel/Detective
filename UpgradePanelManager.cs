using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradePanelManager : MonoBehaviour
{
    [SerializeField] GameObject panel;
    PauseManager pauseManager;
    [SerializeField] ExpBarAnimation expBarAnim;

    [SerializeField] List<UpgradeButton> upgradeButtons;

    void Awake()
    {
        pauseManager = GetComponent<PauseManager>();
    }

    void Start()
    {
        HideButtons();
    }

    public void OpenPanel(List<UpgradeData> upgradeData)
    {
        // GameManager.instance.joystick.SetActive(false);
        Clean();
        pauseManager.PauseGame(); 
        panel.SetActive(true);

        for (int i = 0; i < upgradeData.Count; i++)
        {
            upgradeButtons[i].gameObject.SetActive(true);
            upgradeButtons[i].Set(upgradeData[i]);
        }
    }
    public void Upgrade(int pressButtonID)
    {
        GameManager.instance.player.GetComponent<Level>().Upgrade(pressButtonID);
        ClosePanel();
        // GameManager.instance.joystick.SetActive(true);
    }

    public void Clean()
    {
        for (int i = 0; i < upgradeButtons.Count; i++)
        {
            upgradeButtons[i].Clean();
        }
    }

    // Skip 버튼을 누르면 한 번 더 물어보기
    public void ClosePanel()
    {
        expBarAnim.DisableExpFillBar();

        HideButtons();
        pauseManager.UnPauseGame();
        panel.SetActive(false);
    }

    private void HideButtons()
    {
        for (int i = 0; i < upgradeButtons.Count; i++)
        {
            upgradeButtons[i].gameObject.SetActive(false);
        }
    }
}
