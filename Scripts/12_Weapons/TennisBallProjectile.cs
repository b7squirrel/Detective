// TennisBallProjectile.cs
using UnityEngine;

public class TennisBallProjectile : ProjectileBase
{
    [SerializeField] int deflection = 3;
    [SerializeField] AudioClip hitSound;

    Rigidbody2D rb;
    Animator anim;
    TrailRenderer trailRenderer;

    private bool isReflecting = false;
    private float reflectCooldown = 0f;
    private const float REFLECT_COOLDOWN_TIME = 0.05f;

    // ✅ 외부(TennisWeapon)에서 발사 시 deflection 초기화용
    public void SetDeflection(int value)
    {
        deflection = value;
        isReflecting = false;
        reflectCooldown = 0f;
    }

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        trailRenderer = GetComponent<TrailRenderer>();
    }

    protected override void Update()
    {
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
        rb.velocity = Direction.normalized * Speed;
    }

    // ✅ Is Trigger = false인 일반 콜라이더 (벽, 적 등)
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (isReflecting) return;

        GameObject hitEffect = hitEffects != null ? hitEffects.hitEffect : null;

        if (other.contacts.Length > 0)
        {
            Vector2 normalVector = other.contacts[0].normal;
            HandleCollisionWithNormal(other.gameObject, normalVector, hitEffect);
        }
    }

    // ✅ Is Trigger = true인 콜라이더 (Chest 등 Props)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isReflecting) return;

        if (!other.gameObject.CompareTag("Props")) return;

        GameObject hitEffect = hitEffects != null ? hitEffects.hitEffect : null;

        // Trigger는 contact point가 없으므로 법선 벡터 근사 계산
        Vector2 closestPoint = other.bounds.ClosestPoint(transform.position);
        Vector2 diff = (Vector2)transform.position - closestPoint;
        Vector2 normalVector = diff.magnitude > 0.001f ? diff.normalized : Vector2.up;

        other.gameObject.GetComponent<Idamageable>()?.TakeDamage(
            Damage, KnockBackChance, KnockBackSpeedFactor,
            transform.position, hitEffect);

        PostMessage(Damage, other.transform.position);

        if (!string.IsNullOrEmpty(WeaponName))
            DamageTracker.instance.RecordDamage(WeaponName, Damage);

        ReflectWithCooldown(normalVector);
        TriggerHitEffects();

        if (ShouldDeactivate(ref deflection))
            DeactivateBall();
    }

    private void HandleCollisionWithNormal(GameObject hitObject, Vector2 normalVector, GameObject hitEffect)
    {
        if (hitObject.CompareTag("Enemy"))
        {
            hitObject.GetComponent<Idamageable>().TakeDamage(
                Damage, KnockBackChance, KnockBackSpeedFactor,
                transform.position, hitEffect);
            PostMessage(Damage, hitObject.transform.position);

            if (!string.IsNullOrEmpty(WeaponName))
                DamageTracker.instance.RecordDamage(WeaponName, Damage);

            ReflectWithCooldown(normalVector);
            TriggerHitEffects();

            if (ShouldDeactivate(ref deflection))
                DeactivateBall();
        }
        else if (hitObject.CompareTag("MainCamera") ||
                 hitObject.CompareTag("Wall") ||
                 hitObject.CompareTag("Props"))
        {
            if (hitObject.CompareTag("Props"))
            {
                hitObject.GetComponent<Idamageable>()?.TakeDamage(
                    Damage, KnockBackChance, KnockBackSpeedFactor,
                    transform.position, hitEffect);
            }

            ReflectWithCooldown(normalVector);
            TriggerHitEffects();

            if (ShouldDeactivate(ref deflection))
                DeactivateBall();
        }
    }

    private void ReflectWithCooldown(Vector2 normalVector)
    {
        HandleReflection(normalVector, rb);
        transform.position += (Vector3)(Direction.normalized * 0.6f);
        isReflecting = true;
        reflectCooldown = REFLECT_COOLDOWN_TIME;
    }

    private void DeactivateBall()
    {
        deflection = 3;
        isReflecting = false;
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
        // 물리 충돌 방식 사용, OverlapCircle 방식 사용 안 함
    }
}