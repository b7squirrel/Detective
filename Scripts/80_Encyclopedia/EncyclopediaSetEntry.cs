// ============================================================
//  파일 2 of 4 : EncyclopediaSetEntry.cs
//  역할 : ScrollView 내 세트 카드 1줄 (세트명 + 4슬롯 아이콘)
// ============================================================
using UnityEngine;
using UnityEngine.UI;
using TMPro;
 
public class EncyclopediaSetEntry : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] TextMeshProUGUI setNameText;
    // 인덱스 순서: 0=Head, 1=Face, 2=Chest, 3=Hand
    [SerializeField] Image[]         slotImages;   // 4개
    [SerializeField] Button          tapButton;
 
    static readonly Color COLOR_FILLED = Color.white;
    static readonly Color COLOR_EMPTY  = new Color(0.35f, 0.35f, 0.35f, 0.4f);
 
    EncycSetInfo currentInfo;
 
    /// <summary>EncyclopediaManager가 Instantiate 직후 호출</summary>
    public void Init(EncycSetInfo info, System.Action<EncycSetInfo> onTap)
    {
        currentInfo      = info;
        setNameText.text = info.setName;
 
        // 슬롯 색상 (데이터 있으면 컬러, 없으면 흑백)
        for (int i = 0; i < slotImages.Length; i++)
        {
            if (slotImages[i] != null)
                slotImages[i].color = info.HasItemInSlot(i) ? COLOR_FILLED : COLOR_EMPTY;
        }
 
        tapButton.onClick.RemoveAllListeners();
        tapButton.onClick.AddListener(() => onTap(info));
    }
}