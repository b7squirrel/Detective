using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GachaSystem : MonoBehaviour
{
    CardDataManager cardDataManager;
    CardList cardList;

    [SerializeField] TextAsset gachaPoolDataBase;
    List<CardData> gachaPools;

    void Awake()
    {
        cardDataManager = GetComponent<CardDataManager>();
        cardList = GetComponent<CardList>();
    }

    // shop 패널의 뽑기 버튼에서 호출
    public void Draw()
    {
        if (gachaPools == null)
        {
            gachaPools = new();

            gachaPools = new ReadCardData().GetCardsList(gachaPoolDataBase);
        }

        // 임시로 3개씩 카드가 뽑힘
        for (int i = 0; i < 1; i++)
        {
            int pickIndex = UnityEngine.Random.Range(0, gachaPools.Count);
            //string mType = gachaPools[pickIndex].Type;
            //string mGrade = gachaPools[pickIndex].Grade;
            //string mName = gachaPools[pickIndex].Name;
            //CardData oriData = gachaPools.Find(x => x.Name == "Cowboy");

            CardData oriData = gachaPools[pickIndex];
            cardDataManager.AddNewCardToMyCardsList(oriData);
            Debug.Log(oriData.Name + oriData.ID + " 을 뽑았습니다.");

            if (oriData.Type == "Weapon")
            {
                AddEssentialEquip(oriData);
            }

            gachaPools = null; // 생성된 카드 데이터가 가챠풀에 저장되어 버리므로
        }
    }
    public void AddEssentialEquip(CardData _oriCardData)
    {
        if (gachaPools == null)
        {
            gachaPools = new();

            gachaPools = new ReadCardData().GetCardsList(gachaPoolDataBase);
        }
        List<CardData> sameItems = gachaPools.FindAll(x => x.BindingTo == _oriCardData.Name);

        CardData defaultItem = sameItems.Find(x => x.DefaultItem == DefaultItem.Default.ToString());

        Debug.Log("Default Item = " + defaultItem.Name + " Grade = " + defaultItem.Grade);
        if (defaultItem == null) Debug.Log(_oriCardData.Name + "의 필수 무기가 NULL입니다.");
        if (cardDataManager == null) Debug.Log("카드 데이터 메니져가 NULL");
        cardDataManager.AddNewCardToMyCardsList(defaultItem); // 기본 아이템을 생성
        cardList.Equip(_oriCardData, defaultItem);
    }
}
