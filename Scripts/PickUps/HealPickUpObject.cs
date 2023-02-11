using UnityEngine;

public class HealPickUpObject : Collectable, IPickUpObject
{
    [SerializeField] int healAmount;

    public void OnPickUp(Character character)
    {
        character.GetComponent<Character>().Heal(healAmount);
    }
}
