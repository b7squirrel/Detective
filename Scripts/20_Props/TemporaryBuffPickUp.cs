using UnityEngine;

/// <summary>
/// 필드에 드롭되는 임시 버프 아이템의 공통 스크립트.
/// Inspector에서 buffType과 수치만 바꾸면 4가지 버프 아이템 모두 처리.
///
/// [프리팹 구성]
/// - Collider2D (Is Trigger 체크)
/// - TriggerHeightShadow
/// - ShadowHeight
/// - TemporaryBuffPickUp (이 스크립트)
/// </summary>
public class TemporaryBuffPickUp : Collectable, IPickUpObject
{
    [Header("버프 설정")]
    [SerializeField] FieldBuffType buffType;
    [SerializeField] float duration = 5f;

    [Tooltip(
        "SpeedBoost: 증가할 MoveSpeed 수치 (예: 2.0)\n" +
        "DamageBoost: 증가할 DamageBonus 수치 (예: 100)\n" +
        "DoubleExp / DoubleCoin: 사용 안 함 (배율은 자동으로 +1씩 중첩)"
    )]
    [SerializeField] float buffValue;

    /// <summary>
    /// ChestDrop에서 최대 배율 체크 시 사용
    /// </summary>
    public FieldBuffType GetBuffType() => buffType;

    public void OnPickUp(Character character)
    {
        FieldItemEffect.instance.ApplyBuff(buffType, duration, buffValue);
        Logger.Log($"[TemporaryBuffPickUp] {buffType} 버프 획득! {duration}초, 수치: {buffValue}");
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