using UnityEngine;
using UnityEngine.UI;

public class SimpleUILineConnector : MonoBehaviour
{
    public Vector2 startPos;
    public Vector2 endPos;
    public Color lineColor = Color.white;
    public float lineThickness = 2f;
    public bool useLocalCoordinates = true;
    public bool updateEveryFrame = false;
    public float updateInterval = 0.1f;

    private Image lineImage;
    private RectTransform lineRect;
    private RectTransform canvasRect;
    private Vector2 lastStartPos, lastEndPos;
    private float lastUpdateTime;

    void Start()
    {
        CreateLineImage();
        UpdateLine();
    }

    void Update()
    {
        if (updateEveryFrame)
            UpdateLine();
        else if (Time.time - lastUpdateTime >= updateInterval)
        {
            if (Vector2.Distance(startPos, lastStartPos) > 0.1f ||
                Vector2.Distance(endPos, lastEndPos) > 0.1f)
                UpdateLine();

            lastUpdateTime = Time.time;
        }
    }

    void CreateLineImage()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindObjectOfType<Canvas>();
        canvasRect = canvas.GetComponent<RectTransform>();

        GameObject lineObject = new GameObject("UIPointLine");
        lineObject.transform.SetParent(canvasRect, false); // 🔹 Canvas 직속

        lineImage = lineObject.AddComponent<Image>();
        lineImage.color = lineColor;
        lineImage.raycastTarget = false;

        lineRect = lineObject.GetComponent<RectTransform>();
        lineRect.anchorMin = new Vector2(0.5f, 0.5f);
        lineRect.anchorMax = new Vector2(0.5f, 0.5f);
        lineRect.pivot = new Vector2(0, 0.5f);
    }

    void UpdateLine()
    {
        if (lineImage == null) return;

        Vector2 localStart = useLocalCoordinates ? startPos : WorldToCanvas(startPos);
        Vector2 localEnd = useLocalCoordinates ? endPos : WorldToCanvas(endPos);

        lastStartPos = startPos;
        lastEndPos = endPos;

        Vector2 dir = localEnd - localStart;
        float dist = dir.magnitude;
        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        lineRect.anchoredPosition = localStart;
        lineRect.sizeDelta = new Vector2(dist, lineThickness);
        lineRect.rotation = Quaternion.Euler(0, 0, ang);

        if (lineImage.color != lineColor)
            lineImage.color = lineColor;
    }

    Vector2 WorldToCanvas(Vector2 worldPos)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(null, worldPos),
            null,
            out localPoint
        );
        return localPoint;
    }

    public void SetCoordinateMode(bool useLocal) => useLocalCoordinates = useLocal;

    
}
