using UnityEngine;

public class BombProjectile : ProjectileBase
{
    public Vector2 GroundVelocity { get; private set; }
    float sizeOfArea;
    [SerializeField] LayerMask target;
    [SerializeField] GameObject hitEffect;   // 폭발 이펙트
    [SerializeField] GameObject starEffect;  // 폭발 이펙트 - 별
    [SerializeField] AudioClip hitSFX;

    // ✅ 캐싱: Awake에서 한 번만 GetComponent
    TrailRenderer trailRenderer;

    // ✅ NonAlloc용 버퍼
    readonly Collider2D[] explodeHitBuffer = new Collider2D[20];

    protected override void Awake()
    {
        base.Awake(); // ✅ hitEffects 캐싱

        // ✅ Awake에서 캐싱 (OnEnable/Explode에서 반복 GetComponent 제거)
        trailRenderer = GetComponentInChildren<TrailRenderer>();
    }

    void OnEnable()
    {
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
            trailRenderer.enabled = false;
        }
    }

    protected override void Update()
    {
        ApplyMovement();
    }

    protected override void ApplyMovement()
    {
        transform.position += (Vector3)GroundVelocity * Time.deltaTime;
    }

    public void Explode()
    {
        GenerateHitEffect();
        SoundManager.instance.Play(hitSFX);
        CastDamage();

        // ✅ 캐싱된 trailRenderer 사용
        if (trailRenderer != null)
            trailRenderer.enabled = false;

        gameObject.SetActive(false);
    }

    protected override void CastDamage()
    {
        // ✅ NonAlloc으로 GC 방지
        int count = Physics2D.OverlapCircleNonAlloc(transform.position, sizeOfArea, explodeHitBuffer, target);

        for (int i = 0; i < count; i++)
        {
            // ✅ GetComponent 중복 제거
            Idamageable damageable = explodeHitBuffer[i].GetComponent<Idamageable>();
            if (damageable == null) continue;

            if (explodeHitBuffer[i].GetComponent<DestructableObject>() == null)
                PostMessage(Damage, explodeHitBuffer[i].transform.position);

            // ✅ 캐싱된 hitEffects 사용
            GameObject hitEffectObj = hitEffects != null ? hitEffects.hitEffect : null;
            damageable.TakeDamage(
                Damage, KnockBackChance, KnockBackSpeedFactor,
                transform.position, hitEffectObj);

            if (!string.IsNullOrEmpty(WeaponName))
                DamageTracker.instance.RecordDamage(WeaponName, Damage);

            hitDetected = true;
        }
    }

    public void Init(Vector2 target, WeaponStats weaponStats)
    {
        Speed = weaponStats.projectileSpeed;
        sizeOfArea = weaponStats.sizeOfArea;

        Vector2 dir = (target - (Vector2)transform.position).normalized;
        GroundVelocity = dir * Speed;

        if (trailRenderer != null)
            trailRenderer.enabled = true;
    }

    void GenerateHitEffect()
    {
        GameObject smoke = GameManager.instance.poolManager.GetMisc(hitEffect);
        smoke.transform.position = transform.position;
        smoke.transform.localScale = Vector2.one * sizeOfArea;

        GameObject stars = GameManager.instance.poolManager.GetMisc(starEffect);
        stars.transform.position = transform.position;
        stars.transform.localScale = Vector2.one * sizeOfArea;
    }
}