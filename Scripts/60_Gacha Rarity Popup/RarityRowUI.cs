using UnityEngine;
using TMPro;

public class RarityRowUI : MonoBehaviour
{
    static readonly string[] GRADE_COLORS = new string[]
    {
        "#999999", // Common
        "#80FF00", // Rare
        "#00CCFF", // Epic
        "#B24CFF", // Legendary
        "#FFCC00", // Mythic
    };

    [SerializeField] private TextMeshProUGUI gradeText;
    [SerializeField] private TextMeshProUGUI percentText;

    public void SetInfo(int rarity, float percent)
    {
        var g = LocalizationManager.Game;
        if (g == null) return;

        string gradeName = (rarity >= 0 && rarity < g.gradeNames.Length)
            ? g.gradeNames[rarity]
            : "Unknown";

        string color = (rarity >= 0 && rarity < GRADE_COLORS.Length)
            ? GRADE_COLORS[rarity]
            : "white";

        if (gradeText != null)
            gradeText.text = $"<color={color}>{gradeName}</color>";

        if (percentText != null)
            percentText.text = $"{percent:F2}%";
    }
}