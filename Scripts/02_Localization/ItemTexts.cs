using UnityEngine;

[CreateAssetMenu(fileName = "ItemTexts", menuName = "Localization/Item Texts")]
public class ItemTexts : ScriptableObject
{
    [Header("Item Display Names")]
    [Tooltip("Item의 Name과 매칭됩니다")]
    public ItemLocalizedName[] itemNames = new ItemLocalizedName[9];
    
    [System.Serializable]
    public class ItemLocalizedName
    {
        [Tooltip("Item.Name (내부 식별용)")]
        public string itemInternalName;
        
        [Tooltip("화면에 표시될 이름")]
        public string displayName;
    }
    
    // 내부 이름으로 표시 이름 찾기
    public string GetItemDisplayName(string itemInternalName)
    {
        foreach (var item in itemNames)
        {
            if (item != null && item.itemInternalName == itemInternalName)
                return item.displayName;
        }
        Debug.LogWarning($"Item display name not found: {itemInternalName}");
        return itemInternalName;
    }
    
    [Header("Item Skill Names")]
    public string[] itemSkillNames = new string[]
    {
        "모든 공격",
        "모든 방어",
        "이동 속도",
        "넓은 자석",
    };
    
    [Header("Item Skill Descriptions")]
    public string[] itemSkillDescriptions = new string[]
    {
        "모든 오리들의 공격력을 높여줍니다.",
        "리드 오리의 방어력을 높여줍니다.",
        "리드 오리의 이동 속도를 높여줍니다.",
        "자력 범위를 더 넓혀 줍니다.",
    };
}