using UnityEngine;

public class MergedCardDescription : MonoBehaviour
{
    [SerializeField] GameObject skillPanel;
    [SerializeField] TMPro.TextMeshProUGUI SkillTitle;
    [SerializeField] TMPro.TextMeshProUGUI SkillDescription;

    public void UpdateSkillDescription(CardData cardData)
    {
        // ��ų
        SkillTitle.text = Skills.SkillNames[cardData.PassiveSkill - 1];
        SkillDescription.text = Skills.SkillDescriptions[cardData.PassiveSkill - 1];

        SkillTitle.color = MyGrade.GradeColors[cardData.Grade];
    }
}
