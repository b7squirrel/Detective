using System.Collections.Generic;
using UnityEngine;

public class GachaPanelManager : MonoBehaviour
{
    [SerializeField] GameObject CardPicked;

    CardDisp cardDisp;
    PauseCardDisp pauseCardDisp;
    CardsDictionary cardDictionary;
    public void InitGachaPanel(List<CardData> cards)
    {
        CardData cardData = cards[0];

        if (cardDisp == null) cardDisp = CardPicked.GetComponent<CardDisp>();
        if (pauseCardDisp == null) pauseCardDisp = CardPicked.GetComponent<PauseCardDisp>();
        if (cardDictionary == null) cardDictionary =FindObjectOfType<CardsDictionary>();

        WeaponData wd = cardDictionary.GetWeaponItemData(cardData).weaponData;
        Debug.Log($"weapon name is {wd.DisplayName}");
        cardDisp.InitWeaponCardDisplay(wd, cardData);
        cardDisp.InitSpriteRow();
        for (int i = 0; i < 4; i++)
        {
            Item item = wd.defaultItems[i];

            if (item == null)
            {
                cardDisp.SetEquipCardDisplay(i, null, false, Vector2.zero); // 이미지 오브젝트를 비활성화
                continue;
            }
            SpriteRow equipmentSpriteRow = item.spriteRow;
            Vector2 offset = item.needToOffset ? item.posHead : Vector2.zero;

            cardDisp.SetEquipCardDisplay(i, equipmentSpriteRow, false, offset);
        }
    }

    void DebugGacha(List<CardData> cards)
    {
        foreach (var item in cards)
        {
            string grade = MyGrade.mGrades[item.Grade].ToString();
            Debug.Log($"{grade} {item.Name}을 뽑았습니다.");
        }
    }
}
