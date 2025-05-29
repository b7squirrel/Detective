using UnityEngine;

public class SafeArea : MonoBehaviour
{
    RectTransform rectTransform;
    
    private void Awake() 
    {
        rectTransform = GetComponent<RectTransform>();
        ApplySafeArea();
    }
    
    void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;
        
        // Safe Area가 화면보다 큰 경우 (Mac Editor 오류) 전체 화면 사용
        if (safeArea.width > Screen.width || safeArea.height > Screen.height)
        {
            safeArea = new Rect(0, 0, Screen.width, Screen.height);
            Debug.LogWarning("Safe Area larger than screen, using full screen");
        }
        
        // Safe Area가 비정상적으로 작은 경우도 체크
        if (safeArea.width < Screen.width * 0.5f || safeArea.height < Screen.height * 0.5f)
        {
            safeArea = new Rect(0, 0, Screen.width, Screen.height);
            Debug.LogWarning("Safe Area too small, using full screen");
        }
        
        Vector2 minAnchor = safeArea.position;
        Vector2 maxAnchor = minAnchor + safeArea.size;
        
        minAnchor.x /= Screen.width;
        minAnchor.y /= Screen.height;
        maxAnchor.x /= Screen.width;
        maxAnchor.y /= Screen.height;
        
        rectTransform.anchorMin = minAnchor;
        rectTransform.anchorMax = maxAnchor;
    }
}