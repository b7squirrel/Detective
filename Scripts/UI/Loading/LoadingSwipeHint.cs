using UnityEngine;

public class LoadingSwipeHint : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI hintText;

    private void OnEnable()
    {
        // LocalizationManager가 초기화될 때까지 대기
        if (LocalizationManager.IsInitialized && LocalizationManager.Game != null)
        {
            hintText.text = LocalizationManager.Game.GetRandomHint();
        }
        else
        {
            // 초기화되지 않은 경우 기본 텍스트 표시
            hintText.text = "로딩 중...";
            // 또는 코루틴으로 초기화 대기
            StartCoroutine(WaitForLocalization());
        }
    }
    
    private System.Collections.IEnumerator WaitForLocalization()
    {
        // LocalizationManager가 초기화될 때까지 대기
        while (!LocalizationManager.IsInitialized || LocalizationManager.Game == null)
        {
            yield return null;
        }
        
        hintText.text = LocalizationManager.Game.GetRandomHint();
    }
}