using UnityEngine;
using TMPro;

public class DamageUI : MonoBehaviour
{
    [Header("Garlic Weapon UI")]
    [SerializeField] private TextMeshProUGUI garlicTotalDamageText;
    [SerializeField] private TextMeshProUGUI garlicDPS1SecondText;
    [SerializeField] private TextMeshProUGUI garlicDPS5SecondText;

    [Header("Lightning Weapon UI")]
    [SerializeField] private TextMeshProUGUI lightningTotalDamageText;
    [SerializeField] private TextMeshProUGUI lightningDPS1SecondText;
    [SerializeField] private TextMeshProUGUI lightningDPS5SecondText;

    [Header("Update Settings")]
    [SerializeField] private float updateInterval = 0.1f;
    private float updateTimer;

    void Update()
    {
        if (GameManager.instance.IsPaused) return;

        updateTimer += Time.deltaTime;

        if (updateTimer >= updateInterval)
        {
            UpdateUI();
            updateTimer = 0f;
        }
    }

    void UpdateUI()
    {
        if (DamageTracker.instance == null) return;

        // Garlic 무기 데이터 갱신
        UpdateWeaponUI("Garlic", garlicTotalDamageText, garlicDPS1SecondText, garlicDPS5SecondText);

        // Lightning 무기 데이터 갱신
        UpdateWeaponUI("Lightning", lightningTotalDamageText, lightningDPS1SecondText, lightningDPS5SecondText);
    }

    void UpdateWeaponUI(string weaponName, TextMeshProUGUI totalText, TextMeshProUGUI dps1Text, TextMeshProUGUI dps5Text)
    {
        // 총 누적 데미지
        int totalDamage = DamageTracker.instance.GetTotalDamage(weaponName);
        totalText.text = $"{weaponName} Total: {totalDamage:N0}";

        // 1초 DPS
        float dps1 = DamageTracker.instance.GetDPS_1Second(weaponName);
        dps1Text.text = $"{weaponName} DPS (1s): {dps1:F1}";

        // 5초 DPS
        float dps5 = DamageTracker.instance.GetDPS_5Second(weaponName);
        dps5Text.text = $"{weaponName} DPS (5s): {dps5:F1}";
    }
}