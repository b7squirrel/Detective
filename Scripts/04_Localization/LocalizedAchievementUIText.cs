using TMPro;
using UnityEngine;

public class LocalizedAchievementUIText : MonoBehaviour
{
    [Header("Achievements Panel")]
    [SerializeField] TextMeshProUGUI achievementsTitle;
    

    void Awake()
    {
        LocalizationManager.OnLanguageChanged += UpdateText;
    }
    void Start()
    {
        UpdateText();
    }
    void OnDestroy()
    {
        LocalizationManager.OnLanguageChanged -= UpdateText;
    }
    void UpdateText()
    {
        achievementsTitle.text = LocalizationManager.Game.achievementsTitle;
    }
}
