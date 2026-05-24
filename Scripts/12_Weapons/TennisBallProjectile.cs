using UnityEngine;

public class TennisBallProjectile : ProjectileBase
{
    [SerializeField] int deflection = 3;
    [SerializeField] AudioClip hitSound;

    Rigidbody2D rb;
    Animator anim;
    TrailRenderer trailRenderer;

    protected override void Awake()
    {
        base.Awake(); // ✅ ProjectileBase.Awake() 호출 → hitEffects 캐싱

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        trailRenderer = GetComponent<TrailRenderer>();
    }

    private void OnDisable()
    {
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        // ✅ 캐싱된 hitEffects 사용 (매 충돌마다 GetComponent 제거)
        GameObject hitEffect = hitEffects != null ? hitEffects.hitEffect : null;

        if (other.contacts.Length > 0)
        {
            Vector2 normalVector = other.contacts[0].normal;
            HandleCollisionWithNormal(other, normalVector, hitEffect);
        }
    }

    private void HandleCollisionWithNormal(Collision2D other, Vector2 normalVector, GameObject hitEffect)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            // 데미지 처리
            other.gameObject.GetComponent<Idamageable>().TakeDamage(
                Damage, KnockBackChance, KnockBackSpeedFactor,
                transform.position, hitEffect);

            PostMessage(Damage, other.transform.position);

            if (!string.IsNullOrEmpty(WeaponName))
                DamageTracker.instance.RecordDamage(WeaponName, Damage);

            HandleReflection(normalVector, rb);
            TriggerHitEffects();

            if (ShouldDeactivate(ref deflection))
                DeactivateBall();
        }
        else if (other.gameObject.CompareTag("MainCamera") ||
                 other.gameObject.CompareTag("Wall") ||
                 other.gameObject.CompareTag("Props"))
        {
            // 소품이면 데미지 처리
            if (other.gameObject.CompareTag("Props"))
            {
                other.gameObject.GetComponent<Idamageable>()?.TakeDamage(
                    Damage, KnockBackChance, KnockBackSpeedFactor,
                    transform.position, hitEffect);
            }

            HandleReflection(normalVector, rb);
            TriggerHitEffects();

            if (ShouldDeactivate(ref deflection))
                DeactivateBall();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // ✅ 캐싱된 hitEffects 사용
        GameObject hitEffect = hitEffects != null ? hitEffects.hitEffect : null;

        if (other.gameObject.CompareTag("Props"))
        {
            other.gameObject.GetComponent<Idamageable>()?.TakeDamage(
                Damage, KnockBackChance, KnockBackSpeedFactor,
                transform.position, hitEffect);

            // 트리거는 단순 반대 방향 반사
            Direction = -Direction.normalized;
            rb.velocity = Vector2.zero;
            TriggerHitEffects();

            if (ShouldDeactivate(ref deflection))
                DeactivateBall();
        }
    }

    private void TriggerHitEffects()
    {
        if (anim != null)
            anim.SetTrigger("Hit");

        if (hitSound != null)
            SoundManager.instance.PlaySoundWith(hitSound, 1f, false, .034f);
    }

    private void DeactivateBall()
    {
        deflection = 3; // 초기값으로 리셋
        TimeToLive = 3f;
        transform.localScale = Vector3.one;
        gameObject.SetActive(false);
    }

    protected override void CastDamage()
    {
        // TennisBall은 물리 충돌(OnCollisionEnter2D)로 데미지 처리
        // ProjectileBase의 OverlapCircle 방식 사용 안 함
    }
}