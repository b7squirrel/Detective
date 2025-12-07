using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] Slider tabSlider;
    [SerializeField] Animator tabSliderAnim; // 왼쪽, 중앙, 오른쪽에 따라 모양이 달라지도록
    [SerializeField] GameObject[] tabPanels; // 시작할 떄 모두 비활성화 시킴. 빌드할 때 켜놓든 꺼놓든 상관없음
    [SerializeField] GameObject[] PanelBGs; // Safe Area 바깥쪽에 BG 위치시키기. 화면에 꽉 차도록
    int tabIndex; // 지금 어떤 탭에 있는지 slot action에 알려주려고

    [SerializeField] RectTransform[] BtnRect;
    [SerializeField] RectTransform[] BtnImageRect;
    float[] pos = new float[SIZE];
    const int SIZE = 5;
    int targetIndex;

    Animator[] tabAnims = new Animator[5];

    [Header("아래쪽 탭 제어")]
    [SerializeField] RectTransform tabs;
    Button[] tabButtons = new Button[5];

    [Header("윗쪽 탭 제어")]
    [SerializeField] RectTransform upperTabs;

    [Header("Sound")]
    [SerializeField] AudioClip tabTouched;
    [SerializeField] AudioClip startButtonTouched;
    [SerializeField] AudioClip bgm;

    [Header("Black Transition")]
    [SerializeField] GameObject blackScreen;

    [Header("Swipe Transition")]
    [SerializeField] Animator loadingSwipe;

    [Header("합성 성공 플래그")]
    // 합성 성공한 상태인 것을 저장. 성공한 상태에서 탭해서 나가지 않고, 아래쪽 탭으로 다른 화면으로 넘어갈 경우를 대비
    bool mergeFinished;
    UpgradePanelManager upPanelManager;

    bool slotSwapFinished;

    // 리드오리 카드 데이터
    CardData lead;

    void Start()
    {
        InitBlackTransition();

        for (int i = 0; i < SIZE; i++)
        {
            pos[i] = (1f / 4f) * i;
        }
        SetTabPos(2);

        for (int i = 0; i < tabPanels.Length; i++)
        {
            tabPanels[i].SetActive(false);
            // PanelBGs[i].SetActive(false);
        }

        for (int i = 0; i < BtnImageRect.Length; i++)
        {
            tabAnims[i] = BtnImageRect[i].GetComponent<Animator>();
            BtnImageRect[i].transform.GetChild(0).gameObject.SetActive(false); // 임시로 아이콘 밑의 text는 모두 숨기자
        }

        PlayBGM(); // awake에서 Music Manager가 준비가 안되어 있을 수 있으므로 코루틴으로 약간 기다린 후 재생

        // 저장된 정보를 불러와서 탭 설정하기
        // SetButtonActive(0, false);
        // SetButtonActive(1, false);
        // SetButtonActive(3, false);
        // SetButtonActive(4, false);
    }

    void Update()
    {
        for (int i = 0; i < SIZE; i++)
        {
            BtnRect[i].sizeDelta = new Vector2(i == targetIndex ? 360f : 180f, BtnRect[i].sizeDelta.y);
        }

        if (Time.time < 0.1f) return;

        StartCoroutine(ActivatePanel());
    }

    IEnumerator ActivatePanel()
    {
        yield return null;
        for (int i = 0; i < SIZE; i++)
        {
            Vector3 BtnTargetPos = BtnRect[i].anchoredPosition3D;
            BtnTargetPos.y = -40f;
            Vector3 BtnTargetScale = Vector3.one;
            //bool textActive = true;

            tabAnims[i].SetBool("Up", false);
            tabAnims[i].SetBool("Idle", true);


            if (i == targetIndex)
            {
                BtnTargetPos.y = -15f;
                BtnTargetScale = new Vector3(1.7f, 1.7f, 1);
                tabAnims[i].SetBool("Up", true);
                tabAnims[i].SetBool("Idle", false);
                //textActive = false;
            }

            BtnImageRect[i].anchoredPosition3D = Vector3.Lerp(BtnImageRect[i].anchoredPosition3D, BtnTargetPos, 1f);
            BtnImageRect[i].localScale = Vector3.Lerp(BtnImageRect[i].localScale, BtnTargetScale, .5f);
            BtnImageRect[i].transform.GetChild(0).gameObject.SetActive(false); // setActive의 인자로 textActive를 넘기지만 임시로 모두 숨김
            tabPanels[i].SetActive(i == targetIndex);
            // PanelBGs[i].SetActive(i == targetIndex);

            tabIndex = i == targetIndex ? i : tabIndex; // 어떤 탭에 들어와 있는지 저장
        }
    }

    public void SetTabPos(int pressBtnID)
    {
        tabSlider.value = pos[pressBtnID];
        targetIndex = pressBtnID;
        
        if (pressBtnID == 0)
        {
            tabSliderAnim.SetTrigger("Left");
            return;
        }
        if (pressBtnID == 4)
        {
            tabSliderAnim.SetTrigger("Right");
            return;
        }
        else
        {
            tabSliderAnim.SetTrigger("Center");
        }

        // 합성 성공 후 탭을 해서 합성 패널을 초기화 시키지 않고 나올 경우를 대비해서 
        if (pressBtnID != 3)
        {
            if(mergeFinished)
            {
                
            }
        }
    }

    public void PlayClickSound()
    {
        SoundManager.instance.Play(tabTouched);
    }
    public void PlayStartButtonClickSound()
    {
        SoundManager.instance.Play(startButtonTouched);
    }
    void PlayBGM()
    {
        StartCoroutine(PlayBGMCo());
    }
    IEnumerator PlayBGMCo()
    {
        yield return new WaitForSeconds(.2f);
        MusicManager.instance.Play(bgm, true);
    }

    void InitBlackTransition()
    {
        blackScreen.SetActive(false);
    }

    // 스타트 버튼을 누르면 이벤트로 실행
    public void StartTransition()
    {
        // transition animation 이 끝나면 loading scene manager 호출
        loadingSwipe.gameObject.SetActive(true);
        loadingSwipe.SetTrigger("Close");

        StartCoroutine(LoadScenes());
    }

    IEnumerator LoadScenes()
    {
        yield return new WaitForSecondsRealtime(1.23f);
        LoadingSceneManager loadingSceneManager = FindObjectOfType<LoadingSceneManager>();
        loadingSceneManager.LoadScenes();
    }

    #region 탭 버튼 활성/비활성 제어
    public void SetButtonActive(int buttonIndex, bool active)
    {
        BtnRect[buttonIndex].GetComponent<Button>().interactable = active;
        BtnImageRect[buttonIndex].GetComponent<Image>().enabled = active;
    }
    #endregion
    // UpPanelManager의 UpgradeUICo 와 탭해서 계속하기 버튼에서 참조.
    public void SetActiveBottomTabs(bool active)
    {
        tabButtons = tabs.GetComponentsInChildren<Button>();
        if (active)
        {
            // 탭을 원래대로 되돌림
            tabs.DOAnchorPosY(68f, .5f);

            // 탭 버튼이 작동하도록 하기
            for (int i = 0; i < tabButtons.Length; i++)
            {
                tabButtons[i].interactable = true;
            }
        }
        else
        {
            // 탭 버튼이 작동하지 않도록
            for (int i = 0; i < tabButtons.Length; i++)
            {
                tabButtons[i].interactable = false;
            }
            // 탭을 화면 아래로 이동
            tabs.anchoredPosition = new Vector2(tabs.anchoredPosition.x, -300f);
        }
    }
    public void SetActiveTopTabs(bool active)
    {
        if (active)
        {
            upperTabs.DOAnchorPosY(0f, 0.5f);
        }
        else
        {
            upperTabs.anchoredPosition = new Vector2(upperTabs.anchoredPosition.x, 300f);
        }
    }
    public void SetSlotSwapState(bool isFinished)
    {
        slotSwapFinished = isFinished;
    }

    public int GetTabIndex()
    {
        return tabIndex;
    }

    public void SetLeadCardData(CardData _lead)
    {
        this.lead = _lead;
    }
    public CardData GetLeadCardData()
    {
        if(lead == null) 
        {
            Logger.Log("리드 오리가 아직 저장되지 않았습니다. 확인해 주세요.");
            return null;
        }
        return lead;
    }
}