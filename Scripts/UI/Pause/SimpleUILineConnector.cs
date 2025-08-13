using UnityEngine;
using UnityEngine.UI;

public class SimpleUILineConnector : MonoBehaviour
{
    [Header("연결할 이미지들")]
    public Image startImage;
    public Image endImage;
    
    [Header("선 설정")]
    public Color lineColor = Color.white;
    public float lineThickness = 2f;
    
    [Header("연결점 설정")]
    public bool connectFromCenter = true;
    
    [Header("성능 최적화")]
    public bool updateEveryFrame = true;

    private Image lineImage;
    private RectTransform lineRect;
    private RectTransform canvasRect;
    private Vector2 lastStartPos, lastEndPos;

    void Start()
    {
        CreateLineImage();
        UpdateLine();
    }

    void Update()
    {
        if (updateEveryFrame)
        {
            UpdateLine();
        }
    }

    void CreateLineImage()
    {
        // Canvas RectTransform 찾기
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            canvas = FindObjectOfType<Canvas>();
        
        canvasRect = canvas.GetComponent<RectTransform>();

        // 선 이미지 오브젝트 생성
        GameObject lineObject = new GameObject("UILine");
        lineObject.transform.SetParent(transform, false);
        
        // Image 컴포넌트 추가
        lineImage = lineObject.AddComponent<Image>();
        lineImage.color = lineColor;
        lineImage.raycastTarget = false; // UI 이벤트 차단하지 않음
        
        // RectTransform 설정
        lineRect = lineObject.GetComponent<RectTransform>();
        lineRect.anchorMin = new Vector2(0.5f, 0.5f);
        lineRect.anchorMax = new Vector2(0.5f, 0.5f);
        lineRect.pivot = new Vector2(0, 0.5f); // 왼쪽 중앙을 피벗으로 설정
    }

    void UpdateLine()
    {
        if (startImage == null || endImage == null || lineImage == null) return;

        Vector2 startPos = GetUIPosition(startImage);
        Vector2 endPos = GetUIPosition(endImage);

        // 위치가 변경되지 않았으면 업데이트하지 않음 (성능 최적화)
        if (!updateEveryFrame && 
            Vector2.Distance(startPos, lastStartPos) < 0.1f && 
            Vector2.Distance(endPos, lastEndPos) < 0.1f)
        {
            return;
        }

        lastStartPos = startPos;
        lastEndPos = endPos;

        // 선의 길이와 각도 계산
        Vector2 direction = endPos - startPos;
        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // RectTransform 업데이트
        lineRect.anchoredPosition = startPos;
        lineRect.sizeDelta = new Vector2(distance, lineThickness);
        lineRect.rotation = Quaternion.Euler(0, 0, angle);
    }

    Vector2 GetUIPosition(Image image)
    {
        if (image == null) return Vector2.zero;

        RectTransform rectTransform = image.rectTransform;
        Vector2 position;

        // 스크린 좌표를 Canvas 로컬 좌표로 변환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, 
            RectTransformUtility.WorldToScreenPoint(null, rectTransform.position), 
            null, 
            out position);

