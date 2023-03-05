using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] TMPro.TextMeshProUGUI bossHpText;
    [SerializeField] string bossName;

    public void UpdateBossHealthSlider(int currentHp)
    {
        slider.value = currentHp;
    }

    public void InitHealthBar(int maxHp, string _bossName)
    {
        slider.maxValue = maxHp;
        slider.value = maxHp;
        bossName = _bossName;
        SetBossHpText();
    }
    public void SetBossHpText()
    {
        bossHpText.text = bossName + " HP ";
    }
}
