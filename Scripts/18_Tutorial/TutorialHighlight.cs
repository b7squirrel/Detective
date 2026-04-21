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

        // ✅ 부모가 비활성 상태에서 먼저 위치 계산
        // (비활성 오브젝트도 RectTransform 값 수정 가능)
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector3[] targetCorners = new Vector3[4];
        targetUI.GetWorldCorners(targetCorners);

        Vector2 canvasMin = Vector2.zero;
        Vector2 canvasMax = Vector2.zero;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, targetCorners[0]),
            canvas.worldCamera,
            out canvasMin
        );

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, targetCorners[2]),
            canvas.worldCamera,
            out canvasMax
        );

        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;

        Vector2 normalizedMin = new Vector2(
            (canvasMin.x + canvasWidth * 0.5f) / canvasWidth,
            (canvasMin.y + canvasHeight * 0.5f) / canvasHeight
        );

        Vector2 normalizedMax = new Vector2(
            (canvasMax.x + canvasWidth * 0.5f) / canvasWidth,
            (canvasMax.y + canvasHeight * 0.5f) / canvasHeight
        );

        // ✅ 위치 설정 (아직 비활성 상태)
        SetupPanels(normalizedMin, normalizedMax);
        SetupPointer(targetUI);

        // ✅ 위치가 완전히 설정된 후 한 번에 모두 활성화
        topPanel.gameObject.SetActive(true);
        bottomPanel.gameObject.SetActive(true);
        leftPanel.gameObject.SetActive(true);
        rightPanel.gameObject.SetActive(true);
        fingerPointer.gameObject.SetActive(true);
        gameObject.SetActive(true); // ← 부모는 마지막에 활성화
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

    // 손가락 포인터 위치 및 방향 설정
    private void SetupPointer(RectTransform targetUI)
    {
        fingerPointer.position = targetUI.position;

        // ✅ 추가: 타겟이 화면 왼쪽 절반에 있으면 손가락 방향 반전
        // 캔버스 중앙 기준으로 타겟의 X 위치 판단
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 canvasCenter = canvasRect.position;

        bool isOnLeftSide = targetUI.position.x < canvasCenter.x;

        fingerPointer.localScale = new Vector3(
            isOnLeftSide ? -1f : 1f,
            1f,
            1f
        );
    }

    // 튜토리얼 닫기
    public void Hide()
    {
        // ✅ Hide 시에도 자식들 같이 숨기기
        topPanel.gameObject.SetActive(false);
        bottomPanel.gameObject.SetActive(false);
        leftPanel.gameObject.SetActive(false);
        rightPanel.gameObject.SetActive(false);
        fingerPointer.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }
}