using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradePanelManager : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] GameObject greyBase;
    [SerializeField] GameObject PanelWaitingPopUp;
    PauseManager pauseManager;
    [SerializeField] ExperienceBar experienceBar;

    [SerializeField] List<UpgradeButton> upgradeButtons;

    [SerializeField] GameObject confetti; // 폭죽 숨기고 보이게 하기

    Animator anim;
    Level level;

    // 사운드
    [SerializeField] AudioClip clickSound;
    [SerializeField] AudioClip[] LevelUpSounds;

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

        for (int i = 0; i < LevelUpSounds.Length; i++)
        {
            SoundManager.instance.Play(LevelUpSounds[i]);
        }

        Clean();
        pauseManager.PauseGame();

        if(confetti != null) confetti.SetActive(true);

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
        if(confetti != null) confetti.SetActive(false);

        anim.SetTrigger("Close");
        yield return new WaitForSecondsRealtime(.2f); // close animation 길이만큼
        GameManager.instance.popupManager.IsUIDone = true; // Level.CheckLevelUp 전에 있어야 함. 큐에 쌓이므로
        pauseManager.UnPauseGame();
        HideButtons();
        greyBase.SetActive(false);
        panel.SetActive(false);
        Player.instance.GetComponent<Level>().CheckLevelUp();

    }
}
