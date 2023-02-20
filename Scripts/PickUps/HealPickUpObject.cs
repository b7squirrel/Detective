using UnityEngine;

public class HealPickUpObject : Collectable, IPickUpObject
{
    [field: SerializeField] public int HealAmount { get; set; }

    public void OnPickUp(Character character)
    {
        character.GetComponent<Character>().Heal(HealAmount);
    }
}
