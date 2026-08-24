using UnityEngine;

public class HealPickUpObject : Collectable, IPickUpObject
{
    [Tooltip("Max Health에 대한 비율")]
    [Range(0, 100)]
    [SerializeField] public int HealAmount;

    public void OnPickUp(Character character)
    {
        character.GetComponent<Character>().Heal(HealAmount, true);
        // ⭐ 추가: 우유 메시지
        MessageSystem.instance.PostBuffMessage(LocalizationManager.Game.sweetMilk, MessageSystem.instance.GetBuffColor(FieldMessageType.Milk));
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Character character = collision.GetComponent<Character>();
        if (character != null)
        {
            OnPickUp(character);
            SoundManager.instance.Play(pickup);

            gameObject.SetActive(false);
        }
    }
}