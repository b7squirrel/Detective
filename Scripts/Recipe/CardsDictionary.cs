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
        Debug.Log("WeaponData = " + picked.Name);
        return picked;
    }

    public Item GetItemData(string grade, string Name)
    {
        List<Item> item = itemData.FindAll(x => x.Name == Name);
        Item picked = item.Find(x => x.grade.ToString() == grade);
        return picked;
    }

    // 단순히 카드 오브젝트를 생성해서 화면에 보여주기 위함
    public GameObject GenCard(string cardType, string newGrade, string name)
    {
        GameObject newCard = Instantiate(cardPrefab);

        if (cardType == (CardType.Weapon).ToString()) // weaponData로 무기 카드 초기화
        {
            WeaponData weaponData = GetWeaponData(newGrade, name);
            newCard.GetComponent<Card>().SetWeaponCardData(weaponData);
        }
        if (cardType == (CardType.Item).ToString()) // itemData로 아이템 카드 초기화
        {
            Item itemData = GetItemData(newGrade, name);
            newCard.GetComponent<Card>().SetItemCardData(itemData);
        }

        if (newCard == null)
        {
            Debug.Log("카드 타입이 정해지지 않았습니다");
        }

        return newCard;
    }
}
