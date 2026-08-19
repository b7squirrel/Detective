using System.Collections.Generic;

// ⭐ 동료 오리 1마리 정보
// 리드 오리(StartingDataContainer 필드들)와 달리 여러 마리이므로 리스트로 관리하기 위해 별도 클래스로 분리
[System.Serializable]
public class CompanionData
{
    public CardData cardData;          // 보유 카드 원본 (Level, Atk, Hp 등 스탯 정보)
    public WeaponData weaponData;      // 종류 (Cannon, Bow 등) - 필드에서 실제로 붙는 무기/스프라이트 결정
    public List<Item> equippedItems;   // 실제 장착한 장비 4칸 (Head, Chest, Face, Hand 순서) - 스프라이트용

    // ⭐ 카드 자체 Atk/Hp + 장착 장비 Atk/Hp 합산값 (SetCompanions에서 계산되어 캐싱됨)
    public int totalAtk;
    public int totalHp;
}