using System.Collections.Generic;
using UnityEngine;

public class FireBallWeapon : WeaponBase
{
    [SerializeField] GameObject weapon;
    [SerializeField] AudioClip shoot;

    [Header("Effects")]
    [SerializeField] GameObject muzzleFlash;

    [Header("FireBall 전용 감지 범위")]
    [SerializeField] float fireballDetectRange = 8f;

    [Header("대기 자세 각도")]
    [SerializeField] float idleAngleMain = 30f;   // 2시 방향
    [SerializeField] float idleAngleExtra = 150f; // 10시 방향
    [SerializeField] float aimHoldDuration = 0.3f; // 공격 후 조준 각도 유지 시간
    float lastAttackTime = -999f;

    // 런타임에 결정되는 프로젝타일
    GameObject currentWeaponPrefab;

    // 대기 방향값 캐싱 (매 프레임 삼각함수 재계산 방지)
    Vector2 idleDirMain;
    Vector2 idleDirExtra;

    protected override void Awake()
    {
        base.Awake();
        idleDirMain = new Vector2(Mathf.Cos(idleAngleMain * Mathf.Deg2Rad), Mathf.Sin(idleAngleMain * Mathf.Deg2Rad));
        idleDirExtra = new Vector2(Mathf.Cos(idleAngleExtra * Mathf.Deg2Rad), Mathf.Sin(idleAngleExtra * Mathf.Deg2Rad));
    }

    public override void Init(WeaponStats stats, bool isLead)
    {
        base.Init(stats, isLead);
    }

    protected override void OnWeaponDataReady()
    {
        Item equippedItem = GetEssentialEquippedItem();
        if (equippedItem != null && equippedItem.projectilePrefab != null)
        {
            currentWeaponPrefab = equippedItem.projectilePrefab;
            Logger.Log($"[FireBallWeapon] 프로젝타일 사용: {equippedItem.Name} / IsLead: {InitialWeapon}");
        }
        else
        {
            currentWeaponPrefab = weapon;
            Logger.LogWarning("[FireBallWeapon] 기본값 사용");
        }
    }

    protected override void Update()
    {
        if (GameManager.instance.IsPaused) return;

        timer -= Time.deltaTime;
        bool willAttack = timer < 0f;

        if (willAttack)
        {
            EnemyFinder.instance.GetEnemiesInRange(2, fireballDetectRange, angleQueryBuffer);
            UpdateAimFromBuffer();
            lastAttackTime = Time.time; // 공격 시점 기록
        }
        else if (Time.time - lastAttackTime >= aimHoldDuration)
        {
            // 공격 후 유지 시간이 지났을 때만 대기 각도로 복귀
            SetIdleAngle();
        }
        // else: 유지 시간 중이면 아무것도 안 함 → 방금 조준했던 각도 그대로 유지

        RotateWeapon();
        RotateExtraWeapon();

        FlipWeaponTools();
        LockFlip();

        if (willAttack)
        {
            Attack();
            timer = weaponStats.timeToAttack;
        }
    }

    void SetIdleAngle()
    {
        angle = idleAngleMain;
        dir = idleDirMain;
        angleExtra = idleAngleExtra;
        dirExtra = idleDirExtra;
    }

    void UpdateAimFromBuffer()
    {
        bool hasMainTarget = angleQueryBuffer.Count > 0 && angleQueryBuffer[0] != Vector2.zero;
        if (hasMainTarget)
        {
            dir = GetDirection(angleQueryBuffer[0]);
            angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        }
        else
        {
            angle = idleAngleMain;
            dir = idleDirMain;
        }

        bool hasExtraTarget = angleQueryBuffer.Count > 1 && angleQueryBuffer[1] != Vector2.zero;
        if (hasExtraTarget)
        {
            dirExtra = GetDirection(angleQueryBuffer[1]);
            angleExtra = Mathf.Atan2(dirExtra.y, dirExtra.x) * Mathf.Rad2Deg;
        }
        else
        {
            angleExtra = idleAngleExtra;
            dirExtra = idleDirExtra;
        }
    }

    protected override void Attack()
    {
        base.Attack();

        // 같은 프레임에 이미 갱신된 angleQueryBuffer를 재사용
        bool hasTarget = angleQueryBuffer.Count > 0 && angleQueryBuffer[0] != Vector2.zero;
        if (!hasTarget)
            return;

        AttackCo();
    }

    void AttackCo()
    {
        for (int i = 0; i < weaponStats.numberOfAttacks; i++)
        {
            AnimShoot();
            GetAttackParameters(); // 총알마다 크리티컬 확률, 넉백 확률이 다르게 하기 위해
            SoundManager.instance.Play(shoot);

            GameObject fireBall = GameManager.instance.poolManager.GetMisc(currentWeaponPrefab);
            float index = 0f;
            if (i == 0)
            {
                index = 0;
            }
            else if (i == 1)
            {
                index = -15f;
            }
            else if (i == 2)
            {
                index = 15f;
            }

            if (fireBall != null)
            {
                Vector3 direction = Quaternion.AngleAxis(index, Vector3.forward) * dir;
                fireBall.transform.position = transform.position;
                fireBall.transform.rotation = Quaternion.FromToRotation(Vector2.up, direction);
                ProjectileBase projectile = fireBall.GetComponent<ProjectileBase>();
                projectile.Direction = direction;
                projectile.Speed = weaponStats.projectileSpeed;
                projectile.Damage = GetDamage();
                projectile.KnockBackChance = GetKnockBackChance();
                projectile.IsCriticalDamageProj = isCriticalDamage;
                projectile.WeaponName = weaponData.DisplayName;
            }

            if (isSynergyWeaponActivated)
            {
                AnimShootExtra();
                SoundManager.instance.Play(shoot);

                GameObject fireBallEx = GameManager.instance.poolManager.GetMisc(currentWeaponPrefab);
                if (fireBallEx != null)
                {
                    Vector3 directionExtra = Quaternion.AngleAxis(index, Vector3.forward) * dirExtra;
                    fireBallEx.transform.position = transform.position;
                    fireBallEx.transform.rotation = Quaternion.FromToRotation(Vector2.up, directionExtra);
                    ProjectileBase projectileEx = fireBallEx.GetComponent<ProjectileBase>();
                    projectileEx.Direction = directionExtra;
                    projectileEx.Speed = weaponStats.projectileSpeed;
                    projectileEx.Damage = GetDamage();
                    projectileEx.KnockBackChance = GetKnockBackChance();
                    projectileEx.IsCriticalDamageProj = isCriticalDamage;
                    projectileEx.WeaponName = weaponData.DisplayName;
                }
            }
        }
    }

    public override void ActivateSynergyWeapon()
    {
        base.ActivateSynergyWeapon();
        Player.instance.GetComponent<WeaponManager>().AddExtraWeaponTool(weaponData, this, 1);
    }
}