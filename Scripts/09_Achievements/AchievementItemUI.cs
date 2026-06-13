using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 슬라이더, 설명, 보상 버튼. RuntimeAchievement 이벤트 구독
/// </summary>
public class AchievementItemUI : MonoBehaviour
{
    [Header("그래픽")]
    [SerializeField] TextMeshProUGUI titleText;        // 업적 제목 (추가)
    [SerializeField] TextMeshProUGUI descriptionText;  // 업적 설명
    [SerializeField] Slider progressSlider;
    [SerializeField] TextMeshProUGUI progressText;
    [SerializeField] TextMeshProUGUI rewardText;
    [SerializeField] Image rewardIcon; // 보상 아이콘 이미지
    [SerializeField] Button rewardButton;
    [SerializeField] GameObject CompletedPanel;
    [SerializeField] GameObject checkImage;
    [SerializeField] RectTransform effectStartPos; // 보상 이펙트가 나올 위치
    [SerializeField] GameObject postItYellow;
    [SerializeField] GameObject postItPink;

    [Header("보상 아이콘")]
    [SerializeField] Sprite gemIcon; // 보석 아이콘
    [SerializeField] Sprite coinIcon; // 코인 아이콘

    [Header("진행도 아이콘")]
    [SerializeField] GameObject killIcon;   // Icons/Kill
    [SerializeField] GameObject chestIcon;  // Icons/Chest (AD_DRAW)
    [SerializeField] GameObject timeIcon;   // Icons/Time (SURVIVE)

    [Header("사운드")]
    [SerializeField] AudioClip clipRewardButton;

    [HideInInspector] public RuntimeAchievement ra;
    Animator anim;

    void Awake()
    {
        LocalizationManager.OnLanguageChanged += UpdateText;
    }

    void OnDestroy()
    {
        LocalizationManager.OnLanguageChanged -= UpdateText;

        // ⭐ 추가
        if (ra != null)
        {
            ra.OnProgressChanged -= OnProgressChanged;
            ra.OnCompleted -= OnCompleted;
        }
    }

    void OnEnable()
    {
        // 애니메이션 처리만
        if (ra != null && ra.isCompleted)
        {
            if (anim == null) anim = GetComponent<Animator>();
            if (anim != null) // ← null 체크 추가
                anim.Play("AchievementItem Completed", 0, 0f);
        }
    }

    public void Bind(RuntimeAchievement runtime)
    {
        // ⭐ 기존 구독 해제 (재바인딩 시 중복 방지)
        if (ra != null)
        {
            ra.OnProgressChanged -= OnProgressChanged;
            ra.OnCompleted -= OnCompleted;
        }

        ra = runtime;

        // ⭐ 이벤트 구독
        ra.OnProgressChanged += OnProgressChanged;
        ra.OnCompleted += OnCompleted;

        UpdateText();

        progressSlider.maxValue = runtime.original.targetValue;
        rewardText.text = runtime.original.rewardNum.ToString();

        // KILL, SURVIVE 광고 뽑기 타입일 때 progressText 표시
        bool showProgress =
            runtime.original.type == AchievementType.KILL ||
            runtime.original.type == AchievementType.SURVIVE ||
            runtime.original.type == AchievementType.AD_DRAW;

        if (progressText != null)
            progressText.gameObject.SetActive(showProgress);

        // 아이콘 표시
        if (killIcon != null)
            killIcon.SetActive(runtime.original.type == AchievementType.KILL);

        if (chestIcon != null)
            chestIcon.SetActive(runtime.original.type == AchievementType.AD_DRAW);

        if (timeIcon != null)
            timeIcon.SetActive(runtime.original.type == AchievementType.SURVIVE);

        // 보상 타입에 따라 아이콘 설정
        if (rewardIcon != null)
        {
            switch (runtime.original.rewardType)
            {
                case RewardType.GEM:
                    rewardIcon.sprite = gemIcon;
                    postItPink.SetActive(true);
                    postItYellow.SetActive(false);
                    break;
                case RewardType.COIN:
                    rewardIcon.sprite = coinIcon;
                    postItPink.SetActive(false);
                    postItYellow.SetActive(true);
                    break;
            }
        }

        rewardButton.onClick.RemoveAllListeners();
        rewardButton.onClick.AddListener(() =>
        {
            SoundManager.instance.Play(clipRewardButton);
            AchievementManager.Instance.Reward(ra.original.id, effectStartPos, ra.original.rewardType);
        });

        SetCompleted(runtime.isCompleted);
        Refresh();
    }
    void OnProgressChanged(RuntimeAchievement r)
    {
        Refresh();
    }

    void OnCompleted(RuntimeAchievement r)
    {
        SetCompleted(true);
        Refresh();
    }

    // 언어 변경 시 텍스트 업데이트
    void UpdateText()
    {
        if (ra == null) return;

        // LocalizationManager 초기화 확인
        if (!LocalizationManager.IsInitialized || LocalizationManager.Achievement == null)
        {
            Logger.LogError($"Achievement Item UI 폴백");
            // 폴백: AchievementSO의 레거시 필드 사용
            if (titleText != null)
                titleText.text = ra.original.title;
            descriptionText.text = ra.original.description;
            return;
        }

        // 다국어 텍스트 적용
        if (titleText != null) titleText.text = ra.GetTitle();
        if (descriptionText != null) descriptionText.text = ra.GetDescription();
    }

    public void Refresh()
    {
        progressSlider.value = ra.progress;

        if (progressText != null)
        {
            if (ra.original.type == AchievementType.SURVIVE)
                progressText.text = $"{ra.progress}{LocalizationManager.Game.minute}";
            else
                progressText.text = $"{ra.progress}";
        }

        rewardButton.interactable = ra.isCompleted && !ra.isRewarded;
    }

    void SetCompleted(bool isActive)
    {
        if (CompletedPanel != null) CompletedPanel.SetActive(isActive);
        if (checkImage != null) checkImage.SetActive(isActive);
        rewardButton.enabled = isActive;

        if (anim == null) anim = GetComponent<Animator>();
        if (anim != null && isActive) // ← null 체크 추가
            anim.Play("AchievementItem Completed", 0, 0f);
    }

    // ✅ 추가: 튜토리얼에서 보상 버튼 하이라이트용
    public RectTransform GetRewardButtonRect()
    {
        return rewardButton.GetComponent<RectTransform>();
    }

    // 디버그 용도
    public void ForceComplete()
    {
        if (ra == null) return;

        // 런타임 업적 데이터를 강제로 완료 상태로 설정
        ra.progress = ra.original.targetValue;
        ra.isCompleted = true;

        // UI 업데이트
        SetCompleted(true);
        Refresh();
    }
}