        if (!connectFromCenter)
        {
            // 가장자리에서 연결하는 경우
            Image otherImage = (image == startImage) ? endImage : startImage;
            if (otherImage != null)
            {
                Vector2 otherPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect, 
                    RectTransformUtility.WorldToScreenPoint(null, otherImage.rectTransform.position), 
                    null, 
                    out otherPos);

                Vector2 direction = (otherPos - position).normalized;
                Vector2 imageSize = rectTransform.rect.size;
                
                Vector2 edgeOffset = GetRectEdgePoint(direction, imageSize);
                position += edgeOffset;
            }
        }

        return position;
    }

    Vector2 GetRectEdgePoint(Vector2 direction, Vector2 rectSize)
    {
        float halfWidth = rectSize.x * 0.5f;
        float halfHeight = rectSize.y * 0.5f;
        
        // 사각형과 방향 벡터의 교점 계산
        if (Mathf.Abs(direction.x) * halfHeight > Mathf.Abs(direction.y) * halfWidth)
        {
            // 좌우 모서리와 교차
            float t = halfWidth / Mathf.Abs(direction.x);
            return new Vector2(halfWidth * Mathf.Sign(direction.x), direction.y * t);
        }
        else
        {
            // 상하 모서리와 교차
            float t = halfHeight / Mathf.Abs(direction.y);
            return new Vector2(direction.x * t, halfHeight * Mathf.Sign(direction.y));
        }
    }

    // Public Methods for Runtime Control

    /// <summary>
    /// 연결할 이미지들을 설정합니다.
    /// </summary>
    public void SetImages(Image start, Image end)
    {
        startImage = start;
        endImage = end;
        UpdateLine();
    }

    /// <summary>
    /// 선의 스타일을 변경합니다.
    /// </summary>
    public void SetLineStyle(Color color, float thickness)
    {
        lineColor = color;
        lineThickness = thickness;
        
        if (lineImage != null)
        {
            lineImage.color = color;
            UpdateLine(); // thickness 적용을 위해 업데이트
        }
    }

    /// <summary>
    /// 선의 표시 여부를 설정합니다.
    /// </summary>
    public void SetLineVisible(bool visible)
    {
        if (lineImage != null)
        {
            lineImage.gameObject.SetActive(visible);
        }
    }

    /// <summary>
    /// 선의 투명도를 설정합니다.
    /// </summary>
    public void SetLineAlpha(float alpha)
    {
        if (lineImage != null)
        {
            Color color = lineImage.color;
            color.a = alpha;
            lineImage.color = color;
        }
    }

    /// <summary>
    /// 연결점 설정을 변경합니다.
    /// </summary>
    public void SetConnectionMode(bool fromCenter)
    {
        connectFromCenter = fromCenter;
        UpdateLine();
    }

    /// <summary>
    /// 수동으로 선을 업데이트합니다.
    /// </summary>
    public void ForceUpdateLine()
    {
        UpdateLine();
    }

    // 애니메이션 효과들

    /// <summary>
    /// 선이 점진적으로 나타나는 애니메이션
    /// </summary>
    public System.Collections.IEnumerator FadeInLine(float duration)
    {
        if (lineImage == null) yield break;

        float elapsed = 0f;
        Color startColor = lineImage.color;
        startColor.a = 0f;
        lineImage.color = startColor;

        Color targetColor = startColor;
        targetColor.a = lineColor.a;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            Color currentColor = Color.Lerp(startColor, targetColor, progress);
            lineImage.color = currentColor;
            
            yield return null;
        }

        lineImage.color = targetColor;
    }

    /// <summary>
    /// 선이 한 쪽에서 다른 쪽으로 그려지는 애니메이션
    /// </summary>
    public System.Collections.IEnumerator DrawLine(float duration)
    {
        if (lineImage == null) yield break;

        UpdateLine(); // 최종 상태 계산
        
        Vector2 finalSize = lineRect.sizeDelta;
        Vector2 startSize = new Vector2(0, finalSize.y);
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            Vector2 currentSize = Vector2.Lerp(startSize, finalSize, progress);
            lineRect.sizeDelta = currentSize;
            
            yield return null;
        }

        lineRect.sizeDelta = finalSize;
    }

    void OnDestroy()
    {
        if (lineImage != null)
        {
            DestroyImmediate(lineImage.gameObject);
        }
    }

    // 에디터에서 디버깅용 기즈모
    void OnDrawGizmosSelected()
    {
        if (startImage != null && endImage != null)
        {
            Gizmos.color = lineColor;
            Vector3 start = startImage.transform.position;
            Vector3 end = endImage.transform.position;
            Gizmos.DrawLine(start, end);
        }
    }
}