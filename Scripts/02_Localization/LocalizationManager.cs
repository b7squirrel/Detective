using UnityEngine;

public enum Language { Korean, English }

public class LocalizationManager : MonoBehaviour
{
    [Header("Game Texts")]
    [SerializeField] private GameTexts koreanGameTexts;
    [SerializeField] private GameTexts englishGameTexts;
    
    [Header("Character Texts")]
    [SerializeField] private CharTexts koreanCharTexts;
    [SerializeField] private CharTexts englishCharTexts;
    
    [Header("Item Texts")]
    [SerializeField] private ItemTexts koreanItemTexts;
    [SerializeField] private ItemTexts englishItemTexts;
    
    [Header("Debug")]
    [SerializeField] private Language currentLanguage;
    
    // Singleton 추가
    public static LocalizationManager Instance { get; private set; }
    
    // Static 접근자
    public static GameTexts Game { get; private set; }
    public static CharTexts Char { get; private set; }
    public static ItemTexts Item { get; private set; }
    public static Language CurrentLanguage { get; private set; }
    
    // 언어 변경 이벤트
    public static event System.Action OnLanguageChanged;
    
    // 초기화 완료 여부
    public static bool IsInitialized { get; private set; }
    
    void Awake()
    {
        // Singleton 설정
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // 저장된 언어 불러오기 (기본: Korean = 0)
        int savedLangIndex = PlayerPrefs.GetInt("Language", 0);
        SetLanguage((Language)savedLangIndex);
        
        IsInitialized = true;
    }
    
    public void SetLanguage(Language language)
    {
        CurrentLanguage = language;
        
        // 각 Texts 할당
        Game = language == Language.Korean ? koreanGameTexts : englishGameTexts;
        Char = language == Language.Korean ? koreanCharTexts : englishCharTexts;
        Item = language == Language.Korean ? koreanItemTexts : englishItemTexts;
        
        // 언어 설정 저장
        PlayerPrefs.SetInt("Language", (int)language);
        PlayerPrefs.Save();
        
        // 디버그
        currentLanguage = language;
        
        IsInitialized = true;
        
        // UI 갱신 이벤트 발생
        OnLanguageChanged?.Invoke();
        
        Debug.Log($"Language changed to: {language}");
    }
    
    // 옵션 패널에서 호출할 메서드들
    public void SetKorean() => SetLanguage(Language.Korean);
    public void SetEnglish() => SetLanguage(Language.English);
}