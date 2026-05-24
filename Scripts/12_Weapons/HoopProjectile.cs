using UnityEngine;

public class HoopProjectile : ProjectileBase
{
    [SerializeField] HoopWeapon hoopWeapon;
    LineRenderer lr;

    // ✅ HoopWeapon 캐싱 (GetComponentInParent 반복 호출 제거)
    HoopWeapon parentHoopWeapon;

    protected override void Awake()
    {
        base.Awake(); // ✅ ProjectileBase.Awake() 호출 → hitEffects 캐싱
    }

    public void Init(HoopWeapon hoopWeapon)
    {
        if (this.hoopWeapon == null)
            this.hoopWeapon = hoopWeapon;

        lr = GetComponent<LineRenderer>();
        SetLinePositions(transform.position, hoopWeapon.transform.position);

        // ✅ HitObject에서 매번 GetComponentInParent 하지 않도록 캐싱
        parentHoopWeapon = GetComponentInParent<HoopWeapon>();
    }

    void SetDamageStats()
    {
        Damage = hoopWeapon.GetDamage();
        KnockBackChance = hoopWeapon.GetKnockBackChance();
        IsCriticalDamageProj = hoopWeapon.CheckIsCriticalDamage();
    }

    protected override void Update()
    {
        if (hoopWeapon == null) return;
        if (Time.timeScale == 0) return;
        if (Time.frameCount % 3 != 0) return; // 3프레임 간격으로 공격

        SetDamageStats();
        CastDamage();
        SetLinePositions(transform.position, hoopWeapon.transform.position);
    }

    protected override void CastDamage()
    {
        // ✅ NonAlloc으로 GC 방지 (부모의 static 버퍼 재사용)
        int count = Physics2D.OverlapCircleNonAlloc(transform.position, .7f, overlapBuffer);

        for (int i = 0; i < count; i++)
        {
            // ✅ GetComponent 중복 호출 제거
            Idamageable damageable = overlapBuffer[i].GetComponent<Idamageable>();
            if (damageable == null) continue;

            if (overlapBuffer[i].GetComponent<DestructableObject>() == null)
                PostMessage(Damage, overlapBuffer[i].transform.position);

            // ✅ 캐싱된 hitEffects 사용
            GameObject hitEffect = hitEffects != null ? hitEffects.hitEffect : null;
            damageable.TakeDamage(
                Damage, KnockBackChance, KnockBackSpeedFactor,
                transform.position, hitEffect);

            if (!string.IsNullOrEmpty(WeaponName))
                DamageTracker.instance.RecordDamage(WeaponName, Damage);

            hitDetected = true;
            break;
        }

        if (hitDetected)
        {
            HitObject();
            hitDetected = false;
        }
    }

    protected override void HitObject()
    {
        // ✅ 캐싱된 참조 사용 (매 히트마다 GetComponentInParent 제거)
        if (parentHoopWeapon != null)
            parentHoopWeapon.TakeDamageProjectile();
        else
            GetComponentInParent<HoopWeapon>()?.TakeDamageProjectile(); // 폴백
    }

    void SetLinePositions(Vector2 _startPos, Vector2 _endPos)
    {
        lr.SetPosition(0, _startPos);
        lr.SetPosition(1, _endPos);
    }
}