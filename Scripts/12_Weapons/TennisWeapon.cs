// TennisWeapon.cs
using System.Collections.Generic;
using UnityEngine;

public class TennisWeapon : WeaponBase
{
    [SerializeField] GameObject weaponTennisBall;
    [SerializeField] AudioClip shoot;
    [Header("Effects")]
    [SerializeField] GameObject muzzleFlash;

    // ✅ 시너지 활성화 시 추가 반사 횟수
    private const int BASE_DEFLECTION = 3;
    private const int SYNERGY_DEFLECTION = 6; // 원하는 값으로 조정

    GameObject currentTennisBallPrefab;
    private List<Vector2> enemyQueryBuffer = new List<Vector2>(1);
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

        // ✅ 시너지 여부에 따라 반사 횟수 결정
        int deflectionCount = isSynergyWeaponActivated ? SYNERGY_DEFLECTION : BASE_DEFLECTION;

        for (int i = 0; i < weaponStats.numberOfAttacks; i++)
        {
            GameObject tennisBall = GameManager.instance.poolManager.GetMisc(currentTennisBallPrefab);
            if (tennisBall == null) return;

            tennisBall.transform.position = ShootPoint.position;

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

            // ✅ 발사할 때마다 deflection 명시적으로 초기화
            TennisBallProjectile tennisBallProj = tennisBall.GetComponent<TennisBallProjectile>();
            if (tennisBallProj != null)
            tennisBallProj.SetDeflection(deflectionCount); // ✅ 시너지 반영
        }
    }

    // ✅ WeaponBase의 ActivateSynergyWeapon 오버라이드
    public override void ActivateSynergyWeapon()
    {
        base.ActivateSynergyWeapon(); // isSynergyWeaponActivated = true
        Logger.Log("[TennisWeapon] 시너지 활성화: 반사 횟수 증가");
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