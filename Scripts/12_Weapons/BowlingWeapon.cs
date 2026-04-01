using System.Collections.Generic;
using UnityEngine;

public class BowlingWeapon : WeaponBase
{
    [SerializeField] private GameObject bowlingBall; // 볼링공 프리팹 (폴백)
    [SerializeField] private AudioClip shootSound;
    Vector2 lastValidDir = Vector2.right; // 마지막으로 유효했던 방향 기억

    // ⭐ 런타임에 결정되는 프로젝타일
    GameObject currentBowlingBallPrefab;

    private List<GameObject> activeBowlingBalls = new List<GameObject>(); // 활성화된 모든 볼링공 추적

    public override void Init(WeaponStats stats, bool isLead)
    {
        base.Init(stats, isLead);
    }

    protected override void OnWeaponDataReady()
    {
        Item equippedItem = GetEssentialEquippedItem();

        // ⭐ 어느 조건에서 기본값을 사용하는지 확인
        if (equippedItem == null)
        {
            Logger.LogWarning("[BowlingWeapon] equippedItem이 null입니다!");
        }
        else if (equippedItem.projectilePrefab == null)
        {
            Logger.LogWarning($"[BowlingWeapon] {equippedItem.Name}의 projectilePrefab이 null입니다! Item SO에서 연결을 확인하세요.");
        }

        if (equippedItem != null && equippedItem.projectilePrefab != null)
        {
            currentBowlingBallPrefab = equippedItem.projectilePrefab;
            Logger.Log($"[BowlingWeapon] 아이템: {equippedItem.Name} / 프리팹: {currentBowlingBallPrefab.name} / IsLead: {InitialWeapon}");
        }
        else
        {
            currentBowlingBallPrefab = bowlingBall;
            Logger.LogWarning("[BowlingWeapon] 기본값 사용");
        }

        // ⭐ 브릿지 등록
        if (weaponTools != null)
        {
            WeaponAnimEventBridge bridge = weaponTools.GetComponentInChildren<WeaponAnimEventBridge>();
            if (bridge != null)
                bridge.SetWeapon(this);
            else
                Debug.LogWarning("[BowlingWeapon] Bridge를 찾을 수 없습니다!");
        }
    }

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
        if(dir != Vector2.zero) lastValidDir = dir; // 방향 저장
        
        dir = lastValidDir; // 항상 유효한 방향 보장 
        
        GetAttackParameters(); // ⭐ 캐싱
        AnimShoot();           // ⭐ 애니메이션만 재생
    }

    // ⭐ Animation Event → Bridge → 여기 호출
    public override void OnAnimEvent()
    {
        ThrowBowlingBall();
    }

    private void ThrowBowlingBall()
    {
        // 사운드
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
        if (currentBowlingBallPrefab == null)
        {
            Logger.LogError("[BowlingWeapon] currentBowlingBallPrefab이 null입니다!");
            return;
        }

        // ⭐ currentBowlingBallPrefab 사용
        GameObject ball = GameManager.instance.poolManager.GetMisc(currentBowlingBallPrefab);
        if (ball == null)
        {
            Debug.LogWarning("볼링공을 풀에서 가져올 수 없습니다!");
            return;
        }

        // 활성화된 볼링공 리스트에 추가
        activeBowlingBalls.Add(ball);
        ball.transform.position = transform.position;
        ball.transform.localScale = Vector3.one * weaponStats.sizeOfArea;

        // 발사체 설정
        ProjectileBase projectile = ball.GetComponent<ProjectileBase>();
        projectile.Speed = weaponStats.projectileSpeed;
        projectile.Direction = direction; // 파라미터로 받은 방향 사용
        projectile.Damage = damage;                    // ⭐ 캐싱된 값
        projectile.IsCriticalDamageProj = isCriticalDamage;
        projectile.KnockBackChance = knockback;        // ⭐ 캐싱된 값
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