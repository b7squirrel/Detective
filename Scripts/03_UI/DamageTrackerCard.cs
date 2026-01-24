using TMPro;
using UnityEngine;

public class DamageTrackerCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private TextMeshProUGUI weaponDamageText;
    [SerializeField] private TextMeshProUGUI weaponDpsText;

    public void InitDamageTrackerCard(string weaponName)
    {
        this.weaponNameText.text = weaponName;
    }

    public void UpdateCard(int totalDamage, float dps1, float dps5)
    {
        this.weaponDamageText.text = $"{totalDamage:N0}";
        // this.weaponDpsText.text = $"DPS: {dps1:F1} / {dps5:F1}";
        this.weaponDpsText.text = $"{dps5:F1}";
    }
}