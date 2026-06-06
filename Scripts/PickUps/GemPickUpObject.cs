using UnityEngine;

// 플레이어와 닿았을 때 PickUp 클래스에서 OnPickUp() 실행
public class GemPickUpObject : Collectable, IPickUpObject
{
    [field: SerializeField] public int ExpAmount { get; set; }

    public void OnPickUp(Character character)
    {
        if (ExpAmount == 0) ExpAmount = 2000;

        // ExpMultiplier 배율 적용 (기본 1배, 버프 시 2~4배)
        int finalExp = Mathf.RoundToInt(ExpAmount * FieldItemEffect.instance.ExpMultiplier);

        if (FieldItemEffect.instance.ExpMultiplier > 1f)
            Logger.Log($"[GemPickUp] 경험치 {FieldItemEffect.instance.ExpMultiplier}배 적용! {ExpAmount} → {finalExp}");

        character.level.AddExperience(finalExp);
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