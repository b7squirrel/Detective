using UnityEngine;

public class TennisBallProjectile : ProjectileBase
{
    [SerializeField] int deflection = 3;
    [SerializeField] AudioClip hitSound;

    Rigidbody2D rb;
    Animator anim;
    TrailRenderer trailRenderer;

    // ✅ 반사 직후 재충돌 방지용
    private bool isReflecting = false;
    private float reflectCooldown = 0f;
    private const float REFLECT_COOLDOWN_TIME = 0.05f; // 3프레임 정도

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        trailRenderer = GetComponent<TrailRenderer>();
    }

    protected override void Update()
    {
        // ✅ 쿨다운 타이머 업데이트
        if (isReflecting)
        {
            reflectCooldown -= Time.deltaTime;
            if (reflectCooldown <= 0f)
                isReflecting = false;
        }

        base.Update();
    }

    protected override void ApplyMovement()
    {
        // transform.position 대신 물리 속도로 이동
        rb.velocity = Direction.normalized * Speed;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        // ✅ 반사 쿨다운 중이면 무시
        if (isReflecting) return;

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
            other.gameObject.GetComponent<Idamageable>().TakeDamage(
                Damage, KnockBackChance, KnockBackSpeedFactor,
                transform.position, hitEffect);
            PostMessage(Damage, other.transform.position);
            if (!string.IsNullOrEmpty(WeaponName))
                DamageTracker.instance.RecordDamage(WeaponName, Damage);

            ReflectWithCooldown(normalVector); // ✅ 변경
            TriggerHitEffects();

            if (ShouldDeactivate(ref deflection))
                DeactivateBall();
        }
        else if (other.gameObject.CompareTag("MainCamera") ||
                 other.gameObject.CompareTag("Wall") ||
                 other.gameObject.CompareTag("Props"))
        {
            if (other.gameObject.CompareTag("Props"))
            {
                other.gameObject.GetComponent<Idamageable>()?.TakeDamage(
                    Damage, KnockBackChance, KnockBackSpeedFactor,
                    transform.position, hitEffect);
            }

            ReflectWithCooldown(normalVector); // ✅ 변경
            TriggerHitEffects();

            if (ShouldDeactivate(ref deflection))
                DeactivateBall();
        }
    }

    // ✅ 반사 + 즉시 콜라이더에서 탈출 + 쿨다운 시작
    private void ReflectWithCooldown(Vector2 normalVector)
    {
        HandleReflection(normalVector, rb);

        // 콜라이더 반지름보다 충분히 크게 밀어냄
        transform.position += (Vector3)(Direction.normalized * 0.6f);

        isReflecting = true;
        reflectCooldown = REFLECT_COOLDOWN_TIME;
    }

    private void DeactivateBall()
    {
        deflection = 3;
        isReflecting = false; // ✅ 리셋
        reflectCooldown = 0f;
        TimeToLive = 3f;
        transform.localScale = Vector3.one;
        gameObject.SetActive(false);
    }
    private void TriggerHitEffects()
    {
        if (anim != null)
            anim.SetTrigger("Hit");
        if (hitSound != null)
            SoundManager.instance.PlaySoundWith(hitSound, 1f, false, .034f);
    }
    private void OnDisable()
    {
        if (trailRenderer != null)
            trailRenderer.Clear();
    }

    protected override void CastDamage()
    {
        // TennisBall은 물리 충돌(OnCollisionEnter2D)로 데미지 처리
        // ProjectileBase의 OverlapCircle 방식 사용 안 함
    }
}