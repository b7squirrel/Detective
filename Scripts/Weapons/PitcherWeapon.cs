using UnityEngine;

public class PitcherWeapon : WeaponBase
{
    [SerializeField] GameObject weaponBall;  // 야구공 프리팹
    [SerializeField] AudioClip shoot;
    
    [Header("Effects")]
    [SerializeField] GameObject muzzleFlash;

    protected override void Attack()
    {
        // WeaponBase의 SetAngle()에서 이미 dir이 계산되어 있음
        // dir이 유효한지만 확인
        if (dir == Vector2.zero)
        {
            Debug.Log("투구 대상 없음");
            return;
        }

        Debug.Log($"투구! 방향 = {dir}");
        ThrowBall();
    }

    void ThrowBall()
    {
        // 애니메이션 및 사운드
        AnimShoot();
        SoundManager.instance.Play(shoot);

        // 머즐 이펙트
        Transform muzzleEffect = GameManager.instance.poolManager.GetMisc(muzzleFlash).transform;
        muzzleEffect.transform.position = ShootPoint.position;

        // 공 생성 (단 하나만!)
        GameObject ball = GameManager.instance.poolManager.GetMisc(weaponBall);
        if (ball == null) 
        {
            Debug.LogWarning("공을 풀에서 가져올 수 없습니다!");
            return;
        }
        
        ball.transform.position = ShootPoint.position;

        // 발사체 설정
        ProjectileBase projectile = ball.GetComponent<ProjectileBase>();
        projectile.Speed = weaponStats.projectileSpeed;
        projectile.Direction = dir;  // WeaponBase에서 계산된 방향 사용
        projectile.Damage = GetDamage();
        projectile.IsCriticalDamageProj = isCriticalDamage;
        projectile.KnockBackChance = GetKnockBackChance();
        projectile.TimeToLive = 1.5f;  // 필요시 조정

        Debug.Log($"야구공 발사 - 데미지: {projectile.Damage}, 속도: {projectile.Speed}");
    }

    protected override void FlipWeaponTools()
    {
        if (weaponTools == null)
            return;

        if (flip)
        {
            weaponTools.GetComponent<Transform>().transform.eulerAngles = new Vector3(0, 180f, 0);
        }
        else
        {
            weaponTools.GetComponent<Transform>().transform.eulerAngles = new Vector3(0, 0, 0);
        }
    }
}