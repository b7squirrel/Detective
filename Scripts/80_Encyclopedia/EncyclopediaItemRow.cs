// ============================================================
//  파일 4 of 4 : EncyclopediaItemRow.cs
//  역할 : 팝업 내 아이템 1행 (슬롯 라벨 / 아이템명 / HP / ATK)
// ============================================================
using UnityEngine;
using UnityEngine.UI;
using TMPro;
 
public class EncyclopediaItemRow : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] TextMeshProUGUI slotLabel;        // "Head" / "Face" 등
    [SerializeField] TextMeshProUGUI itemNameText;
    [SerializeField] TextMeshProUGUI hpText;           // 0이면 숨김
    [SerializeField] TextMeshProUGUI atkText;          // 0이면 숨김
    [SerializeField] GameObject      essentialBadge;   // Essential 뱃지 오브젝트
    [SerializeField] Image           rowBackground;    // 빈 행 색상 구분용
 
    static readonly Color COLOR_NORMAL = new Color(0.15f, 0.15f, 0.2f, 1f);
    static readonly Color COLOR_EMPTY  = new Color(0.08f, 0.08f, 0.1f, 1f);
 
    // ── 데이터 있는 행 ────────────────────────────────────────
    public void Init(string slot, string displayName,
                     bool isEssential, int hp, int atk)
    {
        if (rowBackground != null) rowBackground.color = COLOR_NORMAL;
 
        slotLabel.text      = slot;
        itemNameText.text   = displayName;
        itemNameText.color  = Color.white;
 
        // HP
        bool showHp = hp > 0;
        hpText.gameObject.SetActive(showHp);
        if (showHp) hpText.text = $"♥ {hp:N0}";
 
        // ATK
        bool showAtk = atk > 0;
        atkText.gameObject.SetActive(showAtk);
        if (showAtk) atkText.text = $"⚔ {atk}";
 
        // Essential 뱃지
        if (essentialBadge != null)
            essentialBadge.SetActive(isEssential);
    }
 
    // ── 빈 슬롯 행 ───────────────────────────────────────────
    public void InitEmpty(string slot)
    {
        if (rowBackground != null) rowBackground.color = COLOR_EMPTY;
 
        slotLabel.text     = slot;
        itemNameText.text  = "-";
        itemNameText.color = new Color(0.4f, 0.4f, 0.4f);
 
        hpText.gameObject.SetActive(false);
        atkText.gameObject.SetActive(false);
        if (essentialBadge != null) essentialBadge.SetActive(false);
    }
}