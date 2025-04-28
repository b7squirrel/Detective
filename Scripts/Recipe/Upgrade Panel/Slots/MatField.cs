using System.Collections.Generic;
using UnityEngine;

public class MatField : MonoBehaviour
{
    #region 참조 변수
    CardDataManager cardDataManager;
    CardList cardList;
    CardSlotManager cardSlotManager;
    #endregion

    #region 슬롯 생성 관련 변수
    List<CardData> slotsOnField = new();
    #endregion

    #region 유니티 콜백 함수
    void Awake()
    {
        cardDataManager = FindObjectOfType<CardDataManager>();
        cardList = FindObjectOfType<CardList>();
        cardSlotManager = FindObjectOfType<CardSlotManager>();
    }
    #endregion

    #region MatCards 관련
    public void GenerateMatCardsList(CardData cardDataOnUpSlot)
    {
        // 슬롯 위의 CardData들 (= MyCardsList)
        List<CardData> myCardData = new();
        myCardData.AddRange(cardDataManager.GetMyCardList());
        myCardData.Remove(cardDataOnUpSlot); // 업그레이드 슬롯 위의 카드는 빼기

        // 슬롯 위의 카드들 중 업그레이드 슬롯 카드와 같은 이름, 같은 등급을 가진 카드를 추려냄
        List<CardData> picked = new();
        picked.AddRange(new CardClassifier().GetCardsAvailableForMat(myCardData, cardDataOnUpSlot));

        // 장비가 필수장비이고, 다른 오리에게 장착되어 있으면 건너뛴다
        if (cardDataOnUpSlot.Type == CardType.Item.ToString())
        {
            List<CardData> myCardList = new();
            myCardList.AddRange(cardDataManager.GetMyCardList());
            List<CardData> picked2 = new();
            picked2.AddRange(picked);

            for (int i = 0; i < picked2.Count; i++)
            {
                EquipmentCard equipCard = cardList.FindEquipmentCard(picked2[i]);
                if (picked2[i].EssentialEquip == EssentialEquip.Essential.ToString() && equipCard.IsEquipped)
                {
                    picked.Remove(picked2[i]);
                }
            }
        }

        if (picked == null) return;
        foreach (var item in picked)
        {
            slotsOnField.Add(item);
            cardSlotManager.SetSlotActive(item.ID, true);
        }
    }

    public void ClearSlots()
    {
        if (cardSlotManager == null) cardSlotManager = FindObjectOfType<CardSlotManager>();
        cardSlotManager.ClearPresentationField();
    }
    #endregion
}
