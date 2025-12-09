using UnityEngine;

public class GameTextsManager : MonoBehaviour
{
    [SerializeField] GameTexts koreanTexts;
    [SerializeField] GameTexts englishTexts;

    [Header("Debug")]
    [SerializeField] GameTexts CurrentLanguage;

    public static GameTexts Texts { get; private set; }

    void Awake()
    {
        // 저장된 언어 불러오기 (기본: 한국어)
        string savedLanguage = PlayerPrefs.GetString("Language", "Korean");
        SetLanguage(savedLanguage);
    }

    public void SetLanguage(string language)
    {
        Texts = language == "Korean" ? koreanTexts : englishTexts;

        // 언어 설정 저장
        PlayerPrefs.SetString("Language", language);
        PlayerPrefs.Save(); // 즉시 저장 (선택사항이지만 안전함)

        // 디버그
        CurrentLanguage = Texts;
        // UI 갱신 이벤트 발생 (필요시)
    }

    // 모든 UI 텍스트를 갱신하는 메서드
    void RefreshAllUI()
    {
        // 이벤트 방식으로 알림
        OnLanguageChanged?.Invoke();
    }

    // 언어 변경 이벤트 (선택사항)
    public static event System.Action OnLanguageChanged;
}
