using UnityEngine;
using TMPro;

public class SpeechBubbleController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textComponent;
    [SerializeField] private RectTransform backgroundRect;
    [SerializeField] private Vector2 padding = new Vector2(20f, 10f);
    [SerializeField] private float minWidth = 100f;
    [SerializeField] private float maxWidth = 300f;

    private void Awake()
    {
        if (textComponent == null)
            textComponent = GetComponentInChildren<TextMeshProUGUI>();

        if (backgroundRect == null)
            backgroundRect = GetComponent<RectTransform>();
    }

    public void SetText(string text)
    {
        textComponent.text = text;

        // 텍스트 크기 계산을 위해 레이아웃 업데이트
        Canvas.ForceUpdateCanvases();

        // 텍스트의 선호 크기 계산
        Vector2 textSize = textComponent.GetPreferredValues();

        // 패딩 적용
        float width = Mathf.Clamp(textSize.x + padding.x * 2, minWidth, maxWidth);
        float height = textSize.y + padding.y * 2;

        // 배경 크기 조정
        backgroundRect.sizeDelta = new Vector2(width, height);

        // 텍스트 영역 크기 조정
        textComponent.rectTransform.sizeDelta = new Vector2(width - padding.x * 2, height - padding.y * 2);
    }
}