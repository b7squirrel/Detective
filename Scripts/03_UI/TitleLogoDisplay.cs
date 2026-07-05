using UnityEngine;
using UnityEngine.UI;

public class TitleLogoDisplay : MonoBehaviour
{
    [Header("Logo Sprites")]
    [SerializeField] private Sprite koreanLogo;
    [SerializeField] private Sprite englishLogo;

    [Header("Target")]
    [SerializeField] private Image logoImage;

    void Start()
    {
        // LocalizationManager가 먼저 실행되도록 DefaultExecutionOrder(-100)이 걸려 있으므로
        // Start 시점엔 이미 CurrentLanguage가 세팅되어 있음
        if (!LocalizationManager.IsInitialized)
        {
            Debug.LogWarning("LocalizationManager not initialized yet — defaulting to Korean logo");
            logoImage.sprite = koreanLogo;
            return;
        }

        logoImage.sprite = LocalizationManager.CurrentLanguage == Language.Korean
            ? koreanLogo
            : englishLogo;
    }
}