using System.Collections.Generic;
using UnityEngine;

public class TennisWeapon : WeaponBase
{
    [SerializeField] GameObject weaponTennisBall; // 동료 오리 폴백용
    [SerializeField] AudioClip shoot;

    [Header("Effects")]
    [SerializeField] GameObject muzzleFlash;

    // 런타임에 결정되는 프로젝타일
    GameObject currentTennisBallPrefab;

    // Attack에서 매번 new List 하지 않도록 필드로 캐싱
    private List<Vector2> enemyQueryBuffer = new List<Vector2>(1);

    // if-else 체인 대신 배열로 관리 (인덱스 → 각도)
    private static readonly float[] angleOffsets =
    {
        0f, -15f, 15f, -30f, 30f, -45f, 45f, -60f, 60f,
        -75f, 75f, -90f, 90f, -105f, 105f, -120f, 120f,
        -135f, 135f, -150f, 150f
    };

    public override void Init(WeaponStats stats, bool isLead)
    {
        base.Init(stats, isLead);
    }

    protected override void OnWeaponDataReady()
    {
        Item equippedItem = GetEssentialEquippedItem();
        if (equippedItem != null && equippedItem.projectilePrefab != null)
        {
            currentTennisBallPrefab = equippedItem.projectilePrefab;
        }
        else
        {
            currentTennisBallPrefab = weaponTennisBall;
            Logger.LogWarning("[TennisWeapon] 기본값 사용");
        }
    }

    protected override void Attack()
    {
        base.Attack();

        // 버퍼 재사용으로 new List 방지
        EnemyFinder.instance.GetEnemies(1, enemyQueryBuffer);
        if (enemyQueryBuffer.Count == 0 || enemyQueryBuffer[0] == Vector2.zero)
            return;

        AttackCo();
    }

    void AttackCo()
    {
        AnimShoot();
        SoundManager.instance.Play(shoot);

        Transform muzzleEffect = GameManager.instance.poolManager.GetMisc(muzzleFlash).transform;
        muzzleEffect.transform.position = ShootPoint.position;

        for (int i = 0; i < weaponStats.numberOfAttacks; i++)
        {
            GameObject tennisBall = GameManager.instance.poolManager.GetMisc(currentTennisBallPrefab);
            if (tennisBall == null) return;

            tennisBall.transform.position = ShootPoint.position;

            // 배열로 각도 조회 (if-else 체인 제거)
            float index = i < angleOffsets.Length ? angleOffsets[i] : 0f;

            Vector3 direction = Quaternion.AngleAxis(index, Vector3.forward) * dir;
            ProjectileBase projectile = tennisBall.GetComponent<ProjectileBase>();
            projectile.Speed = weaponStats.projectileSpeed;
            projectile.Direction = direction;
            projectile.Damage = GetDamage();
            projectile.IsCriticalDamageProj = isCriticalDamage;
            projectile.KnockBackChance = GetKnockBackChance();
            projectile.TimeToLive = 1.5f;
            projectile.WeaponName = weaponData.DisplayName;
        }
    }

    protected override void FlipWeaponTools()
    {
        if (weaponTools == null) return;

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