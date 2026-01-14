using UnityEngine;
using TMPro;

public class SettingsUIPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] TextMeshProUGUI languageText;
    [SerializeField] GameObject leftArrow;
    [SerializeField] GameObject rightArrow;
    LocalizationManager localicationManager;

    // 지원하는 언어 목록
    private string[] languages = { "English", "한국어"};
    private int currentLanguageIndex = 0;

    private void Start()
    {
        LoadLanguage();
        UpdateLanguageDisplay();
    }

    // 왼쪽 화살표 버튼 이벤트
    public void OnLeftArrowClick()
    {
        currentLanguageIndex--;
        
        // 첫 번째 언어에서 왼쪽을 누르면 마지막 언어로
        if (currentLanguageIndex < 0)
        {
            currentLanguageIndex = languages.Length - 1;
        }
        
        UpdateLanguageDisplay();
        SaveLanguage();
    }

    // 오른쪽 화살표 버튼 이벤트
    public void OnRightArrowClick()
    {
        currentLanguageIndex++;
        
        // 마지막 언어에서 오른쪽을 누르면 첫 번째 언어로
        if (currentLanguageIndex >= languages.Length)
        {
            currentLanguageIndex = 0;
        }
        
        UpdateLanguageDisplay();
        SaveLanguage();
    }

    private void UpdateLanguageDisplay()
    {
        languageText.text = languages[currentLanguageIndex];

        // TODO: 실제 게임 언어 변경 로직
        // LocalizationManager.SetLanguage(languages[currentLanguageIndex]);
        if (localicationManager == null) localicationManager = FindObjectOfType<LocalizationManager>();
        if (currentLanguageIndex == 0)
        {
            localicationManager.SetEnglish();
        }
        else if (currentLanguageIndex == 1)
        {
            localicationManager.SetKorean();
        }
    }

    private void SaveLanguage()
    {
        PlayerPrefs.SetInt("LanguageIndex", currentLanguageIndex);
        PlayerPrefs.Save();
    }

    private void LoadLanguage()
    {
        if (PlayerPrefs.HasKey("LanguageIndex"))
        {
            currentLanguageIndex = PlayerPrefs.GetInt("LanguageIndex");
            
            // 인덱스 범위 검증
            if (currentLanguageIndex < 0 || currentLanguageIndex >= languages.Length)
            {
                currentLanguageIndex = 0;
            }
        }
        else
        {
            // 기본값: 시스템 언어에 따라 설정
            currentLanguageIndex = GetDefaultLanguageIndex();
            SaveLanguage();
        }
    }

    // 시스템 언어에 따라 기본 언어 설정
    private int GetDefaultLanguageIndex()
    {
        SystemLanguage systemLang = Application.systemLanguage;
        
        switch (systemLang)
        {
            case SystemLanguage.Korean:
                return 1; // 한국어
            case SystemLanguage.Japanese:
                return 2; // 日本語
            case SystemLanguage.Chinese:
            case SystemLanguage.ChineseSimplified:
            case SystemLanguage.ChineseTraditional:
                return 3; // 中文
            default:
                return 0; // English
        }
    }

    // 현재 언어 가져오기 (다른 스크립트에서 사용)
    public string GetCurrentLanguage()
    {
        return languages[currentLanguageIndex];
    }

    // 디버그용
    void DeleteLanguagePrefs()
    {
        PlayerPrefs.DeleteKey("LanguageIndex");
        Debug.Log("Language preference has been deleted.");
    }
}
