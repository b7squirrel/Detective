using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 광고 시청으로 번개 획득하는 버튼 (energy_ad 전용)
/// </summary>
public class EnergyAdButton : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] Image productImage;
    [SerializeField] Sprite productSprite;
    [SerializeField] TextMeshProUGUI rewardText;
    [SerializeField] Button button;
    [SerializeField] GameObject availableIcon;

    [Header("포스트잇 상태")]
    [SerializeField] GameObject postItGrey;
    [SerializeField] GameObject postItPink;

    [Header("상태 아이콘 / 텍스트")]
    [SerializeField] GameObject adIcon;
    [SerializeField] GameObject remainingTimeObject;
    [SerializeField] TextMeshProUGUI remainingTimeText;

    [Header("FX 위치")]
    [SerializeField] RectTransform energyPoint;

    private ProductData productData;
    Animator anim;

    // ⭐ 실시간 갱신용 캐시
    private bool lastCanClaim = true; // 초기값은 SetInfo에서 바로 갱신되므로 큰 의미 없음
    private int lastDisplayedSeconds = -1;
    private bool isInitialized = false;

    void OnEnable()
    {
        LocalizationManager.OnLanguageChanged += OnLanguageChanged;
    }

    void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
    }

    void Update()
    {
        if (!isInitialized) return;
        RefreshStatus();
    }

    public void SetInfo(ProductData data)
    {
        productData = data;

        if (productImage != null && productSprite != null)
            productImage.sprite = productSprite;

        if (rewardText != null)
            rewardText.text = $"x{productData.RewardEnergy}";

        isInitialized = true;
        lastDisplayedSeconds = -1; // 강제로 첫 갱신 발생시키기
        RefreshStatus();
    }

    void OnLanguageChanged()
    {
        lastDisplayedSeconds = -1; // 언어 바뀌면 포맷도 다시 적용되도록 강제 갱신
        RefreshStatus();
    }

    void RefreshStatus()
    {
        if (EnergyAdRewardManager.Instance == null) return;

        float cooldown = EnergyAdRewardManager.Instance.GetRemainingCooldownSeconds();
        bool canClaim = cooldown <= 0f;
        int totalSeconds = Mathf.CeilToInt(cooldown);

        // ⭐ 상태(잠김↔가능) 전환은 바뀔 때만 처리
        if (canClaim != lastCanClaim || lastDisplayedSeconds < 0)
        {
            if (button != null) button.interactable = canClaim;
            if (postItPink != null) postItPink.SetActive(canClaim);
            if (postItGrey != null) postItGrey.SetActive(!canClaim);
            if (adIcon != null) adIcon.SetActive(canClaim);
            if (availableIcon != null) availableIcon.SetActive(canClaim);
            if (remainingTimeObject != null) remainingTimeObject.SetActive(!canClaim);

            lastCanClaim = canClaim;
        }

        // ⭐ 텍스트는 초 단위 값이 실제로 바뀔 때만 갱신 (매 프레임 문자열 생성 방지)
        if (!canClaim && totalSeconds != lastDisplayedSeconds && remainingTimeText != null)
        {
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            remainingTimeText.text = string.Format(
                LocalizationManager.Game.energyCooldownFormat, minutes, seconds);

            lastDisplayedSeconds = totalSeconds;
        }
    }

    public void OnAdButtonClicked()
    {
        if (productData == null)
        {
            Logger.LogError("[EnergyAdButton] ProductData가 설정되지 않았습니다.");
            return;
        }

        ShopManager shopManager = FindObjectOfType<ShopManager>();
        if (shopManager == null)
        {
            Logger.LogError("[EnergyAdButton] ShopManager를 찾을 수 없습니다.");
            return;
        }

        if (anim == null) anim = GetComponent<Animator>();
        if (anim != null) anim.SetTrigger("Pressed");

        shopManager.PurchaseProduct(productData.ProductId, energyPoint);

        // ⭐ 광고 시청 후 쿨다운이 즉시 시작되니, 다음 프레임부터 바로 카운트다운이 보이도록 강제 갱신
        lastDisplayedSeconds = -1;
    }
}