using System.Collections.Generic;
using UnityEngine;

public class ResultPanelUI : MonoBehaviour
{
    [Header("Pre-made Slots")]
    [SerializeField] GameObject[] cardSlots = new GameObject[6]; // 미리 만들어둔 6개의 슬롯
    [SerializeField] Animator[] anims = new Animator[6]; // 미리 만들어둔 6개의 슬롯

    public void ShowResults(string animTrigger)
    {
        ClearAllSlots();

        // 모든 무기와 아이템 데이터 수집
        List<SlotData> allSlotData = CollectAllSlotData();

        // 슬롯에 데이터 할당
        AssignDataToSlots(allSlotData);

        SetTrigger(animTrigger);
        SetSlotVisibility(allSlotData.Count, animTrigger);
        RotateWeapons(animTrigger);
    }

    void SetTrigger(string animTrigger)
    {
        for (int i = 0; i < cardSlots.Length; i++)
        {
            anims[i].SetTrigger(animTrigger);
        }
    }

    void SetSlotVisibility(int slotSum, string animTrigger)
    {
        for (int i = 0; i < cardSlots.Length; i++)
        {
            if(i >= slotSum || animTrigger == "Hit" && i != 0)
            {
                cardSlots[i].SetActive(false);
            }
            else
            {
                cardSlots[i].SetActive(true);
            }
        }
    }

    void RotateWeapons(string animTrigger)
    {
        if (animTrigger == "Hit")
        {
            for (int i = 0; i < cardSlots.Length; i++)
            {
                cardSlots[i].transform.localRotation = Quaternion.Euler(0f, 0f, 30f);
            }
        }
    }

    /// <summary>
    /// 모든 무기와 아이템 데이터를 수집
    /// </summary>
    List<SlotData> CollectAllSlotData()
    {
        List<SlotData> slotDataList = new List<SlotData>();

        // 무기 데이터 수집
        WeaponManager weaponManager = GameManager.instance.character.GetComponent<WeaponManager>();
        if (weaponManager != null)
        {
            List<WeaponBase> allWeapons = weaponManager.GetAllWeapons();

            for (int i = 0; i < allWeapons.Count; i++)
            {
                WeaponBase wb = allWeapons[i];
                slotDataList.Add(new SlotData
                {
                    isWeapon = true,
                    weaponData = wb.weaponData,
                    weaponBase = wb,
                    isLead = (i == 0)
                });
            }
        }

        // // 아이템 데이터 수집
        // PassiveItems passiveItems = GameManager.instance.character.GetComponent<PassiveItems>();
        // if (passiveItems != null)
        // {
        //     List<Item> allItems = passiveItems.GetAllItems();

        //     foreach (Item item in allItems)
        //     {
        //         slotDataList.Add(new SlotData
        //         {
        //             isWeapon = false,
        //             itemData = item
        //         });
        //     }
        // }

        return slotDataList;
    }

    /// <summary>
    /// 수집한 데이터를 슬롯에 할당
    /// </summary>
    void AssignDataToSlots(List<SlotData> slotDataList)
    {
        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (cardSlots[i] == null) continue;

            // 데이터가 있으면 표시, 없으면 비워둠
            if (i < slotDataList.Count)
            {
                SlotData data = slotDataList[i];

                if (data.isWeapon)
                {
                    SetupWeaponSlot(cardSlots[i], data);
                }
                else
                {
                    SetupItemSlot(cardSlots[i], data);
                }

                cardSlots[i].SetActive(true);
            }
            else
            {
                // 데이터가 없는 슬롯은 비활성화하거나 비워둠
                EmptySlot(cardSlots[i]);
            }
        }
    }

    /// <summary>
    /// 무기 슬롯 설정
    /// </summary>
    void SetupWeaponSlot(GameObject slot, SlotData data)
    {
        CardDisp cardDisp = slot.GetComponent<CardDisp>();
        // PauseCardDisp pauseDisp = slot.GetComponent<PauseCardDisp>();

        // if (pauseDisp != null)
        // {
        //     pauseDisp.EnableLeadTag(data.isLead);
        //     pauseDisp.InitWeaponCardDisplay(data.weaponData);
        // }

        SetWeaponEquipSprites(cardDisp, data.weaponData, data.isLead);

        // if (pauseDisp != null)
        // {
        //     pauseDisp.UpdatePauseCardLevel(data.weaponBase.weaponStats.currentLevel, true, false);
        // }
    }

    /// <summary>
    /// 아이템 슬롯 설정
    /// </summary>
    void SetupItemSlot(GameObject slot, SlotData data)
    {
        CardDisp cardDisp = slot.GetComponent<CardDisp>();
        // PauseCardDisp pauseCardDisp = slot.GetComponent<PauseCardDisp>();

        // if (pauseCardDisp != null)
        // {
        //     pauseCardDisp.InitItemCardDisplay(data.itemData);
        // }

        if (cardDisp != null)
        {
            cardDisp.InitItemCardDisplay(data.itemData, null, false);
        }

        // if (pauseCardDisp != null)
        // {
        //     pauseCardDisp.UpdatePauseCardLevel(data.itemData.stats.currentLevel, false, false);
        // }
    }

    /// <summary>
    /// 무기의 장비 스프라이트 설정
    /// </summary>
    void SetWeaponEquipSprites(CardDisp cardDisp, WeaponData wd, bool isLead)
    {
        if (cardDisp == null) return;

        cardDisp.InitWeaponCardDisplay(wd, null);
        cardDisp.InitSpriteRow();

        for (int i = 0; i < 4; i++)
        {
            Item item = isLead ?
                GameManager.instance.startingDataContainer.GetItemDatas()[i] :
                wd.defaultItems[i];

            if (item == null)
            {
                cardDisp.SetEquipCardDisplay(i, null, false, Vector2.zero);
                continue;
            }

            Vector2 offset = item.needToOffset ? item.posHead : Vector2.zero;
            cardDisp.SetEquipCardDisplay(i, item.spriteRow, item.needToOffset, offset);
        }
    }

    /// <summary>
    /// 슬롯 비우기 (데이터가 없는 경우)
    /// </summary>
    void EmptySlot(GameObject slot)
    {
        CardDisp cardDisp = slot.GetComponent<CardDisp>();
        if (cardDisp != null)
        {
            cardDisp.EmptyCardDisplay();
        }

        // 옵션 1: 슬롯을 비활성화
        slot.SetActive(false);

        // 옵션 2: 슬롯은 보이되 내용만 비우기 (위 줄 주석 처리하고 이 방식 사용)
        // slot.SetActive(true);
    }

    /// <summary>
    /// 모든 슬롯 초기화
    /// </summary>
    void ClearAllSlots()
    {
        foreach (GameObject slot in cardSlots)
        {
            if (slot != null)
            {
                EmptySlot(slot);
            }
        }
    }

    /// <summary>
    /// 슬롯 데이터를 담는 헬퍼 클래스
    /// </summary>
    class SlotData
    {
        public bool isWeapon;
        public WeaponData weaponData;
        public WeaponBase weaponBase;
        public Item itemData;
        public bool isLead;
    }
}