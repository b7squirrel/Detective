using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// 판매 패널 전용 카드 슬롯
/// CardDisp로 카드 비주얼을 표시하고,
/// 선택/해제 상태와 장착중 오버레이를 관리합니다.
/// </summary>
public class SellCardSlot : MonoBehaviour
{
    [Header("카드 표시")]
    [SerializeField] CardDisp cardDisp;

    [Header("선택 오버레이")]
    [SerializeField] GameObject checkOverlay;       // 체크 원 이미지 (인스펙터에서 스프라이트 연결)

    [Header("장착중 오버레이")]
    [SerializeField] GameObject equippedLockOverlay; // 반투명 잠금 이미지

    [Header("필수 장비 오버레이")]
    [SerializeField] GameObject essentialOverlay; // 필수 장비 표시 이미지

    [Header("리드 오리 오버레이")]
    [SerializeField] GameObject leadOverlay; // "리드" 또는 왕관 이미지

    [Header("판매가 표시")]
    [SerializeField] TextMeshProUGUI sellPriceText;  // 선택 시 판매가 표시

    // ───── 상태 ─────
    CardData cardData;
    bool isSelected = false;
    bool isEquipped = false;
    int sellPrice = 0;

    // SellPanelManager가 등록할 콜백
    // 인자: (슬롯, 선택여부)
    Action<SellCardSlot, bool> onToggleCallback;

    // ───── 초기화 ─────

    /// <summary>
    /// 오리(Weapon) 카드 세팅
    /// </summary>
    public void SetupAsDuck(CardData data, WeaponData weaponData,
    int price, bool equipped, Action<SellCardSlot, bool> callback)
    {
        cardData = data;
        sellPrice = price;
        isEquipped = equipped;
        isSelected = false;
        onToggleCallback = callback;

        cardDisp.InitWeaponCardDisplay(weaponData, data);

        // ⭐ 오리 카드는 필수 오버레이 항상 숨기기
        if (essentialOverlay != null)
            essentialOverlay.SetActive(false);

        // ⭐ 리드 오리는 버튼 비활성화 + 리드 오버레이 표시
        bool isLead = data.StartingMember == StartingMember.Zero.ToString();
        GetComponentInChildren<Button>().interactable = !isLead;
        if (leadOverlay != null)
            leadOverlay.SetActive(isLead);

        RefreshOverlays();
        UpdatePriceText();
    }

    /// <summary>
    /// 오리 카드에 장착된 장비 스프라이트 표시
    /// SetupAsDuck() 호출 후 SellPanelManager에서 호출
    /// </summary>
    public void ApplyEquipSprites(EquipmentCard[] equipCards, CardsDictionary cardsDictionary)
    {
        IEquipSpriteAnim equipSpriteAnim = cardDisp as IEquipSpriteAnim;
        if (equipSpriteAnim == null) return;

        equipSpriteAnim.InitSpriteRow();

        for (int i = 0; i < 4; i++)
        {
            if (equipCards[i] == null)
            {
                equipSpriteAnim.SetEquipCardDisplay(i, null, false, Vector2.zero);
                continue;
            }

            CardData equipCardData = equipCards[i].CardData;
            WeaponItemData weaponItemData = cardsDictionary.GetWeaponItemData(equipCardData);
            if (weaponItemData?.itemData == null) continue;

            Item item = weaponItemData.itemData;
            Vector2 offset = item.needToOffset ? item.posHead : Vector2.zero;
            equipSpriteAnim.SetEquipCardDisplay(i, item.spriteRow, item.needToOffset, offset);
        }
    }

    /// <summary>
    /// 아이템(Item) 카드 세팅
    /// </summary>
    public void SetupAsItem(CardData data, Item itemData,
    int price, bool equipped, Action<SellCardSlot, bool> callback)
    {
        cardData = data;
        sellPrice = price;
        isEquipped = equipped;
        isSelected = false;
        onToggleCallback = callback;

        cardDisp.InitItemCardDisplay(itemData, data, equipped);

        // ⭐ 아이템 카드만 필수 오버레이 표시
        if (essentialOverlay != null)
            essentialOverlay.SetActive(data.BindingTo != "All");

        // ⭐ 리드 오버레이는 아이템에서 항상 숨기기
        if (leadOverlay != null)
            leadOverlay.SetActive(false);

        RefreshOverlays();
        UpdatePriceText();
    }

    // ───── 버튼 클릭 ─────

    /// <summary>
    /// Button의 OnClick에 연결
    /// </summary>
    public void OnClick()
    {
        isSelected = !isSelected;
        RefreshOverlays();
        onToggleCallback?.Invoke(this, isSelected);
    }

    // ───── 외부에서 강제 해제 (팝업 취소 시 등) ─────
    public void Deselect()
    {
        isSelected = false;
        RefreshOverlays();
    }

    // ───── UI 갱신 ─────

    void RefreshOverlays()
    {
        if (checkOverlay != null)
            checkOverlay.SetActive(isSelected);

        if (equippedLockOverlay != null)
            equippedLockOverlay.SetActive(isEquipped);
    }

    void UpdatePriceText()
    {
        if (sellPriceText == null) return;
        sellPriceText.text = sellPrice.ToString("N0"); // 천 단위 콤마
    }

    // ───── Getter ─────
    public CardData GetCardData()  => cardData;
    public int GetSellPrice()      => sellPrice;
    public bool IsSelected()       => isSelected;
    public bool IsEquipped()       => isEquipped;
}