using System.Collections.Generic;
using UnityEngine;

public class TennisWeapon : WeaponBase
{
    [SerializeField] GameObject weaponTennisBall; // ⭐ 동료 오리 폴백용
    [SerializeField] AudioClip shoot;

    [Header("Effects")]
    [SerializeField] GameObject muzzleFlash;

    // ⭐ 런타임에 결정되는 프로젝타일
    GameObject currentTennisBallPrefab;

    public override void Init(WeaponStats stats, bool isLead)
    {
        base.Init(stats, isLead);

        if (InitialWeapon)
        {
            Item equippedItem = GetEssentialEquippedItem();

            if (equippedItem != null && equippedItem.projectilePrefab != null)
            {
                currentTennisBallPrefab = equippedItem.projectilePrefab;
                Logger.Log($"[TennisWeapon] 리드 오리 - 장착 아이템 프로젝타일 사용: {equippedItem.Name}");
            }
            else
            {
                currentTennisBallPrefab = weaponTennisBall;
                Logger.LogWarning("[TennisWeapon] 리드 오리 - 장착된 프로젝타일이 없어서 기본값 사용");
            }
        }
        else
        {
            currentTennisBallPrefab = weaponTennisBall;
            Logger.Log("[TennisWeapon] 동료 오리 - 기본 프로젝타일 사용");
        }
    }

    protected override void Attack()
    {
        List<Vector2> closestEnemyPosition = EnemyFinder.instance.GetEnemies(1);
        if (closestEnemyPosition == null) return;
        if (closestEnemyPosition[0] == Vector2.zero) return;

        AttackCo();
    }

    void AttackCo()
    {
        AnimShoot();
        SoundManager.instance.Play(shoot);
        Transform muzzleEffect =
            GameManager.instance.poolManager.GetMisc(muzzleFlash).transform;
        muzzleEffect.transform.position = ShootPoint.position;

        for (int i = 0; i < weaponStats.numberOfAttacks; i++)
        {
            // ⭐ currentTennisBallPrefab 사용
            GameObject tennisBall = GameManager.instance.poolManager.GetMisc(currentTennisBallPrefab);
            if (tennisBall == null) return;
            
            tennisBall.transform.position = ShootPoint.position;

            float index = 0f;
            if (i == 0) index = 0;
            else if (i == 1) index = -15f;
            else if (i == 2) index = 15f;
            else if (i == 3) index = -30f;
            else if (i == 4) index = 30f;
            else if (i == 5) index = -45f;
            else if (i == 6) index = 45f;
            else if (i == 7) index = -60f;
            else if (i == 8) index = 60f;
            else if (i == 9) index = -75f;
            else if (i == 10) index = 75f;
            else if (i == 11) index = -90f;
            else if (i == 12) index = 90f;
            else if (i == 13) index = -105f;
            else if (i == 14) index = 105f;
            else if (i == 15) index = -120f;
            else if (i == 16) index = 120f;
            else if (i == 17) index = -135f;
            else if (i == 18) index = 135f;
            else if (i == 19) index = -150f;
            else if (i == 20) index = 150f;

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