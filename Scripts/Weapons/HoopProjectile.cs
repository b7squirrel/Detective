using UnityEngine;

public class HoopProjectile : ProjectileBase
{
    [SerializeField] HoopWeapon hoopWeapon;
    public void Init(HoopWeapon hoopWeapon)
    {
        if(this.hoopWeapon == null)
        this.hoopWeapon = hoopWeapon;
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
        if (Time.frameCount % 30 != 0) // 30프레임 간격으로 공격을 함
            return;

        SetDamageStats();
        CastDamage();
    }

    protected override void HitObject()
    {
        GetComponentInParent<HoopWeapon>().TakeDamageProjectile();
    }
}
