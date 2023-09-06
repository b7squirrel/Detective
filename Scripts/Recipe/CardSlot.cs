public interface CardSlot
{
    public bool IsEmpty();
    public CardData GetCardData();
    public void SetWeaponCard(CardData _cardData, WeaponData _weaponData);
    public void SetItemCard(CardData _cardData, Item _itemData);
    public void EmptySlot();
}
