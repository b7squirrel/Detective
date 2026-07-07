using UnityEngine;
using TMPro;
using DG.Tweening;

public class DisplayCurrency : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI coinText;
    [SerializeField] TextMeshProUGUI gemText;
    [SerializeField] TextMeshProUGUI lightningText;

    [Header("애니메이션 설정")]
    public Color flashColor = new Color(1f, 0.9f, 0.3f); // 노란빛
    public float countUpDuration = 0.4f;

    // 현재 화면에 표시 중인 값 (카운트업용)
    private int displayedCoin = 0;
    private int displayedGem = 0;
    private int displayedLightning = 0;

    PlayerDataManager playerDataManager;

    void OnEnable()
    {
        if (playerDataManager == null)
            playerDataManager = FindObjectOfType<PlayerDataManager>();

        playerDataManager.OnCurrencyChanged += UpdateUI;

        // 초기값 설정
        displayedCoin = playerDataManager.GetCurrentCoinNumber();
        displayedGem = playerDataManager.GetCurrentCristalNumber();
        displayedLightning = playerDataManager.GetCurrentLightningNumber();

        UpdateUI();
    }

    void OnDisable()
    {
        if (playerDataManager != null)
            playerDataManager.OnCurrencyChanged -= UpdateUI;
    }

    void Start()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        coinText.text = playerDataManager.GetCurrentCoinNumber().ToString();
        gemText.text = playerDataManager.GetCurrentCristalNumber().ToString();
        lightningText.text = playerDataManager.GetCurrentLightningNumber().ToString()
            + " / " + playerDataManager.GetMaxLightningNumber();
    }

    public void AnimateTextChange(bool isGem)
    {
        TextMeshProUGUI targetText = isGem ? gemText : coinText;
        if (targetText == null) return;

        int currentDisplay = isGem ? displayedGem : displayedCoin;
        int newValue = isGem ? playerDataManager.GetCurrentCristalNumber() : playerDataManager.GetCurrentCoinNumber();

        // 이미 같은 값이면 스킵
        if (currentDisplay == newValue) return;

        // 기존 트윈 중단
        DOTween.Kill(targetText);
        DOTween.Kill(targetText.transform);

        // displayedGem/Coin 값을 먼저 업데이트 (이게 중요!)
        if (isGem) displayedGem = newValue;
        else displayedCoin = newValue;

        // 카운트업 애니메이션
        DOTween.To(() => currentDisplay,
                   x =>
                   {
                       targetText.text = x.ToString();
                   },
                   newValue,
                   countUpDuration)
               .SetEase(Ease.OutQuad)
               .SetTarget(targetText);

        // 스케일 펀치
        targetText.transform.DOPunchScale(Vector3.one * 0.05f, 0.3f, 2, 1f)
            .OnComplete(() => targetText.transform.localScale = Vector3.one);
    }
    
    // ← 추가: 번개 전용 카운트업
    public void AnimateEnergyTextChange()
    {
        if (lightningText == null) return;

        int currentDisplay = displayedLightning;
        int newValue = playerDataManager.GetCurrentLightningNumber();

        if (currentDisplay == newValue) return;

        DOTween.Kill(lightningText);
        DOTween.Kill(lightningText.transform);

        displayedLightning = newValue;

        DOTween.To(() => currentDisplay,
                   x => { lightningText.text = x.ToString() + " / " + playerDataManager.GetMaxLightningNumber(); },
                   newValue,
                   countUpDuration)
            .SetEase(Ease.OutQuad)
            .SetTarget(lightningText);

        lightningText.transform.DOPunchScale(Vector3.one * 0.05f, 0.3f, 2, 1f)
            .OnComplete(() => lightningText.transform.localScale = Vector3.one);
    }
}