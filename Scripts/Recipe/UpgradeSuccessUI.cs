using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeSuccessUI : CardDisplay
{
    public void SetCard(WeaponItemData weaponItemData)
    {
        // 별을 다 없애기
        int childCount = starContainer.childCount;

        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = starContainer.GetChild(i);
            Destroy(child.gameObject);
        }

        // 카드 타입에 따라 초기화
        if (weaponItemData.itemData == null)
        {
            InitWeaponCardDisplay(weaponItemData.weaponData);
        }
        else if (weaponItemData.weaponData == null)
        {
            InitItemCardDisplay(weaponItemData.itemData);
        }
    }

    protected override void SetNumStar(int numStars)
    {
        // 등급만큼 별 생성하고 별리스트에 넣기
        for (int i = 0; i < numStars; i++)
        {
            Instantiate(starPrefab, starContainer);
        }
    }
}
