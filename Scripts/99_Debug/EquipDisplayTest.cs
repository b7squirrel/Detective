using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ItemListWrapper
{
    public List<Item> items = new List<Item>();
}

public class EquipDisplayTest : MonoBehaviour
{
    [Header("테스트 모드")]
    [SerializeField] bool isTesting;
    [SerializeField] GameObject Test;
    [SerializeField] GameObject BG;

    [Header("오리 종류")]
    [SerializeField] List<WeaponData> weapons; // 0등급 모든 무기
    [SerializeField] Animator charImage;
    RuntimeAnimatorController currentWeaponAnim;

    [Header("장비 종류")]
    [SerializeField] Item[] equipments = new Item[4];
    [SerializeField] List<ItemListWrapper> items; // 0등급 모든 부위별 아이템

    [Header("카드 이미지")]
    [SerializeField] Image[] equipmentImages;
    [SerializeField] RectTransform headMain;
    CardSpriteAnim cardSpriteAnim;
    bool needToOffset;

    void Awake()
    {
        if (isTesting == false)
        {
            Test.SetActive(false);
            BG.SetActive(false);
            return;
        }
        else
        {
            Test.SetActive(true);
            BG.SetActive(true);

            // InitSpriteRow();
            SetWeapon("Whistle");
        }
    }

    public void SetWeapon(string weapon)
    {
        WeaponData wd = weapons.Find(x => x.DisplayName == weapon);
        charImage.runtimeAnimatorController = wd.Animators.CardImageAnim;
    }
    public void SetItem(int index, string item) // 인덱스 부위의 아이템 리스트에서만 검색
    {
        Item i = items[index].items.Find(x => x.DisplayName == item);
        InitSpriteRow();
    }
    public void SetAnim(string anim)
    {
        charImage.SetTrigger(anim);
    }
    public List<string> GetWeapon()
    {
        List<string> weaponNames = new();
        foreach (var item in weapons)
        {
            weaponNames.Add(item.DisplayName.ToString());
        }
        return weaponNames;
    }

    public List<string> GetItems(int index) // 인덱스 부위의 아이템 리스트만 돌면서 항목 가져오기
    {
        List<string> itemNames = new();
        foreach (var item in items[index].items)
        {
            itemNames.Add(item.DisplayName.ToString()) ;
        }
        return itemNames;
    }


    #region Card Sprite Anim 참조
    public void InitSpriteRow()
    {
        if (cardSpriteAnim == null)
        {
            cardSpriteAnim = GetComponent<CardSpriteAnim>();
        }
        cardSpriteAnim.Init(equipmentImages);

        for (int i = 0; i < 4; i++)
        {
            SpriteRow spRow = equipments[i].spriteRow != null ? equipments[i].spriteRow : null;
            cardSpriteAnim.StoreItemSpriteRow(i, spRow);
            if (spRow == null)
            {
                equipmentImages[i].gameObject.SetActive(false);
            }
            else
            {
                equipmentImages[i].gameObject.SetActive(true);
            }
        }
    }
    public void SetEquipCardDisplay(int index, SpriteRow spriteRow, bool needToOffset, Vector2 offset)
    {
        // need to offset이 참이 되면 더 이상 변화가 없도록 남겨둠
        this.needToOffset = this.needToOffset ? true : needToOffset;

        // offset을 하게 하는 아이템이 탈착 되었을 때를 위한 초기화
        headMain.anchoredPosition = this.needToOffset == false ? Vector2.zero : headMain.anchoredPosition;

        // cardSpriteAnim.Init을 호출해서 해당 index 부위의 애니메이션 이미지들을 저장해 두기
        if (spriteRow == null)
        {
            equipmentImages[index].gameObject.SetActive(false);
        }
        else
        {
            equipmentImages[index].gameObject.SetActive(true);

            // 탈착, 장착이 반복될 수록 계속 offset이 더해지는 것을 막기 위해 원점이 아닐 때는 offset을 해주지 않기
            headMain.anchoredPosition = headMain.anchoredPosition == Vector2.zero ? headMain.anchoredPosition + offset : headMain.anchoredPosition;
            cardSpriteAnim.StoreItemSpriteRow(index, spriteRow); // 이미지들을 저장해 두고 애니메이션 이벤트로 사용
        }
    }
    #endregion
}
