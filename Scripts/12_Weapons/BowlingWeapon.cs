using System.Collections.Generic;
using UnityEngine;

public class BowlingWeapon : WeaponBase
{
    [SerializeField] private GameObject bowlingBall; // 볼링공 프리팹
    [SerializeField] private AudioClip shootSound;
    
    private List<GameObject> activeBowlingBalls = new List<GameObject>(); // 활성화된 모든 볼링공 추적

    protected override void Update()
    {
        if (GameManager.instance.IsPaused) return;

        SetAngle();
        RotateWeapon();
        RotateExtraWeapon();

        FlipWeaponTools();
        LockFlip();

        // 활성화된 볼링공이 있는지 체크
        CleanupInactiveBalls(); // 비활성화된 볼링공 제거
        
        if (activeBowlingBalls.Count > 0)
        {
            return; // 볼링공이 하나라도 활성화되어 있으면 타이머 스킵
        }

        // 모든 볼링공이 사라지면 타이머 감소 시작
        timer -= Time.deltaTime;
        if (timer < 0f)
        {
            Attack();
            timer = weaponStats.timeToAttack;
        }
    }

    // 비활성화된 볼링공을 리스트에서 제거
    private void CleanupInactiveBalls()
    {
        activeBowlingBalls.RemoveAll(ball => ball == null || !ball.activeInHierarchy);
    }

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

        // 기본 볼링공 발사
        ShootBall(dir);

        // 시너지 활성화 시 반대 방향으로 추가 볼링공 발사
        if (isSynergyWeaponActivated)
        {
            ShootBall(-dir);
        }
    }

    private void ShootBall(Vector2 direction)
    {
        // 볼링공 생성
        GameObject ball = GameManager.instance.poolManager.GetMisc(bowlingBall);
        if (ball == null)
        {
            Debug.LogWarning("볼링공을 풀에서 가져올 수 없습니다!");
            return;
        }

        // 활성화된 볼링공 리스트에 추가
        activeBowlingBalls.Add(ball);

        ball.transform.position = transform.position;

        // 발사체 설정
        ProjectileBase projectile = ball.GetComponent<ProjectileBase>();
        projectile.Speed = weaponStats.projectileSpeed;
        projectile.Direction = direction; // 파라미터로 받은 방향 사용
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