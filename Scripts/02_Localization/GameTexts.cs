using UnityEngine;

[CreateAssetMenu(fileName = "GameTexts", menuName = "Localization/Game Texts")]
public class GameTexts : ScriptableObject
{
    [Header("Loading Messages")]
    public string loadingCardData = "오리 친구들 준비 중...";
    public string loadingPlayerData = "리드 오리 준비 중...";
    public string loadingEquipment = "장비 준비 중...";
    public string loadingComplete = "출동 준비 중...";
    public string loading = "로딩 중";

    [Header("Grade Names")]
    public string[] gradeNames = new string[]
    {
        "일반", "희귀", "고급", "전설", "신화"
    };
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

        [Tooltip("시너지 설명용 이름 (선택사항)")]
        public string synergyDisplayName;
    }

    // 내부 이름으로 표시 이름 찾기
    public string GetWeaponDisplayName(string weaponInternalName)
    {
        foreach (var weapon in weaponNames)
        {
            if (weapon.weaponInternalName == weaponInternalName)
                return weapon.displayName;
        }
        return weaponInternalName; // 못 찾으면 내부 이름 그대로 반환
    }

    public string GetWeaponSynergyName(string weaponInternalName)
    {
        foreach (var weapon in weaponNames)
        {
            if (weapon.weaponInternalName == weaponInternalName)
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

    [Header("Launch Panel")]
    public string tabToSelectLead = "탭해서 리드 오리 선택.";
    public string startButton = "시작!";
    public string level = "레벨";
}
