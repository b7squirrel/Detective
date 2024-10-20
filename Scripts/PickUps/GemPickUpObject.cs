using UnityEngine;

// 플레이어와 닿았을 때 PickUP 클래스에서 OnPickUp() 실행
public class GemPickUpObject : Collectable, IPickUpObject
{
    [field: SerializeField] public int ExpAmount { get; set; }
    public void OnPickUp(Character character)
    {
        
        if (ExpAmount == 0) ExpAmount = 2000;
        
        character.level.AddExperience(ExpAmount);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Character character = collision.GetComponent<Character>();
        if (character != null)
        {
            OnPickUp(character);

            SoundManager.instance.PlaySoundWith(pickup, 1f, false, .034f);
            gameObject.SetActive(false);
        }
    }
}
