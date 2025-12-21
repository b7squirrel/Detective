using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TimeBoxButton : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] Button boxButton;
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] GameObject lockIcon;
    [SerializeField] RectTransform boxPoint;
    
    [Header("디버그")]
    [SerializeField] private bool showDebugLogs = true;
    
    private ProductData productData;
    private bool isReady = false;
    
    void Start()
    {
        // ⭐ Inspector의 OnClick 이벤트 무시하고 코드로 직접 연결
        if (boxButton != null)
        {
            // 기존 이벤트 모두 제거
            boxButton.onClick.RemoveAllListeners();
            
            // 새로 연결
            boxButton.onClick.AddListener(OnButtonClicked);
            
            Debug.Log("========================================");
            Debug.Log("[TimeBoxButton] 버튼 이벤트 연결 완료!");
            Debug.Log("========================================");
        }
        else
        {
            Debug.Log("========================================");
            Debug.LogError("[TimeBoxButton] X boxButton이 NULL!");
            Debug.Log("========================================");
        }
        
        // ⭐ 초기화 상태 로그
        Debug.Log($"[TimeBoxButton] TimeBasedBoxManager: {(TimeBasedBoxManager.Instance != null ? "OK" : "NULL")}");
        Debug.Log($"[TimeBoxButton] ShopManager: {(ShopManager.Instance != null ? "OK" : "NULL")}");
        Debug.Log($"[TimeBoxButton] ProductData: {(productData != null ? productData.ProductId : "NULL")}");
        
        UpdateUI();
    }
    
    void Update()
    {
        UpdateUI();
    }
    
    public void SetInfo(ProductData data)
    {
        productData = data;
        Debug.Log("========================================");
        Debug.Log($"[TimeBoxButton] SetInfo 호출: {(productData != null ? productData.ProductId : "NULL")}");
        Debug.Log("========================================");
    }
    
    void UpdateUI()
    {
        if (TimeBasedBoxManager.Instance == null)
        {
            return;
        }
        
        isReady = TimeBasedBoxManager.Instance.CanClaimBox();
        
        if (isReady)
        {
            if (boxButton != null) boxButton.interactable = true;
            if (timerText != null) timerText.text = "광고 보고 열기!";
            if (lockIcon != null) lockIcon.SetActive(false);
        }
        else
        {
            if (boxButton != null) boxButton.interactable = false;
            if (timerText != null)
            {
                string timeStr = TimeBasedBoxManager.Instance.GetRemainingTimeFormatted();
                timerText.text = timeStr;
            }
            if (lockIcon != null) lockIcon.SetActive(true);
        }
    }

    // ⭐ public으로 유지 (Inspector에서도 보이게)
    public void OnButtonClicked()
    {
        Debug.Log("========================================");
        Debug.Log("[TimeBoxButton] ▶▶▶ CLICK START ◀◀◀");
        Debug.Log("========================================");

        Debug.Log($"[TimeBoxButton] Step 1: isReady = {isReady}");

        if (!isReady)
        {
            Debug.Log("[TimeBoxButton] FAIL Step 1: Not ready");
            return;
        }

        Debug.Log($"[TimeBoxButton] Step 2: productData = {(productData != null ? "OK" : "NULL")}");

        if (productData == null)
        {
            Debug.LogError("[TimeBoxButton] FAIL Step 2: ProductData NULL!");
            return;
        }

        Debug.Log($"[TimeBoxButton] Step 3: ProductId = {productData.ProductId}");
        Debug.Log($"[TimeBoxButton] Step 4: ShopManager = {(ShopManager.Instance != null ? "OK" : "NULL")}");

        if (ShopManager.Instance == null)
        {
            Debug.LogError("[TimeBoxButton] FAIL Step 4: ShopManager NULL!");
            return;
        }

        Debug.Log("[TimeBoxButton] Step 5: Calling PurchaseProduct...");

        try
        {
            ShopManager.Instance.PurchaseProduct(productData.ProductId, boxPoint);
            Debug.Log("[TimeBoxButton] SUCCESS Step 5: PurchaseProduct called");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[TimeBoxButton] FAIL Step 5: Exception - {e.Message}");
        }

        Debug.Log("========================================");
        Debug.Log("[TimeBoxButton] ▶▶▶ CLICK END ◀◀◀");
        Debug.Log("========================================");
    }

    public RectTransform GetGemPoint() => boxPoint;
}