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
        if (Time.frameCount % 2 != 0) // 2프레임 간격으로 공격을 함
            return;

        SetDamageStats();
        CastDamage();
        SetLinePositions(transform.position, hoopWeapon.transform.position);
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
