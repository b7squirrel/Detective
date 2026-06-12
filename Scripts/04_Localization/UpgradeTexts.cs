using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeTexts", menuName = "Localization/Upgrade Texts")]
public class UpgradeTexts : ScriptableObject
{
    [System.Serializable]
    public class UpgradeLocalizedText
    {
        [Tooltip("UpgradeData 에셋 파일명과 정확히 일치해야 합니다")]
        public string upgradeName;
        [TextArea] public string description;
    }

    [Header("오리 업그레이드")]
    public UpgradeLocalizedText[] weaponUpgradeTexts;

    [Header("아이템 업그레이드")]
    public UpgradeLocalizedText[] itemUpgradeTexts;

    [Header("아이템 이름")]
    public UpgradeLocalizedText[] itemNameTexts;

    public string GetDescription(string upgradeName)
    {
        // 오리 업그레이드에서 먼저 검색
        if (weaponUpgradeTexts != null)
        {
            foreach (var t in weaponUpgradeTexts)
            {
                if (t != null && t.upgradeName == upgradeName)
                    return t.description;
            }
        }

        // 아이템 업그레이드에서 검색
        if (itemUpgradeTexts != null)
        {
            foreach (var t in itemUpgradeTexts)
            {
                if (t != null && t.upgradeName == upgradeName)
                    return t.description;
            }
        }

        Debug.LogWarning($"[UpgradeTexts] 설명을 찾을 수 없습니다: {upgradeName}");
        return upgradeName;
    }
    
    public string GetItemName(string itemName)
    {
        if (itemNameTexts != null)
        {
            foreach (var t in itemNameTexts)
            {
                if (t != null && t.upgradeName == itemName)
                    return t.description;
            }
        }
        Debug.LogWarning($"[UpgradeTexts] 아이템 이름을 찾을 수 없습니다: {itemName}");
        return itemName;
    }
}