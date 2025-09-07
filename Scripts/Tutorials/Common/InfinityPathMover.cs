using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIInfinityPathMover : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 1f;
    [SerializeField] private float pathSize = 100f; // UI는 픽셀 단위
    [SerializeField] private bool autoMove = true;
    [SerializeField] private bool useUnscaledTime = false; // timeScale 영향 무시 여부
    
    [Header("Trail Settings")]
    [SerializeField] private GameObject trailPrefab; // UI Image 프리팹
    [SerializeField] private int maxTrailLength = 30;
    [SerializeField] private float trailSpacing = 0.1f;
    [SerializeField] private Transform trailParent; // 트레일 부모 객체 (Canvas 하위)
    // 현재 사용할 델타타임과 타임 값
    private float CurrentDeltaTime => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
    private float CurrentTime => useUnscaledTime ? Time.unscaledTime : Time.time;
    
    [Header("Guide Path (UI Image)")]
    [SerializeField] private bool showGuidePath = true;
    [SerializeField] private Image guidePathImage; // 가이드 경로용 Image
    [SerializeField] private Color guidePathColor = Color.white;
    
    private RectTransform rectTransform;
    private float time = 0f;
    private float lastTrailTime = 0f;
    private Queue<GameObject> trailPoints = new Queue<GameObject>();
    
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("RectTransform이 필요합니다! UI 객체에 이 스크립트를 붙여주세요.");
            return;
        }
        
        if (trailParent == null)
        {
            trailParent = transform.parent; // 기본적으로 부모를 트레일 부모로 설정
        }
        
        SetupGuidePath();
    }
    
    void Update()
    {
        if (autoMove && rectTransform != null)
        {
            MoveAlongPath();
            UpdateTrail();
        }
    }
    
    /// <summary>
    /// UI 좌표계에서 무한대 위치 계산
    /// </summary>
    Vector2 CalculateInfinityPosition(float t)
    {
        float sinT = Mathf.Sin(t);
        float cosT = Mathf.Cos(t);
        float denominator = 1f + sinT * sinT;
        
        float x = (pathSize * cosT) / denominator;
        float y = (pathSize * sinT * cosT) / denominator;
        
        return new Vector2(x, y);
    }
    
    /// <summary>
    /// UI 객체를 무한대 경로를 따라 이동
    /// </summary>
    void MoveAlongPath()
    {
        Vector2 newPosition = CalculateInfinityPosition(time);
        rectTransform.anchoredPosition = newPosition;
        
        time += speed * Time.unscaledDeltaTime;
        
        if (time >= 2f * Mathf.PI)
        {
            time = 0f;
        }
    }

    /// <summary>
    /// UI 트레일 효과 업데이트
    /// </summary>
    void UpdateTrail()
    {
        if (trailPrefab == null || trailParent == null) return;

        if (CurrentTime - lastTrailTime > trailSpacing)
        {
            CreateUITrailPoint();
            lastTrailTime = CurrentTime;
        }

        UpdateTrailVisuals();
    }

    /// <summary>
    /// UI 트레일 점 생성
    /// </summary>
    void CreateUITrailPoint()
    {
        GameObject trailPoint = Instantiate(trailPrefab, trailParent);
        RectTransform trailRect = trailPoint.GetComponent<RectTransform>();
        
        if (trailRect != null)
        {
            trailRect.anchoredPosition = rectTransform.anchoredPosition;
        }
        
        trailPoints.Enqueue(trailPoint);
        
        // 최대 길이 초과시 제거
        if (trailPoints.Count > maxTrailLength)
        {
            GameObject oldestPoint = trailPoints.Dequeue();
            if (oldestPoint != null)
            {
                Destroy(oldestPoint);
            }
        }
    }
    
    /// <summary>
    /// UI 트레일 시각 효과 업데이트
    /// </summary>
    void UpdateTrailVisuals()
    {
        GameObject[] trailArray = trailPoints.ToArray();
        
        for (int i = 0; i < trailArray.Length; i++)
        {
            if (trailArray[i] == null) continue;
            
            float alpha = (float)(i + 1) / trailArray.Length;
            float scale = alpha * 0.8f + 0.2f;
            
            // UI Image 컴포넌트 색상 조정
            Image img = trailArray[i].GetComponent<Image>();
            if (img != null)
            {
                Color color = img.color;
                color.a = alpha * 0.7f; // 약간의 투명도
                img.color = color;
            }
            
            // 크기 조정
            trailArray[i].transform.localScale = Vector3.one * scale;
        }
    }
    
    /// <summary>
    /// UI용 가이드 경로 설정 (단순 이미지 회전)
    /// </summary>
    void SetupGuidePath()
    {
        if (!showGuidePath || guidePathImage == null) return;
        
        guidePathImage.color = guidePathColor;
        
        // 무한대 모양 이미지가 없다면 단순히 색상만 설정
        // 실제로는 무한대 모양의 스프라이트를 사용하는 것이 좋습니다
    }
    
    /// <summary>
    /// 애니메이션 리셋
    /// </summary>
    public void ResetAnimation()
    {
        time = 0f;
        ClearTrail();
    }
    
    /// <summary>
    /// 트레일 클리어
    /// </summary>
    public void ClearTrail()
    {
        while (trailPoints.Count > 0)
        {
            GameObject trailPoint = trailPoints.Dequeue();
            if (trailPoint != null)
            {
                Destroy(trailPoint);
            }
        }
    }
    
    /// <summary>
    /// 공개 함수들 (UI 버튼에서 호출 가능)
    /// </summary>
    public void TogglePause() => autoMove = !autoMove;
    public void SetSpeed(float newSpeed) => speed = newSpeed;
    public void SetPathSize(float newSize) => pathSize = newSize;
}