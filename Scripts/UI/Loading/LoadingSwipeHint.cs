using UnityEngine;
using TMPro;

public class LoadingSwipeHint : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private string fallbackText = "로딩 중...";
    
    private void Start()
    {
        UpdateHint();
        
        // 언어 변경 이벤트 구독
        LocalizationManager.OnLanguageChanged += UpdateHint;
    }
    
    private void OnDestroy()
    {
        // 이벤트 구독 해제
        LocalizationManager.OnLanguageChanged -= UpdateHint;
    }
    
    private void UpdateHint()
    {
        // LocalizationManager 초기화 확인
        if (LocalizationManager.IsInitialized && LocalizationManager.Game != null)
        {
            hintText.text = LocalizationManager.Game.GetRandomHint();
        }
        else
        {
            // 폴백 텍스트 사용
            hintText.text = fallbackText;
        }
    }
}