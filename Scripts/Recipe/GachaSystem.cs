using System.Collections.Generic;
using UnityEngine;

public class GachaSystem : MonoBehaviour
{
    CardDataManager cardDataManager;
    CardList cardList;

    [SerializeField] TextAsset gachaPoolDataBase;
    [SerializeField] TextAsset weaponPoolDatabase;
    [SerializeField] TextAsset itemPoolDatabase;
    List<CardData> gachaPools;
    List<CardData> weaponPools;
    List<CardData> itemPools;

    void Awake()
    {
        cardDataManager = GetComponent<CardDataManager>();
        cardList = GetComponent<CardList>();
    }

    void Draw(string _cardType)
    {
        if (gachaPools == null)
        {
            gachaPools = new();

            gachaPools = new ReadCardData().GetCardsList(gachaPoolDataBase);
        }
        if (weaponPools == null)
        {
            weaponPools = new();

            weaponPools = new ReadCardData().GetCardsList(weaponPoolDatabase);
        }
        if (itemPools == null)
        {
            itemPools = new();

            itemPools = new ReadCardData().GetCardsList(itemPoolDatabase);
        }

        CardData newCardData;

        if (_cardType == "Weapon")
        {
            int pickIndex = UnityEngine.Random.Range(0, weaponPools.Count);
            newCardData = weaponPools[pickIndex];
            cardDataManager.AddNewCardToMyCardsList(newCardData);
            AddEssentialEquip(newCardData);
            Debug.Log(newCardData.Name + newCardData.ID + " 을 뽑았습니다");
        }
        else if (_cardType == "Item")
        {
            int pickIndex = UnityEngine.Random.Range(0, itemPools.Count);
            newCardData = itemPools[pickIndex];
            cardDataManager.AddNewCardToMyCardsList(newCardData);
            Debug.Log(newCardData.Name + newCardData.ID + " 을 뽑았습니다");
        }

        gachaPools = null; // 생성된 카드 데이터가 가챠풀에 저장되어 버리므로
        weaponPools = null; // 생성된 카드 데이터가 가챠풀에 저장되어 버리므로
        itemPools = null; // 생성된 카드 데이터가 가챠풀에 저장되어 버리므로
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
        if (defaultItem == null) Debug.Log(_oriCardData.Name + "의 필수 무기가 NULL입니다");
        if (cardDataManager == null) Debug.Log("카드 데이터 메니져가 NULL");
        cardDataManager.AddNewCardToMyCardsList(defaultItem); // 기본 아이템을 생성
        cardList.Equip(_oriCardData, defaultItem);
    }

    // ??? 踰??
    public void DrawWeapons()
    {
        for (int i = 0; i < 5; i++)
        {
            Draw("Weapon");
        }
    }
    public void DrawWeaponsAboveGrade(int _grade)
    {
        for (int i = 0; i < 5; i++)
        {
            Draw("Weapon");
        }
    }
    public void DrawItems()
    {
        for (int i = 0; i < 5; i++)
        {
            Draw("Item");
        }
    }
    public void DrawCombo()
    {
        for (int i = 0; i < 5; i++)
        {
            Draw("Weapon");
        }
        for (int i = 0; i < 5; i++)
        {
            Draw("Item");
        }
    }
}
