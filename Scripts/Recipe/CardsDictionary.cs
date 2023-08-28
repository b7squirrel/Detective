using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 모든 weaponData, item 들을 모아놓고
// 이름과 등급으로 스크립터블 오브젝트를 찾아서 반환해 준다
// GatchaSystem 과 upgradeSlot 두 클래스가 접근해서 값을 얻어간다. 
public class CardsDictionary : MonoBehaviour
{
    [SerializeField] List<WeaponData> weaponData;
    [SerializeField] List<Item> itemData;
    [SerializeField] GameObject cardPrefab;

    public WeaponData GetWeaponData(string grade, string _name)
    {
        List<WeaponData> wd = weaponData.FindAll(x => x.Name == _name);
        WeaponData picked = wd.Find(x=>x.grade.ToString() == grade);
        return picked;
    }

    public Item GetItemData(string grade, string Name)
    {
        List<Item> item = itemData.FindAll(x => x.Name == Name);
        Item picked = item.Find(x => x.grade.ToString() == grade);
        return picked;
    }

    // 단순히 카드 오브젝트를 생성해서 화면에 보여주기 위함
    public GameObject GenCard(CardData newCardData)
    {
        string _cardType = newCardData.Type;
        string _newGrade = newCardData.Grade;
        string _name = newCardData.Name;

        GameObject newCard = Instantiate(cardPrefab);

        if (_cardType == (CardType.Weapon).ToString()) // weaponData로 무기 카드 초기화
        {
            WeaponData weaponData = GetWeaponData(_newGrade, _name);
            // 겹치는 요소들이 있지만 cardData의 ID를 위해 매개변수로 넘김
            newCard.GetComponent<Card>().SetWeaponCardData(weaponData, newCardData);
        }
        if (_cardType == (CardType.Item).ToString()) // itemData로 아이템 카드 초기화
        {
            Item itemData = GetItemData(_newGrade, _name);
            newCard.GetComponent<Card>().SetItemCardData(itemData, newCardData);
        }

        if (newCard == null)
        {
            Debug.Log("카드 타입이 정해지지 않았습니다");
        }

        return newCard;
    }
}
