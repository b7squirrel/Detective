using UnityEngine;

public class PlaneProjectile : ProjectileBase
{
    Vector3 target;
    [SerializeField] LayerMask targetLayer;
    [SerializeField] float speed = 10f;
    [SerializeField] float rotateSpeed = 600f;
    Vector3 offsetDirection;
    float sizeOfArea = 1f;

    // ✅ 캐싱: Awake에서 한 번만 GetComponent
    TrailRenderer trailRenderer;

    // ✅ NonAlloc용 static 버퍼
    static readonly Collider2D[] planeHitBuffer = new Collider2D[10];

    protected override void Awake()
    {
        base.Awake(); // ✅ hitEffects 캐싱
        trailRenderer = GetComponent<TrailRenderer>();
    }

    private void OnDisable()
    {
        // ✅ 캐싱된 trailRenderer 사용
        if (trailRenderer != null)
            trailRenderer.Clear();
    }

    protected override void Update()
    {
        if (Time.timeScale == 0f) return;
        AttackCoolTimer();
        ApplyMovement();

        if (Time.frameCount % 8 == 0)
        {
            CastDamage();
        }
    }

    protected override void ApplyMovement()
    {
        Vector3 directionToTarget = (target - transform.position).normalized;
        Vector3 currentDirection = transform.up;

        float angle = Vector3.SignedAngle(currentDirection, directionToTarget, Vector3.forward);
        float maxRotationThisFrame = rotateSpeed * Time.deltaTime;
        float rotationAmount = Mathf.Clamp(angle, -maxRotationThisFrame, maxRotationThisFrame);

        transform.Rotate(0, 0, rotationAmount);
        transform.position += transform.up * speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, target) < 1f)
        {
            CastDamage();
            DieProjectile();
        }
    }

    public void Init(Vector3 _target, int damage)
    {
        target = _target;

        float randomAngle = UnityEngine.Random.Range(-70f, 70f);
        offsetDirection = Quaternion.Euler(0, 0, randomAngle) * (target - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(Vector3.forward, offsetDirection);

        transform.SetParent(null);
        transform.localScale = 0.5f * Vector3.one;

        Damage = damage;
    }

    protected override void AttackCoolTimer()
    {
        TimeToLive -= Time.deltaTime;
        if (TimeToLive < 0f)
            DieProjectile();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CastDamage();
    }

    protected override void CastDamage()
    {
        if (Time.frameCount % 10 != 0) return;

        // ✅ NonAlloc으로 GC 방지
        int count = Physics2D.OverlapCircleNonAlloc(transform.position, sizeOfArea, planeHitBuffer, targetLayer);

        for (int i = 0; i < count; i++)
        {
            // ✅ GetComponent 중복 호출 제거
            Idamageable damageable = planeHitBuffer[i].GetComponent<Idamageable>();
            if (damageable == null) continue;

            if (planeHitBuffer[i].GetComponent<DestructableObject>() == null)
                PostMessage(Damage, planeHitBuffer[i].transform.position);

            // ✅ 캐싱된 hitEffects 사용
            GameObject hitEffect = hitEffects != null ? hitEffects.hitEffect : null;
            damageable.TakeDamage(
                Damage, KnockBackChance, KnockBackSpeedFactor,
                transform.position, hitEffect);

            if (!string.IsNullOrEmpty(WeaponName))
                DamageTracker.instance.RecordDamage(WeaponName, Damage);

            hitDetected = true;
        }
    }
}