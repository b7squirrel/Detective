using UnityEngine;
using UnityEngine.UI;

public class TutorialHighlight : MonoBehaviour
{
    [Header("Overlay Panels")]
    [SerializeField] private RectTransform topPanel;
    [SerializeField] private RectTransform bottomPanel;
    [SerializeField] private RectTransform leftPanel;
    [SerializeField] private RectTransform rightPanel;
    
    [Header("Pointer")]
    [SerializeField] private RectTransform fingerPointer;
    
    [Header("Settings")]
    [SerializeField] private Vector2 pointerOffset = new Vector2(0, -50); // 손가락 위치 조정
    [SerializeField] private Canvas canvas;

    void Awake()
    {
        // Canvas가 할당되지 않았으면 자동으로 찾기
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }

        // 그래도 없으면 씬에서 찾기
        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
        }
    }

 // 특정 UI 요소를 강조하는 메서드
public void HighlightUI(RectTransform targetUI)
{
    if (targetUI == null) return;
    
    RectTransform canvasRect = canvas.GetComponent<RectTransform>();
    
    // 타겟 UI를 Canvas의 자식으로 변환하여 로컬 좌표 얻기
    Vector3[] targetCorners = new Vector3[4];
    targetUI.GetWorldCorners(targetCorners);
    
    // Canvas 좌표계로 변환
    Vector2 canvasMin = Vector2.zero;
    Vector2 canvasMax = Vector2.zero;
    
    // 왼쪽 아래 코너
    RectTransformUtility.ScreenPointToLocalPointInRectangle(
        canvasRect,
        RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, targetCorners[0]),
        canvas.worldCamera,
        out canvasMin
    );
    
    // 오른쪽 위 코너
    RectTransformUtility.ScreenPointToLocalPointInRectangle(
        canvasRect,
        RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, targetCorners[2]),
        canvas.worldCamera,
        out canvasMax
    );
    
    // Canvas 중심이 (0,0)이므로 좌표를 캔버스 크기 기준으로 정규화
    float canvasWidth = canvasRect.rect.width;
    float canvasHeight = canvasRect.rect.height;
    
    // 정규화된 좌표 (0~1 범위)
    Vector2 normalizedMin = new Vector2(
        (canvasMin.x + canvasWidth * 0.5f) / canvasWidth,
        (canvasMin.y + canvasHeight * 0.5f) / canvasHeight
    );
    
    Vector2 normalizedMax = new Vector2(
        (canvasMax.x + canvasWidth * 0.5f) / canvasWidth,
        (canvasMax.y + canvasHeight * 0.5f) / canvasHeight
    );
    
    Debug.Log($"Target Bounds - Min: {normalizedMin}, Max: {normalizedMax}");
    
    // 4개 패널 크기 조정
    SetupPanels(normalizedMin, normalizedMax);
    
    // 손가락 포인터 위치 설정
    SetupPointer(targetUI);
    
    // 오버레이 활성화
    gameObject.SetActive(true);
}

// 4개 패널의 크기와 위치 설정
private void SetupPanels(Vector2 normalizedMin, Vector2 normalizedMax)
{
    // Top Panel
    topPanel.anchorMin = new Vector2(0, normalizedMax.y);
    topPanel.anchorMax = new Vector2(1, 1);
    topPanel.offsetMin = Vector2.zero;
    topPanel.offsetMax = Vector2.zero;
    
    // Bottom Panel
    bottomPanel.anchorMin = new Vector2(0, 0);
    bottomPanel.anchorMax = new Vector2(1, normalizedMin.y);
    bottomPanel.offsetMin = Vector2.zero;
    bottomPanel.offsetMax = Vector2.zero;
    
    // Left Panel
    leftPanel.anchorMin = new Vector2(0, normalizedMin.y);
    leftPanel.anchorMax = new Vector2(normalizedMin.x, normalizedMax.y);
    leftPanel.offsetMin = Vector2.zero;
    leftPanel.offsetMax = Vector2.zero;
    
    // Right Panel
    rightPanel.anchorMin = new Vector2(normalizedMax.x, normalizedMin.y);
    rightPanel.anchorMax = new Vector2(1, normalizedMax.y);
    rightPanel.offsetMin = Vector2.zero;
    rightPanel.offsetMax = Vector2.zero;
}
    
    // 손가락 포인터 위치 설정
    private void SetupPointer(RectTransform targetUI)
    {
        fingerPointer.position = targetUI.position + (Vector3)pointerOffset;
    }
    
    // 튜토리얼 닫기
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}