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

    PlaneWeapon owner;

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
        Vector2 toTarget2D = (Vector2)target - (Vector2)transform.position;

        if (!IsFinite(toTarget2D) || toTarget2D.sqrMagnitude < 0.0001f)
        {
            CastDamage();
            DieProjectile();
            return;
        }

        Vector2 directionToTarget = toTarget2D.normalized;
        Vector2 currentDirection = transform.up;

        float angle = Vector2.SignedAngle(currentDirection, directionToTarget);
        float maxRotationThisFrame = rotateSpeed * Time.deltaTime;
        float rotationAmount = Mathf.Clamp(angle, -maxRotationThisFrame, maxRotationThisFrame);

        transform.Rotate(0, 0, rotationAmount);
        transform.position += transform.up * speed * Time.deltaTime;

        if (toTarget2D.magnitude < 1f)
        {
            CastDamage();
            DieProjectile();
        }
    }

    public void Init(Vector3 _target, int damage, PlaneWeapon _owner)
    {
        owner = _owner;

        Vector2 toTarget2D = (Vector2)_target - (Vector2)transform.position;

        // ✅ NaN/Infinity를 직접 검사 (sqrMagnitude 임계값보다 확실함)
        bool isValid = IsFinite(toTarget2D) && toTarget2D.sqrMagnitude > 0.0001f;
        target = isValid ? _target : (Vector3)((Vector2)transform.position + Vector2.up);

        Vector2 baseDirection2D = isValid ? toTarget2D.normalized : Vector2.up;

        float randomAngle = UnityEngine.Random.Range(-70f, 70f);
        Vector2 offsetDirection2D = Quaternion.Euler(0, 0, randomAngle) * baseDirection2D;
        offsetDirection = offsetDirection2D;

        float startAngle = Mathf.Atan2(offsetDirection2D.y, offsetDirection2D.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, startAngle);

        transform.SetParent(null);
        transform.localScale = 0.5f * Vector3.one;
        Damage = damage;
    }

    // ✅ 벡터에 NaN이나 Infinity가 섞여 있는지 검사하는 헬퍼
    static bool IsFinite(Vector2 v)
    {
        return !float.IsNaN(v.x) && !float.IsNaN(v.y)
            && !float.IsInfinity(v.x) && !float.IsInfinity(v.y);
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

            owner?.OnProjectileHit(Damage);

            hitDetected = true;
        }
    }
}