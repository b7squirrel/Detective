using UnityEngine;
using TMPro;

public class LanguageSelectPopup : MonoBehaviour
{
    private const string LANGUAGE_SELECTED_KEY = "LanguageSelected";
    private const string LANGUAGE_INDEX_KEY = "LanguageIndex";

    [Header("UI References")]
    [SerializeField] TextMeshProUGUI languageText;

    private readonly string[] languages = { "English", "한국어" };
    private int currentLanguageIndex = 0;

    // ⭐ 추가: 다른 스크립트(GameInitializer)가 대기할 수 있는 정적 플래그
    public static bool IsConfirmed { get; private set; } = false;

    void Awake()
    {
        int savedValue = PlayerPrefs.GetInt(LANGUAGE_SELECTED_KEY, -1); // -1 = 키 자체가 없음
        Logger.Log($"[LanguageSelectPopup] 저장된 LanguageSelected 값: {savedValue}");

        bool alreadySelected = PlayerPrefs.GetInt(LANGUAGE_SELECTED_KEY, 0) == 1;

        if (alreadySelected)
        {
            IsConfirmed = true; // ⭐ 이미 선택된 유저는 즉시 통과
            gameObject.SetActive(false);
            return;
        }

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

    public void OnConfirmClick()
    {
        PlayerPrefs.SetInt(LANGUAGE_INDEX_KEY, currentLanguageIndex);
        PlayerPrefs.SetInt(LANGUAGE_SELECTED_KEY, 1);
        PlayerPrefs.Save();

        IsConfirmed = true; // ⭐ 확인 버튼을 눌러야 비로소 통과
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 디버그용 — 언어 선택 기록을 초기화합니다.
    /// 다음 앱 실행(또는 씬 재시작) 시 언어 선택 팝업이 다시 뜹니다.
    /// </summary>
    public void ResetLanguageSelection()
    {
        PlayerPrefs.DeleteKey(LANGUAGE_SELECTED_KEY);
        PlayerPrefs.DeleteKey(LANGUAGE_INDEX_KEY);
        PlayerPrefs.Save();

        IsConfirmed = false;

        Logger.Log("[LanguageSelectPopup] 언어 선택 기록 초기화 완료. 다음 실행 시 팝업이 다시 표시됩니다.");
    }
}