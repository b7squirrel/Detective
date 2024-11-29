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

        // �ؽ�Ʈ ũ�� ����� ���� ���̾ƿ� ������Ʈ
        Canvas.ForceUpdateCanvases();

        // �ؽ�Ʈ�� ��ȣ ũ�� ���
        Vector2 textSize = textComponent.GetPreferredValues();

        // �е� ����
        float width = Mathf.Clamp(textSize.x + padding.x * 2, minWidth, maxWidth);
        float height = textSize.y + padding.y * 2;

        // ��� ũ�� ����
        backgroundRect.sizeDelta = new Vector2(width, height);

        // �ؽ�Ʈ ���� ũ�� ����
        textComponent.rectTransform.sizeDelta = new Vector2(width - padding.x * 2, height - padding.y * 2);
    }
}