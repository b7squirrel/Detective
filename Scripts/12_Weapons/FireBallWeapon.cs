using System.Collections.Generic;
using UnityEngine;

public class FireBallWeapon : WeaponBase
{
    [SerializeField] GameObject weapon;
    [SerializeField] AudioClip shoot;

    [Header("Effects")]
    [SerializeField] GameObject muzzleFlash;

    // 런타임에 결정되는 프로젝타일
    GameObject currentWeaponPrefab;

    // Attack에서 매번 new List 하지 않도록 필드로 캐싱
    private List<Vector2> enemyQueryBuffer = new List<Vector2>(1);

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