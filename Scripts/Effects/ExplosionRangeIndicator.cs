using UnityEngine;

public class ExplosionRangeIndicator : MonoBehaviour
{
    [Header("외형 설정")]
    [SerializeField] private Color fillColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private Color outlineColor = new Color(1f, 0f, 0f, 0.8f);
    [SerializeField] private float outlineWidth = 0.05f;
    [SerializeField] private int dashCount = 30;
    [SerializeField] private float dashLength = 0.7f;
    
    [Header("애니메이션")]
    [SerializeField] private bool useScaleAnimation = true;
    [SerializeField] private float scaleAnimationDuration = 0.3f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("페이드 효과")]
    [SerializeField] private bool useFadeAnimation = true;
    [SerializeField] private float fadeSpeed = 2f;
    
    private SpriteRenderer fillRenderer;
    private LineRenderer lineRenderer;
    private float radius = 1f;
    private float animationTimer = 0f;
    private bool isAnimating = false;
    private bool isInitialized = false;

    void Initialize()
    {
        if (isInitialized) return;

        CreateIndicatorComponents();
        isInitialized = true;
    }

    void CreateIndicatorComponents()
    {
        // 1. 내부 채우기용 원 생성
        GameObject fillObject = new GameObject("Fill");
        fillObject.transform.SetParent(transform);
        fillObject.transform.localPosition = Vector3.zero;
        
        fillRenderer = fillObject.AddComponent<SpriteRenderer>();
        fillRenderer.sprite = CreateCircleSprite();
        fillRenderer.color = fillColor;
        fillRenderer.sortingOrder = -1;

        // 2. 점선 외곽선용 LineRenderer 생성
        GameObject outlineObject = new GameObject("Outline");
        outlineObject.transform.SetParent(transform);
        outlineObject.transform.localPosition = Vector3.zero;
        
        lineRenderer = outlineObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = outlineColor;
        lineRenderer.endColor = outlineColor;
        lineRenderer.startWidth = outlineWidth;
        lineRenderer.endWidth = outlineWidth;
        lineRenderer.useWorldSpace = false;
        lineRenderer.sortingOrder = 0;
        lineRenderer.numCapVertices = 5;

        // 활성화
        fillObject.SetActive(true);
        outlineObject.SetActive(true);
    }

    Sprite CreateCircleSprite()
    {
        Texture2D texture = new Texture2D(256, 256);
        Color[] colors = new Color[256 * 256];
        
        Vector2 center = new Vector2(128, 128);
        float radius = 128f;
        
        for (int y = 0; y < 256; y++)
        {
            for (int x = 0; x < 256; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                colors[y * 256 + x] = distance <= radius ? Color.white : Color.clear;
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f), 100f);
    }

    void DrawDashedCircle()
    {
        int totalPoints = dashCount * 2;
        lineRenderer.positionCount = totalPoints + 1;
        
        float angleStep = 360f / dashCount;
        int pointIndex = 0;
        
        for (int i = 0; i < dashCount; i++)
        {
            float startAngle = i * angleStep;
            float endAngle = startAngle + (angleStep * dashLength);
            
            float startRad = startAngle * Mathf.Deg2Rad;
            Vector3 startPos = new Vector3(
                Mathf.Cos(startRad) * radius,
                Mathf.Sin(startRad) * radius,
                0
            );
            lineRenderer.SetPosition(pointIndex++, startPos);
            
            float endRad = endAngle * Mathf.Deg2Rad;
            Vector3 endPos = new Vector3(
                Mathf.Cos(endRad) * radius,
                Mathf.Sin(endRad) * radius,
                0
            );
            lineRenderer.SetPosition(pointIndex++, endPos);
        }
        
        lineRenderer.SetPosition(pointIndex, lineRenderer.GetPosition(0));
    }

    void Update()
    {
        if (!isInitialized) return;

        if (isAnimating)
        {
            animationTimer += Time.deltaTime;
            
            if (useScaleAnimation && animationTimer < scaleAnimationDuration)
            {
                float t = animationTimer / scaleAnimationDuration;
                float scale = scaleCurve.Evaluate(t);
                transform.localScale = Vector3.one * scale;
            }
            else
            {
                isAnimating = false;
                transform.localScale = Vector3.one;
            }
        }
        
        if (useFadeAnimation && fillRenderer != null && fillRenderer.gameObject.activeSelf)
        {
            float alpha = (Mathf.Sin(Time.time * fadeSpeed) + 1f) * 0.5f;
            alpha = Mathf.Lerp(0.2f, 0.5f, alpha);
            
            Color newFillColor = fillColor;
            newFillColor.a = alpha;
            fillRenderer.color = newFillColor;
        }
    }

    public void Show(Vector3 position, float explosionRadius)
    {
        Initialize();

        radius = explosionRadius;
        transform.position = position;

        if (useScaleAnimation)
        {
            transform.localScale = Vector3.zero;
            animationTimer = 0f;
            isAnimating = true;
        }
        else
        {
            transform.localScale = Vector3.one;
        }

        fillRenderer.transform.localScale = Vector3.one * radius * 2f;
        DrawDashedCircle();

        fillRenderer.gameObject.SetActive(true);
        lineRenderer.gameObject.SetActive(true);

        fillRenderer.transform.position = transform.position;
        lineRenderer.transform.position = transform.position;
    }

    public void Hide()
    {
        if (fillRenderer != null) fillRenderer.gameObject.SetActive(false);
        if (lineRenderer != null) lineRenderer.gameObject.SetActive(false);
    }
}