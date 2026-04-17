using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] Slider tabSlider;
    [SerializeField] Animator tabSliderAnim; // 왼쪽, 중앙, 오른쪽에 따라 모양이 달라지도록
    [SerializeField] GameObject[] tabPanels; // 시작할 떄 모두 비활성화 시킴. 빌드할 때 켜놓든 꺼놓든 상관없음
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

    [Header("Swipe Transition")]
    [SerializeField] Animator loadingSwipe;

    [Header("합성 성공 플래그")]
    // 합성 성공한 상태인 것을 저장. 성공한 상태에서 탭해서 나가지 않고, 아래쪽 탭으로 다른 화면으로 넘어갈 경우를 대비
    bool mergeFinished;

    [Header("튜토리얼")]
    [SerializeField] RuntimeAnimatorController[] defaultTabControllers;
    [SerializeField] RuntimeAnimatorController[] tutorialTabControllers;
    [SerializeField] Image Tab_Base;
    [SerializeField] Sprite[] Tab_BaseSprites;
    [SerializeField] RuntimeAnimatorController defaultTabSlideHandleCon;
    [SerializeField] RuntimeAnimatorController darkTabSlideHandleCon;
    [SerializeField] Animator tabSliderHandleAnim;

    bool slotSwapFinished;

    // 리드오리 카드 데이터
    CardData lead;



    void Start()
    {
        SetTutorialMode(false);

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

        // ✅ 추가: 튜토리얼 단계에 따라 초기 탭 상태 설정
        InitTabsByTutorialStep();
    }
    void OnEnable()
    {
        // ✅ 추가: 단계 변경 이벤트 구독
        TutorialManager.OnStepChanged += OnTutorialStepChanged;
    }

    void OnDisable()
    {
        // ✅ 추가: 구독 해제
        TutorialManager.OnStepChanged -= OnTutorialStepChanged;
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

            tabAnims[i].SetBool("Up", false);
            tabAnims[i].SetBool("Idle", true);

            if (i == targetIndex)
            {
                BtnTargetPos.y = -15f;
                BtnTargetScale = new Vector3(1.7f, 1.7f, 1);
                tabAnims[i].SetBool("Up", true);
                tabAnims[i].SetBool("Idle", false);
                BtnImageRect[i].transform.GetChild(0).gameObject.SetActive(true); // 선택된 탭만 텍스트 보이기
            }
            else
            {
                BtnImageRect[i].transform.GetChild(0).gameObject.SetActive(false); // 다른 탭은 텍스트 숨기기
            }

            BtnImageRect[i].anchoredPosition3D = Vector3.Lerp(BtnImageRect[i].anchoredPosition3D, BtnTargetPos, 1f);
            BtnImageRect[i].localScale = Vector3.Lerp(BtnImageRect[i].localScale, BtnTargetScale, .5f);
            tabPanels[i].SetActive(i == targetIndex);

            tabIndex = i == targetIndex ? i : tabIndex;
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
            if (mergeFinished)
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

    // 스타트 버튼을 누르면 이벤트로 실행. 레귤러 스테이지
    public void StartTransition()
    {
        // transition animation 이 끝나면 loading scene manager 호출
        loadingSwipe.gameObject.SetActive(true);
        loadingSwipe.SetTrigger("Close");

        StartCoroutine(LoadRegularMode());
    }

    // 무한 모드 스타트 버튼을 누르면 이벤트로 실행. 무한 스테이지
    public void StartInfiniteMode()
    {
        loadingSwipe.gameObject.SetActive(true);
        loadingSwipe.SetTrigger("Close");
        StartCoroutine(LoadInfiniteMode());
    }

    IEnumerator LoadRegularMode()
    {
        yield return new WaitForSecondsRealtime(1.23f);
        LoadingSceneManager loadingSceneManager = FindObjectOfType<LoadingSceneManager>();
        PlayerDataManager.Instance.SetGameMode(GameMode.Regular);
        loadingSceneManager.LoadScenes(GameMode.Regular);
    }

    IEnumerator LoadInfiniteMode()
    {
        yield return new WaitForSecondsRealtime(1.23f);
        LoadingSceneManager loadingSceneManager = FindObjectOfType<LoadingSceneManager>();
        PlayerDataManager.Instance.SetGameMode(GameMode.Infinite);
        loadingSceneManager.LoadScenes(GameMode.Infinite);
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
        if (lead == null)
        {
            Logger.Log("리드 오리가 아직 저장되지 않았습니다. 확인해 주세요.");
            return null;
        }
        return lead;
    }

    #region 튜토리얼
    /// <summary>
    /// 어느 탭을 밝게 해서 터치가 가능하게 할지 정함
    /// </summary>
    public void SetTutorialMode(bool tutorialMode, int indexToActivate = 0)
    {
        if (tutorialMode)
        {
            Tab_Base.sprite = Tab_BaseSprites[1];
            tabSliderHandleAnim.runtimeAnimatorController = darkTabSlideHandleCon;

            for (int i = 0; i < BtnImageRect.Length; i++)
            {
                BtnRect[i].GetComponent<Image>().color = new Color(1, 1, 1, 0);

                if (i == indexToActivate)
                {
                    BtnImageRect[i].GetComponent<Animator>().runtimeAnimatorController = defaultTabControllers[i];
                    BtnRect[i].GetComponent<Button>().interactable = true;
                    continue;
                }
                BtnImageRect[i].GetComponent<Animator>().runtimeAnimatorController = tutorialTabControllers[i];
                BtnRect[i].GetComponent<Button>().interactable = false;
            }
            bool isSRHand = indexToActivate == 4 ? false : true;
            GetComponent<TutorialMainMenuUI>().GenerateHand(BtnImageRect[indexToActivate], isSRHand);
            // ✅ bgTutorial.SetActive(true); 제거
        }
        else
        {
            Tab_Base.sprite = Tab_BaseSprites[0];
            tabSliderHandleAnim.runtimeAnimatorController = defaultTabSlideHandleCon;
            // ✅ bgTutorial.SetActive(false); 제거

            for (int i = 0; i < BtnImageRect.Length; i++)
            {
                BtnImageRect[i].GetComponent<Animator>().runtimeAnimatorController = defaultTabControllers[i];
                BtnRect[i].GetComponent<Image>().color = new Color(1, 1, 1, 1);
            }
        }
    }

    // ✅ 추가: TutorialManager 단계 → 탭 인덱스 변환
    private void InitTabsByTutorialStep()
    {
        if (TutorialManager.instance == null) return;
        OnTutorialStepChanged(TutorialManager.instance.CurrentStep);
    }

    // ✅ 추가: 단계가 바뀔 때마다 자동 호출
    private void OnTutorialStepChanged(TutorialStep step)
    {
        if (step == TutorialStep.Completed)
        {
            SetTutorialMode(false); // 기존 메서드로 전체 초기화
            return;
        }

        SetTabsByStep(step); // ← 수정된 메서드 호출
    }
    // ✅ 추가: 현재 단계에서 몇 번 탭까지 활성화할지 결정
    private int GetUnlockedTabCount(TutorialStep step)
    {
        switch (step)
        {
            case TutorialStep.Step0_OnlyBattle: return 1; // Battle만
            case TutorialStep.Step1_ShopUnlocked: return 2; // Battle + Shop
            case TutorialStep.Step2_GearUnlocked: return 3; // + Gear
            case TutorialStep.Step3_MergeUnlocked: return 4; // + Merge
            case TutorialStep.Step4_AchievementUnlocked: return 5; // 전체
            default: return 5;
        }
    }

    // ✅ 추가: 기존 SetTutorialMode를 단계 기반으로 확장
    private void SetTabsByStep(TutorialStep step)
    {
        bool[] unlocked = GetUnlockedTabs(step);

        for (int i = 0; i < BtnRect.Length; i++)
        {
            bool isUnlocked = unlocked[i];

            // ✅ 버튼 클릭 가능 여부만 제어
            BtnRect[i].GetComponent<Button>().interactable = isUnlocked;

            // ✅ 애니메이터 컨트롤러 전환 (시각적 표현은 애니메이터에 위임)
            BtnImageRect[i].GetComponent<Animator>().runtimeAnimatorController = isUnlocked
                ? defaultTabControllers[i]
                : tutorialTabControllers[i];
        }

        // 탭바 배경 및 슬라이더 핸들 처리
        bool isTutorialMode = step != TutorialStep.Completed;
        Tab_Base.sprite = Tab_BaseSprites[isTutorialMode ? 1 : 0];
        tabSliderHandleAnim.runtimeAnimatorController = isTutorialMode
            ? darkTabSlideHandleCon
            : defaultTabSlideHandleCon;
    }
    // 단계별 활성 탭 명시적으로 정의
    // 0=Shop, 1=Gear, 2=Battle, 3=Merge, 4=Achievement
    private bool[] GetUnlockedTabs(TutorialStep step)
    {
        switch (step)
        {
            case TutorialStep.Step0_OnlyBattle:
                return new bool[] { false, false, true, false, false };

            case TutorialStep.Step1_ShopUnlocked:
                return new bool[] { true, false, true, false, false };

            case TutorialStep.Step2_GearUnlocked:
                return new bool[] { true, true, true, false, false };

            case TutorialStep.Step3_MergeUnlocked:
                return new bool[] { true, true, true, true, false };

            case TutorialStep.Step4_AchievementUnlocked:
            case TutorialStep.Completed:
            default:
                return new bool[] { true, true, true, true, true };
        }
    }
    #endregion
}