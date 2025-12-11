using UnityEngine;

public enum Language { Korean, English }

[DefaultExecutionOrder(-100)] // 다른 스크립트보다 먼저 실행
public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }
    
    public static bool IsInitialized { get; private set; }
    
    [Header("Game Texts")]
    [SerializeField] private GameTexts koreanGameTexts;
    [SerializeField] private GameTexts englishGameTexts;
    
    [Header("Character Texts")]
    [SerializeField] private CharTexts koreanCharTexts;
    [SerializeField] private CharTexts englishCharTexts;
    
    [Header("Item Texts")]
    [SerializeField] private ItemTexts koreanItemTexts;
    [SerializeField] private ItemTexts englishItemTexts;
    
    [Header("Achievement Texts")]
    [SerializeField] private AchievementTexts koreanAchievementTexts;
    [SerializeField] private AchievementTexts englishAchievementTexts;
    
    [Header("Debug")]
    [SerializeField] private Language currentLanguage;
    
    // Static 접근자
    public static GameTexts Game { get; private set; }
    public static CharTexts Char { get; private set; }
    public static ItemTexts Item { get; private set; }
    public static AchievementTexts Achievement { get; private set; }
    public static Language CurrentLanguage { get; private set; }
    
    // 언어 변경 이벤트
    public static event System.Action OnLanguageChanged;
    
    void Awake()
    {
        // Singleton 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // 저장된 언어 불러오기 (기본: Korean = 0)
        int savedLangIndex = PlayerPrefs.GetInt("Language", 0);
        SetLanguage((Language)savedLangIndex);
        
        // ★ 이 줄 추가 (필수!)
        IsInitialized = true;
        
        Debug.Log("LocalizationManager initialized");
    }
    
    public void SetLanguage(Language language)
    {
        CurrentLanguage = language;
        
        // 각 Texts 할당
        Game = language == Language.Korean ? koreanGameTexts : englishGameTexts;
        Char = language == Language.Korean ? koreanCharTexts : englishCharTexts;
        Item = language == Language.Korean ? koreanItemTexts : englishItemTexts;
        Achievement = language == Language.Korean ? koreanAchievementTexts : englishAchievementTexts;
        
        // Null 체크
        if (Game == null || Char == null || Item == null || Achievement == null)
        {
            Debug.LogError("LocalizationManager: One or more Texts assets are not assigned!");
            return;
        }
        
        // 언어 설정 저장
        PlayerPrefs.SetInt("Language", (int)language);
        PlayerPrefs.Save();
        
        // 디버그
        currentLanguage = language;
        
        // UI 갱신 이벤트 발생
        OnLanguageChanged?.Invoke();
        
        Debug.Log($"Language changed to: {language}");
    }
    
    // 옵션 패널에서 호출할 메서드들
    public void SetKorean() => SetLanguage(Language.Korean);
    public void SetEnglish() => SetLanguage(Language.English);
}