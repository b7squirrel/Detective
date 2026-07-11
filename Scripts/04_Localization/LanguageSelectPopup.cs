using UnityEngine;
using TMPro;

public class LanguageSelectPopup : MonoBehaviour
{
    private const string LANGUAGE_SELECTED_KEY = "LanguageSelected";
    private const string LANGUAGE_INDEX_KEY = "LanguageIndex"; // SettingsUIPanel과 동일 키 공유

    [Header("UI References")]
    [SerializeField] TextMeshProUGUI languageText;

    // SettingsUIPanel과 동일한 순서 (0 = English, 1 = 한국어)
    private readonly string[] languages = { "English", "한국어" };
    private int currentLanguageIndex = 0;

    void Awake()
    {
        bool alreadySelected = PlayerPrefs.GetInt(LANGUAGE_SELECTED_KEY, 0) == 1;

        if (alreadySelected)
        {
            gameObject.SetActive(false);
            return;
        }

        // 시스템 언어 기준 기본값으로 시작 (SettingsUIPanel의 기본값 로직과 동일 기준)
        currentLanguageIndex = (Application.systemLanguage == SystemLanguage.Korean) ? 1 : 0;
        UpdateLanguageDisplay();
    }

    public void OnLeftArrowClick()
    {
        currentLanguageIndex--;
        if (currentLanguageIndex < 0)
            currentLanguageIndex = languages.Length - 1;

        UpdateLanguageDisplay();
    }

    public void OnRightArrowClick()
    {
        currentLanguageIndex++;
        if (currentLanguageIndex >= languages.Length)
            currentLanguageIndex = 0;

        UpdateLanguageDisplay();
    }

    void UpdateLanguageDisplay()
    {
        if (languageText != null)
            languageText.text = languages[currentLanguageIndex];

        if (LocalizationManager.Instance == null)
        {
            Logger.LogError("[LanguageSelectPopup] LocalizationManager.Instance가 없습니다.");
            return;
        }

        if (currentLanguageIndex == 0)
            LocalizationManager.Instance.SetEnglish();
        else
            LocalizationManager.Instance.SetKorean();
    }

    /// <summary>
    /// 확인 버튼 클릭 — 선택을 확정하고 팝업을 닫습니다.
    /// 이후로는 다시 뜨지 않습니다.
    /// </summary>
    public void OnConfirmClick()
    {
        // SettingsUIPanel이 나중에 읽을 값도 같이 저장 (일관성 유지)
        PlayerPrefs.SetInt(LANGUAGE_INDEX_KEY, currentLanguageIndex);
        PlayerPrefs.SetInt(LANGUAGE_SELECTED_KEY, 1);
        PlayerPrefs.Save();

        gameObject.SetActive(false);
    }
}