using UnityEngine;

public class NewCardStatsDisplay : MonoBehaviour
{
    [System.Serializable]
    public class StatRow
    {
        public GameObject rowObject;              // Atk_Hp (또는 복제된 두 번째 줄) 오브젝트
        public TMPro.TextMeshProUGUI statText;     // Additaional Stats Text (TMP)_1
        public GameObject atkIcon;                 // Image Atk
        public GameObject hpIcon;                  // Image Hp
    }

    [Header("첫 번째 줄 (기존 Atk_Hp)")]
    [SerializeField] StatRow atkRow;

    [Header("두 번째 줄 (복제해서 새로 만든 행)")]
    [SerializeField] StatRow hpRow;

    /// <summary>
    /// 합성 결과로 오른 ATK/HP 증가량을 받아서 표시.
    /// 값이 0 이하인 스탯은 해당 줄을 통째로 숨김.
    /// </summary>
    public void SetStats(int atkGain, int hpGain)
    {
        SetRow(atkRow, atkGain, showAtkIcon: true);
        SetRow(hpRow, hpGain, showAtkIcon: false);
    }

    void SetRow(StatRow row, int gain, bool showAtkIcon)
    {
        if (row == null || row.rowObject == null) return;

        if (gain <= 0)
        {
            row.rowObject.SetActive(false);
            return;
        }

        row.rowObject.SetActive(true);
        if (row.statText != null) row.statText.text = "+" + gain;

        if (row.atkIcon != null) row.atkIcon.SetActive(showAtkIcon);
        if (row.hpIcon != null) row.hpIcon.SetActive(!showAtkIcon);
    }
}