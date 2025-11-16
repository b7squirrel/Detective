using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 슬라이더, 설명, 보상 버튼. RuntimeAchievement 이벤트 구독
/// </summary>
public class AchievementItemUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI description;
    [SerializeField] Slider progressSlider;
    [SerializeField] TextMeshProUGUI progressText;
    [SerializeField] TextMeshProUGUI rewardText;
    [SerializeField] Button rewardButton;
    [SerializeField] GameObject CompletedPanel;
    [SerializeField] GameObject checkImage;

    [HideInInspector] public RuntimeAchievement ra;
    Animator anim;

    public void Bind(RuntimeAchievement runtime)
    {
        ra = runtime;

        description.text = runtime.original.description;

        progressSlider.maxValue = runtime.original.targetValue;

        rewardText.text = runtime.original.rewardGem.ToString();

        rewardButton.onClick.AddListener(() =>
        {
            AchievementManager.Instance.Reward(ra.original.id, GetComponent<RectTransform>());
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
}