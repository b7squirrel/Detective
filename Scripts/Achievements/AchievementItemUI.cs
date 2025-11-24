using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 슬라이더, 설명, 보상 버튼. RuntimeAchievement 이벤트 구독
/// </summary>
public class AchievementItemUI : MonoBehaviour
{
    [Header("그래픽")]
    [SerializeField] TextMeshProUGUI description;
    [SerializeField] Slider progressSlider;
    [SerializeField] TextMeshProUGUI progressText;
    [SerializeField] TextMeshProUGUI rewardText;
    [SerializeField] Image rewardIcon; // 보상 아이콘 이미지
    [SerializeField] Button rewardButton;
    [SerializeField] GameObject CompletedPanel;
    [SerializeField] GameObject checkImage;
    [SerializeField] RectTransform effectStartPos; // 보상 이펙트가 나올 위치

    [Header("보상 아이콘")]
    [SerializeField] Sprite gemIcon; // 보석 아이콘
    [SerializeField] Sprite coinIcon; // 코인 아이콘

    [Header("사운드")]
    [SerializeField] AudioClip clipRewardButton;

    [HideInInspector] public RuntimeAchievement ra;
    Animator anim;

    public void Bind(RuntimeAchievement runtime)
    {
        ra = runtime;

        description.text = runtime.original.description;

        progressSlider.maxValue = runtime.original.targetValue;

        rewardText.text = runtime.original.rewardNum.ToString();

        // 보상 타입에 따라 아이콘 설정
        if (rewardIcon != null)
        {
            switch (runtime.original.rewardType)
            {
                case RewardType.GEM:
                    rewardIcon.sprite = gemIcon;
                    break;
                case RewardType.COIN:
                    rewardIcon.sprite = coinIcon;
                    break;
            }
        }

        rewardButton.onClick.AddListener(() =>
        {
            SoundManager.instance.Play(clipRewardButton);
            AchievementManager.Instance.Reward(ra.original.id, effectStartPos, ra.original.rewardType);
        });

        SetCompleted(runtime.isCompleted);

        Refresh();
    }

    // 다른 탭에 갔다오면 (비활성화가 되었다가 다시 활성화가 되면) 다시 디폴트 애니메이션을 재생하는 것을 방지
    void OnEnable()
    {
        if (ra != null && ra.isCompleted)
        {
            if (anim == null) anim = GetComponent<Animator>();
            anim.Play("AchievementItem Completed", 0, 0f);
        }
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