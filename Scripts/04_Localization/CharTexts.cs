using UnityEngine;

[CreateAssetMenu(fileName = "CharTexts", menuName = "Localization/Character Texts")]
public class CharTexts : ScriptableObject
{
    [Header("Weapon Display Names")]
    [Tooltip("WeaponData의 Name과 매칭됩니다")]
    public WeaponLocalizedName[] weaponNames = new WeaponLocalizedName[9];
    
    [System.Serializable]
    public class WeaponLocalizedName
    {
        [Tooltip("WeaponData.Name (내부 식별용)")]
        public string weaponInternalName;
        
        [Tooltip("화면에 표시될 이름")]
        public string displayName;
        
        [Tooltip("시너지 설명용 이름")]
        public string synergyDisplayName;
    }
    
    // 내부 이름으로 표시 이름 찾기
    public string GetWeaponDisplayName(string weaponInternalName)
    {
        foreach (var weapon in weaponNames)
        {
            if (weapon != null && weapon.weaponInternalName == weaponInternalName)
                return weapon.displayName;
        }
        Debug.LogWarning($"Weapon display name not found: {weaponInternalName}");
        return weaponInternalName;
    }
    
    public string GetWeaponSynergyName(string weaponInternalName)
    {
        foreach (var weapon in weaponNames)
        {
            if (weapon != null && weapon.weaponInternalName == weaponInternalName)
                return weapon.synergyDisplayName;
        }
        return weaponInternalName;
    }
    
    [Header("Skill Names")]
    public string[] skillNames = new string[]
    {
        "강철 피부",
        "느림보 최면술",
        "넓은 공격",
        "천하 무적",
        "파티 타임"
    };
    
    [Header("Skill Descriptions")]
    public string[] skillDescriptions = new string[]
    {
        "동료들이 몸으로 적들의 공격을 막아줍니다.",
        "잠시 최면을 걸어 적들을 느려지게 합니다.",
        "화면 안의 모든 적들에게 데미지를 줍니다.",
        "잠시동안 무적이 됩니다.",
        "잠시동안 자신과 동료들의 공격력을 올려줍니다."
    };
}