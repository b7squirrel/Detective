using UnityEngine;
using TMPro;

/// <summary>
/// 메인 텍스트와 그림자(외곽선용) 텍스트를 하나로 묶어서 다루는 래퍼 컴포넌트입니다.
/// 위치(RectTransform)와 색상(color 계열)을 제외한 모든 속성은
/// mainText -> shadowText 로 자동 동기화됩니다.
///
/// 사용법:
/// 1. 프리팹에서 메인 Text, Shadow Text 오브젝트를 각각 만든 뒤
///    이 컴포넌트를 (둘 중 아무 오브젝트에나, 보통 부모나 메인 텍스트 오브젝트에) 붙입니다.
/// 2. 인스펙터에서 mainText / shadowText 를 각각 드래그해서 연결합니다.
/// 3. 기존에 TextMeshProUGUI 변수를 쓰던 곳에서, 이 컴포넌트의 프로퍼티를 대신 사용합니다.
///    예) grade.text = "전설";  (내부적으로 mainText.text, shadowText.text 둘 다 반영됨)
/// </summary>
[DisallowMultipleComponent]
public class ShadowedText : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI mainText;
    [SerializeField] private TextMeshProUGUI shadowText;

    // ───────────────────────────────
    // 자주 쓰는 속성 프로퍼티
    // ───────────────────────────────

    public string text
    {
        get => mainText.text;
        set
        {
            mainText.text = value;
            if (shadowText != null) shadowText.text = value;
        }
    }

    public float fontSize
    {
        get => mainText.fontSize;
        set
        {
            mainText.fontSize = value;
            if (shadowText != null) shadowText.fontSize = value;
        }
    }

    public FontStyles fontStyle
    {
        get => mainText.fontStyle;
        set
        {
            mainText.fontStyle = value;
            if (shadowText != null) shadowText.fontStyle = value;
        }
    }

    public TMP_FontAsset font
    {
        get => mainText.font;
        set
        {
            mainText.font = value;
            if (shadowText != null) shadowText.font = value;
        }
    }

    public TextAlignmentOptions alignment
    {
        get => mainText.alignment;
        set
        {
            mainText.alignment = value;
            if (shadowText != null) shadowText.alignment = value;
        }
    }

    public bool enableWordWrapping
    {
        get => mainText.enableWordWrapping;
        set
        {
            mainText.enableWordWrapping = value;
            if (shadowText != null) shadowText.enableWordWrapping = value;
        }
    }

    public TextOverflowModes overflowMode
    {
        get => mainText.overflowMode;
        set
        {
            mainText.overflowMode = value;
            if (shadowText != null) shadowText.overflowMode = value;
        }
    }

    public float characterSpacing
    {
        get => mainText.characterSpacing;
        set
        {
            mainText.characterSpacing = value;
            if (shadowText != null) shadowText.characterSpacing = value;
        }
    }

    public float lineSpacing
    {
        get => mainText.lineSpacing;
        set
        {
            mainText.lineSpacing = value;
            if (shadowText != null) shadowText.lineSpacing = value;
        }
    }

    public bool enabled_TextComponent
    {
        get => mainText.enabled;
        set
        {
            mainText.enabled = value;
            if (shadowText != null) shadowText.enabled = value;
        }
    }

    // 실제 TextMeshProUGUI 컴포넌트가 필요한 경우 (Tween 등 외부 라이브러리 연동용)
    public TextMeshProUGUI Main => mainText;
    public TextMeshProUGUI Shadow => shadowText;

    /// <summary>
    /// 위 프로퍼티에 없는 속성까지 한 번에 통째로 동기화하고 싶을 때 사용합니다.
    /// (position, color 계열은 의도적으로 제외)
    /// </summary>
    public void SyncAll()
    {
        if (shadowText == null || mainText == null) return;

        shadowText.text = mainText.text;
        shadowText.font = mainText.font;
        shadowText.fontSize = mainText.fontSize;
        shadowText.fontStyle = mainText.fontStyle;
        shadowText.fontWeight = mainText.fontWeight;
        shadowText.alignment = mainText.alignment;
        shadowText.enableWordWrapping = mainText.enableWordWrapping;
        shadowText.overflowMode = mainText.overflowMode;
        shadowText.characterSpacing = mainText.characterSpacing;
        shadowText.lineSpacing = mainText.lineSpacing;
        shadowText.wordSpacing = mainText.wordSpacing;
        shadowText.paragraphSpacing = mainText.paragraphSpacing;
        shadowText.margin = mainText.margin;
        shadowText.enabled = mainText.enabled;
        shadowText.richText = mainText.richText;
        shadowText.horizontalMapping = mainText.horizontalMapping;
        shadowText.verticalMapping = mainText.verticalMapping;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (mainText == null) mainText = GetComponent<TextMeshProUGUI>();
    }
#endif
}