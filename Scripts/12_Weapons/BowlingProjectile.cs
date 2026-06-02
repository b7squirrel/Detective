using UnityEngine;

public class BowlingProjectile : ProjectileBase
{
    [SerializeField] private int maxReflections = 5;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip reflectSound;
    [SerializeField] private AudioClip bowlingStrikeSound;
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private Transform spriteTransform;

    private Rigidbody2D rb;
    private Animator anim;
    private TrailRenderer trailRenderer;
    private int currentReflections = 0;

    // 같은 프레임 중복 반사 방지
    private bool isReflectingThisFrame = false;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        trailRenderer = GetComponent<TrailRenderer>();
    }

    private void OnEnable()
    {
        currentReflections = 0;
        isReflectingThisFrame = false;

        // rb.velocity로 초기 이동 설정
        // if (rb != null)
        //     rb.velocity = Direction.normalized * Speed;
        // ✅ rb.velocity 초기화는 여기서 하지 않음
    }
    // ✅ 외부에서 Direction/Speed 할당 후 명시적으로 호출
    public void Launch()
    {
        if (rb != null)
            rb.velocity = Direction.normalized * Speed;
    }

    private void OnDisable()
    {
        if (trailRenderer != null)
            trailRenderer.Clear();
    }

    // ✅ transform 직접 이동 비활성화 (rb.velocity가 담당)
    protected override void ApplyMovement()
    {
        // 아무것도 하지 않음
    }

    protected override void Update()
    {
        base.Update();

        // 스프라이트 회전
        if (spriteTransform != null && Mathf.Abs(Direction.x) > 0.01f)
        {
            float rotationDirection = -Mathf.Sign(Direction.x);
            spriteTransform.Rotate(0, 0, rotationDirection * rotationSpeed * Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        // Direction을 실제 velocity와 동기화 (회전 계산 등에 사용)
        if (rb != null && rb.velocity.sqrMagnitude > 0.01f)
            Direction = rb.velocity.normalized;

        // 프레임 중복 반사 플래그 초기화
        isReflectingThisFrame = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject hitEffect = hitEffects != null ? hitEffects.hitEffect : null;

        if (other.CompareTag("Enemy") || other.CompareTag("Props"))
        {
            HandleDamage(other, hitEffect);
        }
        else if (other.CompareTag("MainCamera") || other.CompareTag("Wall"))
        {
            HandleWallReflection(other);
        }
    }

    private void HandleDamage(Collider2D target, GameObject hitEffect)
    {
        Idamageable damageable = target.GetComponent<Idamageable>();
        if (damageable == null) return;

        damageable.TakeDamage(
            Damage, KnockBackChance, KnockBackSpeedFactor,
            transform.position, hitEffect);

        if (target.CompareTag("Enemy"))
            PostMessage(Damage, target.transform.position);

        if (!string.IsNullOrEmpty(WeaponName))
            DamageTracker.instance.RecordDamage(WeaponName, Damage);

        TriggerHitEffects(hitSound);
    }

    private void HandleWallReflection(Collider2D wall)
    {
        if (isReflectingThisFrame) return; // 중복 반사 방지
        isReflectingThisFrame = true;

        Vector2 normalVector = GetWallNormal(wall);
        HandleReflection(normalVector, rb);
        TriggerHitEffects(reflectSound);

        currentReflections++;
        if (currentReflections >= maxReflections)
            DieProjectile();
    }

    private Vector2 GetWallNormal(Collider2D wall)
    {
        Vector2 closestPoint = wall.ClosestPoint(transform.position);
        return ((Vector2)transform.position - closestPoint).normalized;
    }

    private void TriggerHitEffects(AudioClip sound)
    {
        if (anim != null) anim.SetTrigger("Hit");
        if (sound != null) SoundManager.instance.PlaySoundWith(sound, 1f, false, 0.034f);
    }

    protected override void DieProjectile()
    {
        SoundManager.instance.Play(bowlingStrikeSound);
        currentReflections = 0;
        base.DieProjectile();
    }

    protected override void CastDamage()
    {
        // OnTriggerEnter2D로만 처리하므로 비움
    }
}