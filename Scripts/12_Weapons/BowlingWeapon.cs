using UnityEngine;

public class BowlingWeapon : WeaponBase
{
    [SerializeField] private GameObject bowlingBall; // 볼링공 프리팹
    [SerializeField] private AudioClip shootSound;
    
    protected override void Attack()
    {
        if (dir == Vector2.zero)
        {
            Debug.Log("볼링공 투구 대상 없음");
            return;
        }
        
        ThrowBowlingBall();
    }
    
    private void ThrowBowlingBall()
    {
        // 애니메이션 및 사운드
        AnimShoot();
        SoundManager.instance.Play(shootSound);
        
        // 볼링공 생성
        GameObject ball = GameManager.instance.poolManager.GetMisc(bowlingBall);
        if (ball == null)
        {
            Debug.LogWarning("볼링공을 풀에서 가져올 수 없습니다!");
            return;
        }
        
        ball.transform.position = transform.position;
        
        //발사체 설정
        ProjectileBase projectile = ball.GetComponent<ProjectileBase>();
        projectile.Speed = weaponStats.projectileSpeed;
        projectile.Direction = dir;
        projectile.Damage = GetDamage();
        projectile.IsCriticalDamageProj = isCriticalDamage;
        projectile.KnockBackChance = GetKnockBackChance();
        projectile.TimeToLive = 5f; // 볼링공은 더 오래 지속
        projectile.WeaponName = weaponData.DisplayName;
    }
    
    protected override void FlipWeaponTools()
    {
        if (weaponTools == null)
            return;
            
        if (flip)
        {
            weaponTools.transform.eulerAngles = new Vector3(0, 180f, 0);
        }
        else
        {
            weaponTools.transform.eulerAngles = new Vector3(0, 0, 0);
        }
    }
}