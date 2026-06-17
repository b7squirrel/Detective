using UnityEngine;

public class BaseballProjectile : ProjectileBase
{
    [SerializeField] AudioClip hitSound;
    [SerializeField] GameObject deadProjectile;
    [SerializeField] float deadVerticalSpeed;
    [SerializeField] SpriteRenderer deadSprite;
    [SerializeField] Sprite deadSpriteImage;

    [Header("Rotation Settings")]
    [SerializeField] bool rotateToDirection = true; // 회전 활성화 여부
    [SerializeField] float rotationOffset = 0f;     // 각도 보정값 (필요시)

    Animator anim;
    TrailRenderer trailRenderer;
    bool hasHitOnce = false;

    protected override void Awake()
    {
        base.Awake(); // ✅ ProjectileBase.Awake() 호출 → hitEffects 캐싱

        anim = GetComponent<Animator>();
        trailRenderer = GetComponent<TrailRenderer>();
    }

    private void OnEnable()
    {
        hasHitOnce = false;
    }

    private void OnDisable()
    {
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
        }
    }

    protected override void Update()
    {
        base.Update();

        if (rotateToDirection && !hasHitOnce)
        {
            RotateToDirection();
        }
    }

    void RotateToDirection()
    {
        if (Direction != Vector3.zero && deadSprite != null)
        {
            float angle = Mathf.Atan2(Direction.y, Direction.x) * Mathf.Rad2Deg;

            // 스프라이트만 회전
            deadSprite.transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHitOnce) return;

        // 유효한 충돌 대상인지 먼저 확인
        // 슬라임드롭을 먼저 통과하면 hasHitOnce인 상태로 적에게 도달해서 그냥 통과하므로
        bool validHit = other.CompareTag("Enemy") ||
                        other.CompareTag("Props") ||
                        other.CompareTag("Wall") ||
                        other.CompareTag("MainCamera");

        if (!validHit) return; // Untagged인 SlimeDrop은 여기서 걸러짐

        hasHitOnce = true;

        // ✅ 캐싱된 hitEffects 사용 (매 충돌마다 GetComponent 제거)
        GameObject hitEffect = hitEffects != null ? hitEffects.hitEffect : null;

        Vector2 hitPosition = transform.position;
        Vector2 otherPosition = other.transform.position;
        Vector2 normalVector = (hitPosition - otherPosition).normalized;

        if (deadSpriteImage == null) deadSpriteImage = deadSprite.sprite;
        GenDeadProjectile(normalVector * Speed, deadVerticalSpeed, deadSpriteImage);

        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<Idamageable>()?.TakeDamage(
                Damage, KnockBackChance, KnockBackSpeedFactor,
                transform.position, hitEffect);

            PostMessage(Damage, other.transform.position);

            // 적에게의 공격에만 데미지 기록
            if (!string.IsNullOrEmpty(WeaponName))
                DamageTracker.instance.RecordDamage(WeaponName, Damage);

            TriggerHitEffects(transform.position);
            gameObject.SetActive(false);
        }
        else if (other.CompareTag("Props"))
        {
            other.GetComponent<Idamageable>()?.TakeDamage(
                Damage, KnockBackChance, KnockBackSpeedFactor,
                transform.position, hitEffect);

            TriggerHitEffects(transform.position);
            gameObject.SetActive(false);
        }
        else if (other.CompareTag("Wall") || other.CompareTag("MainCamera"))
        {
            TriggerHitEffects(transform.position);
            gameObject.SetActive(false);
        }
    }

    private void TriggerHitEffects(Vector2 position)
    {
        if (anim != null)
            anim.SetTrigger("Hit");

        if (hitSound != null)
            SoundManager.instance.Play(hitSound);

        if (anim != null)
            anim.SetTrigger("Hit");

        if (hitSound != null)
            SoundManager.instance.Play(hitSound);

        // 이펙트 스폰 추가
        if (hitEffects != null && hitEffects.hitEffect != null)
        {
            GameObject fx = GameManager.instance.poolManager.GetMisc(hitEffects.hitEffect);
            fx.transform.position = position;
        }
    }

    protected override void CastDamage()
    {
        // 충돌은 OnTriggerEnter2D에서 처리하므로 여기서는 아무것도 하지 않음
    }

    void GenDeadProjectile(Vector2 groundVel, float verticalVelocity, Sprite sprite)
    {
        GameObject deadPr = GameManager.instance.poolManager.GetMisc(deadProjectile);
        deadPr.transform.position = transform.position;
        deadPr.GetComponent<ShadowHeightDeadProjectile>()
            .Initialize(groundVel / 8f, verticalVelocity, sprite);
    }
}