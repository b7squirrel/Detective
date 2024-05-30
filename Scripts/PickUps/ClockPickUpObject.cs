using UnityEngine;

/// <summary>
/// 적들을 멈추게 하고, 스폰 이벤트도 일시 정지해야 함. 
/// 플레이어에 닿으면 사라져야 하므로 Field Item Effect에 행동을 맡김
/// </summary>
public class ClockPickUpObject : Collectable, IPickUpObject
{
    FieldItemEffect fieldItemEffect;
    public void OnPickUp(Character character)
    {
        if(fieldItemEffect == null) fieldItemEffect = FindObjectOfType<FieldItemEffect>();
        fieldItemEffect.StopEnemies();
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