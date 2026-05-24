using UnityEngine;

public class ProjectileBase : MonoBehaviour
{
    [field: SerializeField] public Vector3 Direction { get; set; }
    [field: SerializeField] public float Speed { get; set; }
    protected bool hitDetected = false;
    public int Damage { get; set; } = 5;
    public float KnockBackChance { get; set; }
    [field: SerializeField] public float KnockBackSpeedFactor { get; set; }
    public bool IsCriticalDamageProj { get; set; }
    [field: SerializeField] public float TimeToLive { get; set; } = 3f;
    public string WeaponName { get; set; }

    // ✅ 캐싱: Awake에서 한 번만 GetComponent
    protected HitEffects hitEffects;

    // ✅ NonAlloc용 버퍼: 모든 발사체가 공유 (static)
    // 발사체는 메인 스레드에서만 실행되므로 static 공유 안전
    protected static readonly Collider2D[] overlapBuffer = new Collider2D[10];

    protected virtual void Awake()
    {
        hitEffects = GetComponent<HitEffects>();
    }

    protected virtual void Update()
    {
        if (Time.timeScale == 0) return;
        ApplyMovement();
        CastDamage();
        AttackCoolTimer();
    }

    protected virtual void AttackCoolTimer()
    {
        TimeToLive -= Time.deltaTime;
        if (TimeToLive < 0f)
        {
            DieProjectile();
        }
    }

    protected virtual void ApplyMovement()
    {
        transform.position += Speed * Time.deltaTime * Direction.normalized;
    }

    protected virtual void CastDamage()
    {
        // ✅ NonAlloc: 배열 재사용으로 GC 제거
        int count = Physics2D.OverlapCircleNonAlloc(transform.position, .7f, overlapBuffer);

        for (int i = 0; i < count; i++)
        {
            // ✅ GetComponent 횟수 최소화
            Idamageable damageable = overlapBuffer[i].GetComponent<Idamageable>();
            if (damageable == null) continue;

            if (overlapBuffer[i].GetComponent<DestructableObject>() == null)
                PostMessage(Damage, overlapBuffer[i].transform.position);

            // ✅ 캐싱된 hitEffects 사용
            GameObject hitEffect = hitEffects != null ? hitEffects.hitEffect : null;
            damageable.TakeDamage(
                Damage, KnockBackChance, KnockBackSpeedFactor,
                transform.position, hitEffect);

            hitDetected = true;

            if (!string.IsNullOrEmpty(WeaponName))
                DamageTracker.instance.RecordDamage(WeaponName, Damage);

            break;
        }

        if (hitDetected)
        {
            HitObject();
            hitDetected = false;
        }
    }

    protected virtual void HitObject()
    {
        TimeToLive = 3f;
        transform.localScale = Vector3.one;
        gameObject.SetActive(false);
    }

    protected virtual void PostMessage(int damage, Vector3 targetPosition)
    {
        MessageSystem.instance.PostMessage(
            damage.ToString(), targetPosition, IsCriticalDamageProj);
    }

    protected virtual void DieProjectile()
    {
        TimeToLive = 3f;
        transform.localScale = Vector3.one;
        gameObject.SetActive(false);
    }

    #region 반사 로직
    // ProjectileBase
    protected void HandleReflection(Vector2 normalVector, Rigidbody2D rb)
    {
        Vector2 incomingVector = Direction.normalized;
        Vector2 deflectionVector = Vector2.Reflect(incomingVector, normalVector).normalized;
        Direction = deflectionVector;
        rb.velocity = deflectionVector * Speed; // ✅ 반사 방향으로 즉시 속도 적용
    }

    protected bool ShouldDeactivate(ref int deflection)
    {
        deflection--;
        return deflection < 0;
    }
    #endregion
}