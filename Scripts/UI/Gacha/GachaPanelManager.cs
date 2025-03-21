using System.Collections.Generic;
using UnityEngine;

public class GachaPanelManager : MonoBehaviour
{
    [SerializeField] GameObject CardPicked; // 생성을 위한 프리펩
    [SerializeField] RectTransform resultPanel;


    CardDisp cardDisp;
    PauseCardDisp pauseCardDisp;
    CardsDictionary cardDictionary;
    public void InitGachaPanel(List<CardData> cards)
    {
        if (cardDictionary == null) cardDictionary =FindObjectOfType<CardsDictionary>();

        for (int i = 0; i < cards.Count; i++)
        {
            var slot = Instantiate(CardPicked, resultPanel);
            slot.transform.position = Vector3.zero;
            slot.transform.localScale = .5f * Vector2.one;

            WeaponData wData = cardDictionary.GetWeaponItemData(cards[i]).weaponData;
            CardDisp cardDisp = slot.GetComponent<CardDisp>();
            PauseCardDisp pCardDisp = slot.GetComponent<PauseCardDisp>();

            cardDisp.InitWeaponCardDisplay(wData, cards[i]);
            cardDisp.InitSpriteRow();

            for (int j = 0; i < 4; i++)
            {
                Item item = wData.defaultItems[j];

                if (item == null)
                {
                    Debug.Log($"{wData.DisplayName}의 {j}번째 아이템이 NULL입니다.");
                    cardDisp.SetEquipCardDisplay(j, null, false, Vector2.zero); // 이미지 오브젝트를 비활성화
                    continue;
                }
                SpriteRow equipmentSpriteRow = item.spriteRow;
                Vector2 offset = item.needToOffset ? item.posHead : Vector2.zero;

                cardDisp.SetEquipCardDisplay(j, equipmentSpriteRow, false, offset);
            }
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
