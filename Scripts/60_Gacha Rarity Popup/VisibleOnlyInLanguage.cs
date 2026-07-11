using UnityEngine;

/// <summary>
/// 지정된 언어일 때만 이 오브젝트를 활성화합니다.
/// 예: 확률 정보 버튼을 한국어 설정에서만 노출.
/// </summary>
public class VisibleOnlyInLanguage : MonoBehaviour
{
    [SerializeField] private Language visibleLanguage = Language.Korean;

    void Start()
    {
        LocalizationManager.OnLanguageChanged += UpdateVisibility;
        UpdateVisibility();
    }

    void OnDestroy()
    {
        LocalizationManager.OnLanguageChanged -= UpdateVisibility;
    }

    void UpdateVisibility()
    {
        gameObject.SetActive(LocalizationManager.CurrentLanguage == visibleLanguage);
    }
}