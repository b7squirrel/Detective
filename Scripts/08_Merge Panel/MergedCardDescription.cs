using UnityEngine;
public class MergedCardDescription : MonoBehaviour
{
    [SerializeField] GameObject skillPanel;
    [SerializeField] TMPro.TextMeshProUGUI SkillTitle;
    [SerializeField] TMPro.TextMeshProUGUI SkillDescription;

    CardData currentCardData;

    void Awake()
    {
        LocalizationManager.OnLanguageChanged += UpdateText;
    }

    void OnDestroy()
    {
        LocalizationManager.OnLanguageChanged -= UpdateText;
    }

    public void UpdateSkillDescription(CardData cardData)
    {
        currentCardData = cardData;
        UpdateText();
    }

    void UpdateText()
    {
        if (currentCardData == null || LocalizationManager.Char == null) return;

        int index = currentCardData.PassiveSkill - 1;
        if (index < 0 || index >= LocalizationManager.Char.skillNames.Length) return;

        // 스킬
        SkillTitle.text = LocalizationManager.Char.skillNames[index];
        SkillDescription.text = LocalizationManager.Char.skillDescriptions[index];
        SkillTitle.color = MyGrade.GradeColors[currentCardData.Grade];
    }
}