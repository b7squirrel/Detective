using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePanelManager : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] GameObject greyBase;
    [SerializeField] GameObject PanelWaitingPopUp;
    PauseManager pauseManager;
    [SerializeField] ExperienceBar experienceBar;

    [SerializeField] List<UpgradeButton> upgradeButtons;

    Animator anim;
    Level level;

    // 사운드
    [SerializeField] AudioClip clickSound;

    void Awake()
    {
        pauseManager = GetComponent<PauseManager>();
        anim = panel.GetComponent<Animator>();
        level = FindObjectOfType<Level>();
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

        greyBase.SetActive(true);
        panel.SetActive(true);

        int expToLevelUp = level.GetExpToLevelUp();
        experienceBar.ExpBarBlink(expToLevelUp);

        for (int i = 0; i < upgradeData.Count; i++)
        {
            upgradeButtons[i].gameObject.SetActive(true);
            upgradeButtons[i].Set(upgradeData[i]);
        }
        StartCoroutine(PopUpPanel());
    }

    // 업그레이드 버튼을 누르면 실행
    public void Upgrade(int pressButtonID)
    {
        for (int i = 0; i < upgradeButtons.Count; i++)
        {
            if (upgradeButtons[i].IsClicked)
            {
                Debug.Log("Clicked");
                return;
            }
        }
        GameManager.instance.player.GetComponent<Level>().Upgrade(pressButtonID);
        StartCoroutine(SelectionEvent(pressButtonID));
        SoundManager.instance.Play(clickSound);
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
        experienceBar.ExpBarIdle();
        level.ApplyUpdatedLevel();
        StartCoroutine(VanishPanel());
    }

    IEnumerator SelectionEvent(int pressButtonID)
    {
        for (int i = 0; i < upgradeButtons.Count; i++)
        {
            if (i == pressButtonID)
            {
                upgradeButtons[i].Selected();
            }
            else
            {
                upgradeButtons[i].UnSelected();
            }
        }

        yield return new WaitForSecondsRealtime(.25f); // 카드 누름 애니메이션 + 잠시 홀드 하는 시간

        ClosePanel();
    }

    void HideButtons()
    {
        for (int i = 0; i < upgradeButtons.Count; i++)
        {
            upgradeButtons[i].ResetUnseletedPanel();
            upgradeButtons[i].gameObject.SetActive(false);
        }
    }

    IEnumerator PopUpPanel()
    {
        anim.SetTrigger("PopUp");
        PanelWaitingPopUp.SetActive(true);
        yield return new WaitForSecondsRealtime(.1f); // popup animation 길이만큼
        PanelWaitingPopUp.SetActive(false);
    }

    IEnumerator VanishPanel()
    {
        anim.SetTrigger("Close");
        yield return new WaitForSecondsRealtime(.2f); // close animation 길이만큼
        pauseManager.UnPauseGame();
        HideButtons();
        greyBase.SetActive(false);
        panel.SetActive(false);
        Player.instance.GetComponent<Level>().CheckLevelUp();
    }
}
