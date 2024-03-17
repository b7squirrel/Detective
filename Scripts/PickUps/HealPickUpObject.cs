using UnityEngine;

public class HealPickUpObject : Collectable, IPickUpObject
{
    [field: SerializeField] public int HealAmount { get; set; }

    public void OnPickUp(Character character)
    {
        character.GetComponent<Character>().Heal(HealAmount, true);
    }

    // 알이나 우유 등은 일단 물리를 이용해서 충돌체크
    // 추후에 화면에 보이는 프랍들만 따로 관리해서 물리 없이 하자
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
