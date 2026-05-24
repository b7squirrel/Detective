using UnityEngine;

public class BowlingProjectile : ProjectileBase
{
    [SerializeField] private int maxReflections = 5;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip reflectSound;
    [SerializeField] private AudioClip bowlingStrikeSouind;
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private Transform spriteTransform;

    private Rigidbody2D rb;
    private Animator anim;
    private TrailRenderer trailRenderer;
    private int currentReflections = 0;

    protected override void Awake()
    {
        base.Awake(); // ✅ hitEffects 캐싱

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        trailRenderer = GetComponent<TrailRenderer>();
    }

    private void OnEnable()
    {
        currentReflections = 0;
    }

    private void OnDisable()
    {
        if (trailRenderer != null)
            trailRenderer.Clear();
    }

    protected override void Update()
    {
        base.Update();

        if (spriteTransform != null && Mathf.Abs(Direction.x) > 0.01f)
        {
            float rotationDirection = -Mathf.Sign(Direction.x);
            spriteTransform.Rotate(0, 0, rotationDirection * rotationSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ✅ 캐싱된 hitEffects 사용
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
        SoundManager.instance.Play(bowlingStrikeSouind);
        currentReflections = 0;
        base.DieProjectile();
    }

    protected override void CastDamage()
    {
        // 트리거로만 처리하므로 비움
    }
}