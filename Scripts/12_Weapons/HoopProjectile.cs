using UnityEngine;

public class HoopProjectile : ProjectileBase
{
    [SerializeField] HoopWeapon hoopWeapon;
    LineRenderer lr;
    public void Init(HoopWeapon hoopWeapon)
    {
        if(this.hoopWeapon == null)
        this.hoopWeapon = hoopWeapon;

        lr = GetComponent<LineRenderer>();
        SetLinePositions(transform.position, hoopWeapon.transform.position);
    }
    void SetDamageStats()
    {
        Damage = hoopWeapon.GetDamage();
        KnockBackChance = hoopWeapon.GetKnockBackChance();
        IsCriticalDamageProj = hoopWeapon.CheckIsCriticalDamage();
    }

    protected override void Update()
    {
        if(hoopWeapon == null)
            return;

        if (Time.timeScale == 0)
            return;
        if (Time.frameCount % 3 != 0) // 2프레임 간격으로 공격을 함
            return;

        SetDamageStats();
        CastDamage();
        SetLinePositions(transform.position, hoopWeapon.transform.position);
    }

    protected override void CastDamage()
    {
        Collider2D[] hit = Physics2D.OverlapCircleAll(transform.position, .7f);
        for (int i = 0; i < hit.Length; i++)
        {
            Transform enmey = hit[i].GetComponent<Transform>();
            if (enmey.GetComponent<Idamageable>() != null)
            {
                if (enmey.GetComponent<DestructableObject>() == null)
                    PostMessage(Damage, enmey.transform.position);

                GameObject hitEffect = GetComponent<HitEffects>().hitEffect;
                enmey.GetComponent<Idamageable>().TakeDamage(Damage, KnockBackChance, KnockBackSpeedFactor, transform.position, hitEffect);

                // ✨ HoopWeapon 이름으로 데미지 기록
                // ✨ WeaponName 프로퍼티 사용
                if (!string.IsNullOrEmpty(WeaponName))
                {
                    DamageTracker.instance.RecordDamage(WeaponName, Damage);
                }

                hitDetected = true;
                break;
            }
        }

        if (hitDetected == true)
        {
            HitObject();
            hitDetected = false;
        }
    }

    protected override void HitObject()
    {
        GetComponentInParent<HoopWeapon>().TakeDamageProjectile();
    }

    void SetLinePositions(Vector2 _startPos, Vector2 _endPos)
    {
        lr.SetPosition(0, _startPos);
        lr.SetPosition(1, _endPos);
    }
}
