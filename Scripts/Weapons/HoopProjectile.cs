using UnityEngine;

public class HoopProjectile : ProjectileBase
{
    protected override void Update()
    {
        if (Time.timeScale == 0)
            return;
        if (Time.frameCount % 30 != 0) // 30프레임 간격으로 공격을 함
            return;
        CastDamage();
    }

    protected override void HitObject()
    {
        GetComponentInParent<HoopWeapon>().TakeDamageProjectile();
    }
}
