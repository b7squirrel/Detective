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
    }

    void OnEnable()
    {
        // 애니메이션 처리만
        if (ra != null && ra.isCompleted)
        {
            if (anim == null) anim = GetComponent<Animator>();
            anim.Play("AchievementItem Completed", 0, 0f);
        }
    }

    public void Bind(RuntimeAchievement runtime)
    {
        ra = runtime;

        // 다국어 적용
        UpdateText();

        progressSlider.maxValue = runtime.original.targetValue;

        rewardText.text = runtime.original.rewardNum.ToString();

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

        rewardButton.onClick.RemoveAllListeners(); // 중복 방지
        rewardButton.onClick.AddListener(() =>
        {
            SoundManager.instance.Play(clipRewardButton);
            AchievementManager.Instance.Reward(ra.original.id, effectStartPos, ra.original.rewardType);
        });

        SetCompleted(runtime.isCompleted);

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
        if (titleText != null)
            titleText.text = ra.GetTitle();
        descriptionText.text = ra.GetDescription();
    }

    public void Refresh()
    {
        progressSlider.value = ra.progress;
        progressText.text = $"{ra.progress} / {ra.original.targetValue}";

        rewardButton.interactable = ra.isCompleted && !ra.isRewarded;
    }

    void SetCompleted(bool isActive)
    {
        CompletedPanel.SetActive(isActive);
        checkImage.SetActive(isActive);
        rewardButton.enabled = isActive;
        progressSlider.gameObject.SetActive(!isActive); // 완료 시 슬라이더 숨김

        if (anim == null) anim = GetComponent<Animator>();
        if (isActive)
        {
            anim.Play("AchievementItem Completed", 0, 0f);
        }
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